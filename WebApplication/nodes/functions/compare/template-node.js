class CompareNode extends Node {
  constructor(params) { super("compare", params || {}) }
  renderOperator(type, connect = true) {
    /* type = string, number, face,... */
    var endpoint = {}
    this.nodeParams.endpoints.forEach(e => {
      if(e.id == 'op') { 
        endpoint = jsonCopy(e);
        endpoint.field.options = endpoint.field[type+'_options'];
      }
    })
    var partial = connect ? 'selectOp' : 'op';
    var template = Handlebars.compile('{{dynamic \''+partial+'\' this}}');
    this.elem.find('.endpoint-field[data-eid="op"]').html(template(endpoint));
    if(type != '') {
      this.addChangeEvents();
    }
    this.repaintEndpoints();
  }
  onConnectionDelete(info) {
    this.elem.find('.nin').each((i, e) => {
      var ttype = $(e).data('orgtype');
      if(ttype == prim_type) {
        var stype = $(e).data('type');
        changeEndpointType(e, stype, ttype)
      }
    });
    var type = $(info.target).data('orgtype')
    this.renderOperator(type, false)
  }
  onConnectionConnect(info) {
    this.elem.find('.nin').each((i, e) => {
      var ttype = $(e).data('type');
      if(ttype == prim_type) {
        $(e).data('orgtype', ttype)
        var stype = $(info.source).data('type')  
        changeEndpointType(e, ttype, stype)
      }
    });
    var type = $(info.target).data('type')
    this.renderOperator(type)
  }
}