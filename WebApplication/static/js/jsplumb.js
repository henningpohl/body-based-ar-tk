/* 
Add a node to the canvas/scene
used when adding from menu and when loading a scene
*/
function addNode(params, load) {
  load = load || false;
  var node = null;

  try {
    t = params.type.capitalize();
    n = `${t}Node`;
    node = eval(`new ${n}(params)`);
  } catch {
    switch (params.type){
      case "singleArrayOp":
        node = new ArrayOpNode(params.type, params); break;
      case "string":
      case "number":
        node = new ArrayableNode(params.type, params); break;
      default:
        node = new GenericNode(params.type, params); break;
    }
  }
  node.run();
  classNodes[node.id] = node;

  /* we dont want to save a newly loaded scene */
  if(!load){ 
    onDesync();
    saveScene();
  }
  return node.id;
}

/* keep track of the number of loaded connections, to not save on load */
function addConnector(params) {
  /* highlight the endpoints when connected */
  $('.endpoint-out-' + params['source']).addClass('jtk-endpoint-connected')
  $('.endpoint-in-' + params['target']).addClass('jtk-endpoint-connected')

  inst.connect({
    source: $(`.out-${params['source']}`).attr('id'),
    target: $(`.in-${params['target']}`).attr('id'),
    endpoints: [["Blank", {}], ["Blank", {}]],
    anchors:[
      ["Continuous", {faces:["right"]}],
      ["Continuous", {faces:["left"]}]
    ]
  });
}

/* When jsPlumb is ready initialize things */
jsPlumb.ready(function() {

/* 
  the jsPlumb instance, which is used everywhere
  sets default values, which can be changed in addEndpoint or connect
   */
  inst = jsPlumb.getInstance({
    Container: $("#canvas"),
    Connector: [ "Bezier", { curviness:50 } ],
    ConnectionsDetachable: false,
    Anchor: ['Left', 'Right'],
  });

  inst.batch(function () {
    
    /* when a connection is deleted, save */
    inst.bind("connectionDetached", function(info) { 
      if(
        info.connection.getData().hasOwnProperty('save') 
        && !info.connection.getData().save
      ) {
        return true;
      }
      saveScene(); 
    });

    /* when a new connection is made, save and make sure that the connection is styled correctly */
    inst.bind("connection", function(info, originalEvent) {
      if(originalEvent != null) { 
        saveScene(); 
        onDesync();
      }

      /* change type based on input */
      classNodes[$(info.target).closest('.window').attr('id')].onConnectionConnect(info)
      
      getColor = id => endpointTypes[$(id).data('type')].paintStyle.fill;
      source_color = getColor(info.source);
      target_color = getColor(info.target);
      
      style = { 
        gradient: {
          stops: [
            [0, source_color],
            [1, target_color]
          ]
        },
        strokeWidth: 5,
        stroke: source_color,
        dashstyle: "0"
      }
      
      hoverStyle = { 
        gradient: {
          stops: [
            [0, '#d9534f'],
            [1, '#d9534f']
          ]
        },
        strokeWidth: 5,
        stroke: '#d9534f',
        dashstyle: "0" 
      }
      
      info.connection.setPaintStyle(style);
      info.connection.setHoverPaintStyle(hoverStyle);
      
      /* add a label on hover and color the connection */
      info.connection.bind("mouseover", function(conn) {
        conn.addOverlay(["Label", { 
          label: `<em id="connection-label">click to remove</em>`,
          cssClass: 'push-top',
          labelStyle: {
            fill: '#ddd',
            color: '#d9534f',
            padding: '2px',
            borderStyle: "#d9534f",
            borderWidth: 1
          },
          location: 0.5,
          id: "connection-label"
        }]);
        var top = $('#connection-label').parent().css('top');
        top = parseInt(top.replace('px', ''));
        $('#connection-label').parent().css('top', `${top-15.0}px`)
      }); 
      
      /* remove label on mouseout */
      info.connection.bind("mouseout", function(conn) {
        conn.removeOverlay("connection-label");
      });
    });

    /* delete a connection, based on target/source component */
    function _deleteConnection(component) {
      var tid = $(component.target).closest('.window').attr('id');      
      classNodes[tid].onConnectionDelete(component)
      
      var c = {
        sourceId: jsonCopy(component.sourceId),
        targetId: jsonCopy(component.targetId)
      };
      inst.deleteConnection(component);

      var removeConnectedClass = (id, dir) => {
        var endpointId = `${id}`;
        var select = dir ? {source:id} : {target:id}
        if(inst.select(select).length == 0) {
          var id = $(`#${endpointId}`).closest('.window').attr('id');
          var eid = $(`#${endpointId}`).data('eid'); 
          $(`.endpoint-${dir ? 'in' : 'out'}-${id}-${eid}`).removeClass('jtk-endpoint-connected');
        }
      }
      removeConnectedClass(c.sourceId, true);
      removeConnectedClass(c.targetId, false);
      saveScene();
      onDesync();
    }

    /* delete a connection on click */
    inst.bind("click", function (component, originalEvent) {
      jsPlumbUtil.consume(originalEvent);
      _deleteConnection(component);
    });
    
    
    /* styles for the endpoints */ 
    // http://materialuicolors.co/?utm_source=launchers
    endpointTypes = {
      "number": { paintStyle:{fill:"#FF9800"} },
      "string": { paintStyle:{fill:"#8BC34A"} },
      "color": { paintStyle:{fill:"#FF6859"} },
      "face": { paintStyle:{fill:"#72DEFF"} },
      "position":{ paintStyle:{fill:"#03A9F4"} },
      "any":{ paintStyle:{fill:"#FFEB3B"} },
      "primitive":{ paintStyle:{fill:"#FFEB3B"} },
      "array":{ paintStyle:{fill:"#1EB980"} },
      "pose":{ paintStyle:{fill:"#009688"} },
      "emotion":{ paintStyle:{fill:"#E91E63"} },
      "boolean":{ paintStyle:{fill:"#B15DFF"} },
      "image":{ paintStyle:{fill:"#00BCD4"} },
    }

    css = "";
    for (var type in endpointTypes) {
      var style = endpointTypes[type];
      css += `.endpoint-type.${type} { color: ${style.paintStyle.fill}; } `
    }
    $("<style>").prop("type", "text/css").html(css).appendTo("head");

    inst.registerEndpointTypes(endpointTypes);
    loadScene(scene);
  });

});
