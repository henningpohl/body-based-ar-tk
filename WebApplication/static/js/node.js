const any_type = "any";
const prim_type = "primitive";
const prim_scope = "string color number position";
const array_type = "array";
const array_scope = "strings colors numbers positions faces poses"

function changeEndpointType (endpoint, fromtype, totype, dir, customTo) {
  var str = customTo || totype;
  var dir = dir || 'in';

  $(endpoint).data('type', totype)
    .siblings('.endpoint-type.' + dir)
    .text(str)
    .removeClass(fromtype)
    .addClass(totype);

  fill = endpointTypes[totype].paintStyle.fill
  endpointId = $(endpoint).attr('id')
  endpoints = inst.getEndpoints(endpointId).forEach(e => {
    if(e.type != 'Blank') {
      e.removeType(fromtype);
      e.addType(totype);
    }
  })
}

function isEmpty(obj) {
  for (var o in obj) if (o) return false;
  return true;
}

function jsonCopy(src) { return JSON.parse(JSON.stringify(src)); } 

class Node {
  constructor(type, params) {
    this.id = null;
    this.elem = null;
    this.type = type
    this.params = params || {};
    this.values = this.params.values || {};
    this.endpoints = this.params.endpoints || []
    this.nodeParams = {};
  }
  run() {
    this.genNodeParams();
    this.render();
    this.init();
    this.addChangeEvents();
    this.repaintEndpoints();
  }
  /* compile handlbars template and add endpoints */
  addEndpoint(elem, dir) {
    var _isSource = dir == 'out';
    if(endpointTypes.hasOwnProperty(elem.data('type'))) {
      var color = endpointTypes[elem.data('type')].paintStyle.fill;
    } else { 
      var color = '#333'
    }
    if(_isSource) {
      var _cssClass = `in-endpoint endpoint-type-${elem.data('type')} endpoint-in-${this.id}-${elem.data('eid')}`
      var _cssClass = `out-endpoint endpoint-type-${elem.data('type')} endpoint-out-${this.id}-${elem.data('eid')}`
    } else {
      var _cssClass = `in-endpoint endpoint-type-${elem.data('type')} endpoint-in-${this.id}-${elem.data('eid')}`
    }
    var _endpoint = [ "Rectangle", {
      width: 20,
      height: 20,
      cssClass: _cssClass
    }];
    /* allow for any scope */
    var scope = elem.data('scope')
    if(scope == any_type) { scope = any_scope.join(' '); }
    if(scope == prim_type) { scope = prim_scope; }
    if(scope == array_type) { scope = array_scope; }
    var endpoint = {
      isSource: _isSource, 
      isTarget: !_isSource,
      scope: scope, 
      type: elem.data('type'),
      maxConnections: _isSource ? -1 : elem.data('maxconnections'),
      dropOptions: {
        tolerance: "touch",
        hoverClass: "dropHover",
        activeClass: "dragActive"
      },
      endpoint: _endpoint,
      connectorStyle: {
        strokeWidth: 5,
        stroke: color,
        dashstyle: "1 2"
      },
      anchor: _isSource ? "Right" : "Left"
    }
    inst.addEndpoint(elem, endpoint);
  }
  render() {
    var template = Handlebars.compile($("#nodeTemplate").html());
    $("#canvas").append(template(this.nodeParams));
    this.elem = $(`#${this.id}`);
    
    /* add input endpoints */
    $(`#${this.id} .nin`).each((i, e) => {
      this.addEndpoint($(e), 'in')
    });
    /* add output endpoints */
    $(`#${this.id} .nout`).each((i, e) => {
      this.addEndpoint($(e), 'out')
    });
  }
  repaintEndpoints() {
    this.elem.find('.nout, .nin').each((i, elem) => {
      inst.revalidate($(elem).attr('id'));
    });
  }
  /* initialize draggable, should be used everytime as super.init() */
  init(repaint = false) {
    var repaintToggleDragged = el => {
      var id = $(el).attr('id')
      this.repaintEndpoints()
      var endpoints = $(`div[class*="endpoint-in-${id}"], div[class*="endpoint-out-${id}"]`);
      endpoints.toggleClass("push-top");
      $(el).toggleClass("push-top");
    }

    /* make nodes draggable */
    inst.draggable(this.elem, {
      /* nessessary for allowing resizing of nodes, fx when having a script node */
      start: params => repaintToggleDragged(params['el']),
      drag: () => { if(repaint){ this.repaintEndpoints(); } },
      stop: params => {
        repaintToggleDragged(params['el']);
        saveScene();
      }
    });
    /* make node deleteable */
    $(`#${this.id} .delete-node`).each((i, e) => {
      $(e).click(() => {
        $.confirm({
          animation: 'none',
          title: 'Delete',
          content: 'Are you sure you want to delete this?',
          buttons: {
            yes: {
            action : () => {
                this.remove();
                saveScene();
              },
              btnClass: "btn-danger",
              keys: ['enter'],
            },
            no: () => {},
          }
        });
      });
    });

    this.elem.click(function (e) {
      if(/*select_mode &&*/ e.shiftKey) {
        $(this).toggleClass('window-selected');
        if($(this).hasClass('window-selected')) {
          selected.push($(this).attr('id'))
          inst.addToPosse($(this), "posse");
        } else {
          selected = selected.filter(s => s != $(this).attr('id') );
          inst.removeFromPosse($(this), "posse");
        }
      }
    });
  }
  remove(){
    this.elem.find('.nin, .nout').each((i, e) => {
      var id = $(e).attr('id');
      var es = inst.getEndpoints(id)
      es.forEach(i => {
        i.connections.forEach(c => c.setData({save: false}));
        inst.deleteEndpoint(i, false, true)
      });
    })
    this.elem.remove();
    delete classNodes[this.id];
    onDesync();
  }
  copyEndpoint(e) {
    var copy = e.data('copy');
    var toCopy = this.nodeParams.endpoints.filter(e => e.id == copy)
    var toAddEndpointIndex;
    var toAddEndpoint = this.nodeParams.endpoints.some((e, i) => {
      if(
        e.hasOwnProperty('field')
        && e.field.copy == copy
        && e.type == 'add'
        ) { toAddEndpointIndex = i; return i; }
    });
    if(toCopy.length < 1 || !toAddEndpoint) { return; }
    var toCopyEndpoint = jsonCopy(toCopy[0])
    var existingCopies = $(`#${this.id} .endpoint-type.${toCopyEndpoint.type}`)
    toCopyEndpoint.id = `${toCopyEndpoint.id}_${existingCopies.length}`
    var template = Handlebars.compile(
      `{{#each endpoints}}{{> genericEndpoint parentId='${this.id}'}}{{/each}}`
    );
    
    $(this.elem.find('.list-group-item')[toAddEndpointIndex])
      .before(template({endpoints: [toCopyEndpoint]}));

    this.addEndpoint($(`.${toCopyEndpoint.direction}-${this.id}-${toCopyEndpoint.id}`), toCopyEndpoint.direction)
    this.nodeParams.endpoints.splice(toAddEndpointIndex, 0, toCopyEndpoint);
    this.addChangeEvents();
  }
  /* generic input/select events */
  addChangeEvents(desync = false) {
    $(`#${this.id} input, #${this.id} select`).each((i, e) => {
      $(e).unbind('change');
      $(e).change(() => {
        this.values[$(e).data('eid')] = $(e).val();
        if(desync) { onDesync(); }
        this.emitDataChange($(e).data('eid'));
        saveScene();
      });
    });
  }
  /* send data change through the socket to the device, via the server */
  emitDataChange(eid=null) {
    eid = eid || this.type
    var inSync = false
    for(k in devices.devices) {
      if(devices.devices[k].sync) { inSync = true; break; }
    }
    if(inSync) {
      let values = this.serialize().values;
      let payload = {
        pid: pid,
        nid: this.id,
        ntype: eid,
        value: values
      }
      socket.emit('updateValue', payload);
    }
  }
  /* make a node ready for saving/updateValue */
  serialize() {
    return {
      id: this.id,
      type: this.type,
      x: parseInt(this.elem.css("left"), 10),
      y: parseInt(this.elem.css("top"), 10),
      values: this.values,
    }
  }
  /* build nodeParams for compilation */
  genNodeParams() {
    var nodeParams = jsonCopy(nodes[this.type]);
    nodeParams['type'] = this.type;
    nodeParams['values'] = this.values;
    
    /* load endpoints if saved (mostly used for arrayables) */
    if(typeof this.endpoints !== 'undefined' && this.endpoints.length > 0) { 
      nodeParams['endpoints'] = this.endpoints;
    } else {
      this.endpoints = nodeParams['endpoints'];
    }

    nodeParams['x'] = this.params['x'] || 200.0;
    nodeParams['y'] = this.params['y'] || 100.0;
    
    if('id' in this.params) {
      nodeParams['id'] = this.params['id'];
      this.id = this.params['id'];
    } else {
      var es = $("#canvas").find(`div[id^="${this.type}"].window`);
      this.id = this.type + "_" + es.length;
      nodeParams['id'] = this.id;
    }

    nodeParams.endpoints.forEach(e => {
      if('field' in e) {
        if('values' in e.field) {
          /* add values from definition */
          this.params.values = { ...e.field.values, ...this.params.values }
        }
        e.field.values = this.params.values || {}
      }
      e = this.endpointFieldCallback(e);
    });
    this.nodeParams = nodeParams;
  }
  endpointFieldCallback(e) { return e; }
  setValue(field, value) { this.values[field] = value }
  onConnectionConnect(info) {
    var ttype = $(info.target).data('type')
    if(ttype == prim_type) {
      var stype = $(info.source).data('type')
      $(info.target).data('orgtype', ttype)
      var customTo = $(info.source).siblings('.endpoint-type').text();
      changeEndpointType(info.target, ttype, stype, null, customTo)
    } 
  }
  onConnectionDelete(info) {
    var stype = $(info.target).data('orgtype')
    if(stype == prim_type) {
      var ttype = $(info.target).data('type')
      changeEndpointType(info.target, ttype, stype)
    }
  }
}

// string, number, faceRecognizer, faceTracker, faceAugmentation
class GenericNode extends Node {
  constructor(type, params) { super(type, params || {}) }
}

/* Nodes that have endpoints that can handle multiple inputs/outputs */
class ArrayableNode extends Node {
  constructor(type, params) {
    super(type, params)
    this.inputToCopy = {};
  }
  run() {
    super.run();
    if(!isEmpty(this.inputToCopy)) { this.addArrayInput(); }
  }
  init() {
    super.init(true);

    $(`#${this.id} .build-array`).click((e) => {
      this.copyInput($(e.currentTarget).data('copy'), true);
      saveScene();
    });
  }
  addArrayInput() {
    this.inputToCopy.values.forEach((e, i) => {
      if(i > 0) { this.copyInput(this.inputToCopy.id); }
      this.populateArrayInput(e, i)
    });
  }
  populateArrayInput(e, i) {
    var elemIndex = '';
    if(i > 0) { elemIndex = `_${i}`; }

    this.elem
      .find(`#input-${this.id}-${this.inputToCopy.id}${elemIndex}`)
      .val(e)
  }
  addChangeEvents(delete_elem = '.input-group', array=true) {
    $(`#${this.id} input, #${this.id} select`).each((i, e) => {
      $(e).unbind('change');
      $(e).change(() => {
        var orgEid = $(e).closest('.endpoint-field').data('eid');
        this.arrayValuesChange(orgEid);
        this.emitDataChange(orgEid);
        saveScene();
      });
    });
    this.addDeleteInputEvent(delete_elem, array);
  }
  addDeleteInputEvent(delete_elem, array) {
    $(`#${this.id} .delete-input`).each((i, e) => {
      $(e).unbind('click');
      $(e).click(() => {
        var orgEid = $(e).closest('.endpoint-field').data('eid');
        if(!array) { this.removeEndpoint(orgEid); }
        $(e).closest(delete_elem).remove();
        this.arrayValuesChange(orgEid);
        saveScene();
        this.repaintEndpoints();
        return false;
      });
    });
  }
  removeEndpoint(eid){
    $(`#${this.id} .jtk-endpoint-anchor[data-eid="${eid}"]`).each((i, e) => {
      inst.remove($(e).attr('id'));
    });
  }
  arrayValuesChange(id, valuefn=null, elements='input') {
    valuefn = valuefn || (f =>  $(f).val());
    var regex = /_\d+/gi;
    id = id.replace(regex, '');
    this.values[id] = [];
    this.elem.find(`.endpoint-field[data-eid^="${id}"] ${elements}`).each((i, f) => {
      var val = valuefn(f);
      this.values[id].push(val);
    });
  }
  copyInput(copy, click=false, elements='input', array=true) {
    /* find the to be copied endpoint */
    var toCopy = this.nodeParams.endpoints.filter(f => f.id == copy)
    if(toCopy.length < 1) { return; }
    var toCopyEndpoint = jsonCopy(toCopy[0])
    /* add deleteable to type */
    if('field' in toCopyEndpoint) {
      toCopyEndpoint.field.type = `deleteable-${toCopyEndpoint.field.type}`;
    } 
    var e = !array ? 'deleteable-genericEndpoint' : `${toCopyEndpoint.field.type || 'genericEndpoint'}`;
    /* compile endpoint */
    var template = Handlebars.compile(`{{dynamic '${e}' this}}`);
    var numInput = this.elem.find(`.endpoint-field[data-eid^=${toCopyEndpoint.id}] ${elements}`).length
    toCopyEndpoint.parentId = this.id;
    toCopyEndpoint.id = `${toCopyEndpoint.id}_${numInput}`;

    if(array) {
      this.elem.find(`.endpoint-field[data-eid=${copy}]`)
        .append(template(toCopyEndpoint));
    } else {
      this.elem.find(`.endpoint-field[data-eid="add-in"]`)
        .parent().before(template(toCopyEndpoint));
      var endpoint = this.elem.find(`i.in-${this.id}-${toCopyEndpoint.id}`)
      this.addEndpoint(endpoint, 'in')
    }
    
    /* add change events to inputs/selects */
    this.addChangeEvents();
    /* add an empty string to make the system aware of the added copy */
    if(!this.values.hasOwnProperty(copy)) { this.values[copy] = ['']; }
    if(click) { this.values[copy].push(''); }

    /* change to types instead of type  */
    if(array) {
      this.elem.find(`.endpoint-type.${toCopyEndpoint.type}`).text(`${toCopyEndpoint.type}s`);
    }

    this.repaintEndpoints();
  }
  endpointFieldCallback(e) {
    if('field' in e) {
      if(e.field.hasOwnProperty('values') && Array.isArray(e.field.values[e.id])) {
        this.inputToCopy = {
          id: e.id,
          values: e.field.values[e.id]
        };
      }
    }
  }
  serialize() {
    return {
      id: this.id,
      type: this.type,
      x: parseInt(this.elem.css("left"), 10),
      y: parseInt(this.elem.css("top"), 10),
      values: this.values,
      endpoints: this.nodeParams.endpoints
    }
  }
}

class ArrayOpNode extends Node {
  constructor(type, params) { super(type, params || {}) } 
  init() { super.init(true); }
  addChangeEvents() { super.addChangeEvents(); }
  renderOperator(type, connect=true) {
    /* type = string, number, face,... */
    var endpoint = {}
    this.nodeParams.endpoints.forEach(e => {
      if(e.id == 'op') { 
        endpoint = jsonCopy(e);
        endpoint.field.options = endpoint.field[''+type+'_options'];
      }
    })
    var partial = connect ? 'selectOp' : 'op';
    var template = Handlebars.compile('{{dynamic \''+partial+'\' this}}');
    this.elem.find('.endpoint-field[data-eid="op"]').html(template(endpoint));

    if(type != '') { this.addChangeEvents(); }
    if(this.values.hasOwnProperty('op')) {
      this.elem.find('select').change();
    }
    this.repaintEndpoints();
  }
  onConnectionDelete(info) {
    this.elem.find('.nin, .nout').each((i, e) => {
      var ttype = $(e).data('orgtype');
      if(ttype == array_type) {
        var stype = $(e).data('type');
        var dir = $(e).hasClass('nin') ? 'in' : 'out';
        changeEndpointType(e, stype, ttype, dir);
      }
    });
    var type = $(info.target).data('orgtype')
    this.renderOperator(type, false)
  }
  onConnectionConnect(info) {
    this.elem.find('.nin, .nout').each((i, e) => {
      var ttype = $(e).data('type');
      if(ttype == array_type) {
        $(e).data('orgtype', ttype);
        var stype = $(info.source).data('type');
        var dir = $(e).hasClass('nin') ? 'in' : 'out';
        var customTo = $(info.source).siblings('.endpoint-type').text();
        changeEndpointType(e, ttype, stype, dir, customTo);
      }
    });
    var type = $(info.source).data('type')
    this.renderOperator(type)
  }
}

