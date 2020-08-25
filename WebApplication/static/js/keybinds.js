// canvas = '#canvas';
content = '#content';
windowSelectedClass = 'window-selected';

/* create keybindings */
var copied_selection;
$(function () {
  $("body").on("keydown", function (e) {
    /* ctrl+s, save scene */
    if (e.ctrlKey && 83 == e.which) {
      e.preventDefault();
      saveScene();
    }
    /* ctrl+a, selects all */
    if (e.ctrlKey && 65 == e.which) {
      if(!pz.isPaused()) {
        e.preventDefault();
        selectAll();
      }
    }
    /* del, delete selected */
    if (
      $('.window').hasClass(windowSelectedClass) &&
      46 == e.which
    ) {
      e.preventDefault();
      deleteSelected();
    }
    /* esc, deselect all */
    if (
      $('.window').hasClass(windowSelectedClass) && 
      27 == e.which
    ) {
      e.preventDefault();
      deselectAll();
    }
    /* ctrl+shift+s, save selected */
    if(
      $('.window').hasClass(windowSelectedClass) && 
      e.shiftKey && e.ctrlKey && 83 == e.which
    ) {
      e.preventDefault();
      saveSelected();
    }
    /* ctrl+0, reset pan/zoom to initial position */
    if(e.ctrlKey && 48 == e.which) {
      if(typeof pz !== 'undefined') {
        pz.moveTo(1, 1);
        pz.zoomAbs(1, 1, 1);
      }
    }
    /* ctrl+c, copy selection */
    if(
      selected.length > 0 &&
      e.ctrlKey && 67 == e.which
    ) {
      e.preventDefault();
      copied_selection = serializeBlock();
      copied_selection.block.forEach((e, i , a) => {
        e.x += 200;
        e.y += 200;
        a[i] = e;
      });
    }
    /* ctrl+v, paste selection */
    if(
      typeof copied_selection !== 'undefined' &&
      e.ctrlKey && 86 == e.which
    ) {
      e.preventDefault();
      nids = loadBlock(copied_selection);
      selectNodes(nids);
      copied_selection = null;
    };
  });
});

/* select nodes with an id in the ids array */
function selectNodes(ids) {
  deselectAll();
  ids.forEach(id => {
    selected.push(id)
    inst.addToPosse($(`#${id}`), "posse");
    $(`#${id}`).addClass(windowSelectedClass);
  });
}

/* display keybindings overview */
$('#key-bindings').click(function () {
  bindings = [
    { binding: "ctrl + s" , action: "save scene" },
    { binding: "shift + click", action: "select nodes" },
    { binding: "ctrl + a" , action: "select all nodes" },
    { binding: "ctrl + c" , action: "copy" },
    { binding: "ctrl + v" , action: "paste" },
    { binding: "esc" , action: "deselect all nodes" },
    { binding: "del" , action: "delete selected nodes" },
    { binding: "ctrl + shift + s" , action: "save selected nodes as building block" },
    { binding: "scroll" , action: "zoom" },
    { binding: "click + drag" , action: "pan" },
    { binding: "ctrl + 0" , action: "reset pan/zoom to initial position" },
  ]

  content = '<table class="table" style="color:inherit;">' +
    '<thead><tr><th>key</th><th>action</th></tr></thead>' +
    '<tbody>';
  for (var k in bindings) {
    v = bindings[k]
    content += 
      '<tr>' +
        '<th>' + v['binding'] + '</th>' +
        '<td>' + v['action'] + '</td>' +
      '</tr>';
  }
  content += '</tbody></table>';

  $.alert({
    title : '',
    animation: 'none',
    content: content,
    // escapeKey: true,
    backgroundDismiss: true,
    columnClass: 'col-md-5',
    draggable: false,
  });
});

/* select all */
function selectAll() {
  selected = [];
  $('.window').addClass(windowSelectedClass);
  $('.window').each(function () {
    selected.push($(this).attr('id'));
  });
  inst.addToPosse($('.window'), "posse");
}
$('#select-all').click(() => selectAll());

/* deselect all */
function deselectAll() {
  if(selected.length > 0) {
    $('.window').removeClass(windowSelectedClass);
    inst.removeFromPosse($('.window'), "posse");
    selected = [];
  }
}
$('#deselect-all').click(() => deselectAll());

/* on canvas click, deselect all */
$(content).click((e) => {
  if(
    $('.window').hasClass(windowSelectedClass) 
    && $(e.target).closest('.window').length == 0
  ) {
    /* if we just panned do nothing */
    if(panend) { 
      panend = false; 
    } else {
      deselectAll();
    }
  }
});

/* delete selected */
function _deleteSelected() {
  selected.forEach( s => classNodes[s].remove() );
  selected = [];
  setTimeout(saveScene, 500);
}

/* confirmation of deletion */
function deleteSelected() {
  $.confirm({
    animation: 'none',
    title: 'Delete',
    content: 'Are you sure you want to delete your selection?',
    buttons: {
        yes: {
          action : () => _deleteSelected(),
          btnClass: "btn-danger",
          keys: ['enter'],
        },
        no: function () {},
    }
  }); 
}

/* delete selection button click */
$('#delete-selected').click(() => {
  if(selected.length == 0) { return; }
  deleteSelected();
  saveScene();
});

/* serialize selected nodes to a block 
(corresponds to the blocks saved in db) */
function serializeBlock() {
  var block = [];
  selected.forEach(s => {
    var node = jsonCopy(classNodes[s].serialize())
    node.oid = jsonCopy(node.id)
    node.id = node.id.replace(/_\d+/g, '')
    block.push(node);
  });

  /* include connections to the block */
  var connections = [];
  $.each(inst.getAllConnections(), function (idx, connection) {
    /* add a connection/edge to the save */
    getId = id => {
      elem = $(`#${id}`);
      nodeId = elem.closest('.window').attr('id');
      eid = elem.data('eid').replace(/_\d+/gi, '');
      return `${nodeId}-${eid}`;
    }
    sourceId = getId(connection.sourceId);
    targetId = getId(connection.targetId);
    sourceElem = sourceId.split('-')[0]
    targetElem = targetId.split('-')[0]
    /* sort out connections that do not have both endpoints in the block */
    sourceNode =  block.find(x => x.oid == sourceElem);
    targetNode =  block.find(x => x.oid == targetElem);
    if(
      typeof sourceNode !== 'undefined' 
      && typeof targetNode !== 'undefined'
    ) {
      connections.push({
        source: sourceId,
        target: targetId,
      });
    }
  });
  return {
    block: block,
    connections: connections
  };
}

/* send selection to server, to be saved in db */
function saveSelected() {
  if(selected.length == 0) { return; }
  // https://craftpip.github.io/jquery-confirm/#confirm
  $.confirm({
    title: 'Name',
    content: '' +
      '<form action="" class="formName">' +
      '<div class="form-group">' +
      '<label>Give the building block a name</label>' +
      '<input type="text" placeholder="Building block name" class="name form-control" required style="border: 1px solid #ccc;" />' +
      '</div>' +
      '</form>',
    animation: 'none',
    buttons: {
      formSubmit: {
        text: 'save',
        btnClass: 'btn-dark',
        action: function () {
          var name = this.$content.find('.name').val();
          if(!name){
            $.alert('provide a valid name');
            return false;
          }

          data = {
            name: name,
            data: serializeBlock()
          };

          console.log('save-block', data)
          $.ajax({
            type: "POST",
            url: '/edit/save/block',
            data: JSON.stringify(data),
            success: function(msg) {
              id = msg['id'];
              blocks[id] = data;
              data['id'] = id; 
              /* show in submenu */
              addBlockSubmenu(data, false);
              deselectAll();
            },
            contentType: "application/json",
            dataType: "json",
          });
        }
      },
      cancel: function () {},
    },
    onContentReady: function () {
      // bind to events
      var jc = this;
      this.$content.find('form').on('submit', function (e) {
        e.preventDefault();
      });
    }
  });
}
/* click on save selection */
$('#save-selected').click(() => saveSelected());
