/* seralize the scene */
function serializeScene() {
  // https://stackoverflow.com/questions/17607438/can-we-export-a-jsplumb-flowchart-as-a-json-or-xml
  var blocks = []
  $("#canvas .window").each(function (idx, elem) {
    var id = $(elem).attr('id')
    if(id in classNodes) {
      block = classNodes[id].serialize();
      blocks.push(block);
    }
  });

  var connections = [];
  $.each(inst.getAllConnections(), function (idx, connection) {
    /* add a connection/edge to the save */
    getId = id => {
      elem = $(`#${id}`);
      nodeId = elem.closest('.window').attr('id');
      eid = elem.data('eid');
      return `${nodeId}-${eid}`;
    }
    sourceId = getId(connection.sourceId);
    targetId = getId(connection.targetId);
    connections.push({
      source: sourceId,
      target: targetId,
    });
  });

  return {
    nodes: blocks,
    connectors: connections
  }
}

/* saves a scene */
function saveScene() {
  if(socket.id == null) { return; }
  data = {
    clientId: socket.id,
    scene: serializeScene()
  }

  /* send the save to the server */
  $.ajax({
    type: "POST",
    url: '/edit/save/' + pid,
    data: JSON.stringify(data),
    contentType: "application/json",
    dataType: "json",
  });
}

/* loads a scene */
function loadScene(scene, delete_current=true) {
  inst.batch(function () {
    if(delete_current) {
      // delete old nodes
      inst.getAllConnections().forEach(c => c.setData({save: false}));
      inst.deleteEveryEndpoint({fireEvent: false})
        .deleteEveryConnection({fireEvent: false});
      $("#canvas .window").each(function (idx, elem) {
        var id = $(elem).attr('id')
        if (id in classNodes) {
          classNodes[id].remove();
        }
      })
    }

    if(scene !== null) {
      /* load nodes */
      if(scene.hasOwnProperty('nodes')) {
        scene["nodes"].forEach(function(o) {
          addNode(o, true); 
        });
      }
      /* load connectors */
      if(scene.hasOwnProperty('connectors')) {
        scene["connectors"].forEach(function(o) { 
          addConnector(o); 
        });
      }
    }
  });
}

/* click on delete everything */
$("#delete-scene").click(e => {
  $.confirm({
    animation: 'none',
    title: 'Delete',
    content: 'Are you sure you want to delete everything?',
    buttons: {
      yes: {
        action : () => { 
          selectAll();
          _deleteSelected();
        },
        btnClass: "btn-danger",
        keys: ['enter'],
      },
      no: function () {},
    }
  });
})

/* load db blocks to the submenu */
function loadBlocks() {
  for (const key in blocks) {
    if (blocks.hasOwnProperty(key)) {
      var b = blocks[key];
      addBlockSubmenu(b, true)
    }
  }
  saveScene();
}
$(function () { loadBlocks(); })

/* add a submenu item to the building blocks menu */
function addBlockSubmenu(block, load) {
  load = load || false;
  var id = block['id']; var name = block['name'];

  loadable = `<span>${name}</span>`;
  deleteable = !(isNaN(parseFloat(id))) ? `<span onclick="deleteBlock('${id}')" class="pull-right btn btn-dark btn-sm delete-block"><i class="material-icons md-14">close</i></span>` : '';
  fn = load ? 'prepend' : 'append';
  $('#block-submenu')[fn]( 
    `<li id="block-${id}">
      <a id="load-${id}">
        ${loadable}
        ${deleteable}        
      </a>
    </li>` 
  );

  $(`#load-${id}`).click(e => {
    nids = loadBlock(blocks[id].data);
    selectNodes(nids)
  });
}

/* load a block to the scene */
function loadBlock(block) {
  var nids = [];
  inst.batch(function () {
    block.block.forEach(e => {
      delete e.id
      e.connection_loaded = e.connection_loaded || false;
      var nid = addNode(e, true)
      nids.push(nid);
      /* prepare connectors */
      if(!e.connection_loaded) {
        block.connections.forEach(c => {
          ssplit = c.source.split('-');
          csid = ssplit[0];
          if(csid == e.oid) { c.source = [nid, ssplit[1]].join('-') }
          
          stplit = c.target.split('-');
          ctid = stplit[0];
          if(ctid == e.oid) { c.target = [nid, stplit[1]].join('-') }
        });
        e.connection_loaded = true;
      }
    });

    block.connections.forEach(c => addConnector(c));
    saveScene();
  });
  return nids;
}

/* delete a block in the db */
function deleteBlock(id) {
  $.confirm({
    animation: 'none',
    title: 'Delete',
    content: 'Are you sure you want to delete this block?',
    buttons: {
      yes: {
        action : () => { 
          delete blocks[id];
          $(`#block-${id}`).remove();
          $.ajax({
            type: "DELETE",
            url: `/edit/delete/block/${id}`
          });
        },
        btnClass: "btn-danger",
        keys: ['enter'],
      },
      no: function () {},
    }
  });
}

/* keeps track of if the panning just ended, such that the click
on canvas does not trigger to soon */
var panend = false;
$(function () {

  pz = panzoom(document.querySelector('#canvas'), {
    maxZoom: 2,
    minZoom: 0.1,
    smoothScroll: false,
    filterKey: () => true,
  })
  .on('panend', e => {
    panend = true;
  });

  $('textarea, input')
    .focus(() => pz.pause())
    .blur(() => pz.resume())
    ;
});

$(function () {
  /* receive a message that the scene has been saved, loads it
  if the client did not save it themself. */
  socket.on('saved', function (msg) {
    $('#last-saved').text(`last saved: ${msg['timestamp']}`);
    clientId = socket.id;
    if(clientId != msg['clientId']) {
      $.get('/edit/scene/' + pid, function(scene) {
        loadScene(scene);
      });
    }
  });
});

// Used to indicate errors, adapted from https://stackoverflow.com/questions/4399005/implementing-jquerys-shake-effect-with-animate
jQuery.fn.indicateError = function(text) {
  intShakes = 2;
  intDistance = 4;
  intDuration = 200;
  this.each(function() {
    var element = $(this);
    element.tooltip({title: text, trigger: 'manual'});
    element.tooltip('show');
    setTimeout(function() {
      element.tooltip('dispose');
    }, 1500);
    element.css("position","relative"); 
    for (var x=1; x<=intShakes; x++) {
      element.animate({left:(intDistance*-1)}, (((intDuration/intShakes)/4)))
      .animate({left:intDistance}, ((intDuration/intShakes)/2))
      .animate({left:0}, (((intDuration/intShakes)/4)));
    }
  });
  return this;
};

String.prototype.capitalize = function() {
  return this.charAt(0).toUpperCase() + this.slice(1)
}