class ConditionalNode extends Node {
  constructor(params) { super("conditional", params || {}) }

  onConnectionDelete(info) {
    if($(info.target).data('eid') == 'in') { return; }
    this.elem.find('.nin, .nout').each((i, e) => {
      var ttype = $(e).data('orgtype');
      if(ttype == any_type) {
        var stype = $(e).data('type');
        var dir = $(e).hasClass('nin') ? 'in' : 'out';
        changeEndpointType(e, stype, ttype, dir);
      }
    });
  }
  onConnectionConnect(info) {
    if($(info.target).data('eid') == 'input') { return; }
    this.elem.find('.nin, .nout').each((i, e) => {
      var ttype = $(e).data('type');
      if(ttype == any_type) {
        $(e).data('orgtype', ttype);
        var stype = $(info.source).data('type');
        var dir = $(e).hasClass('nin') ? 'in' : 'out';
        changeEndpointType(e, ttype, stype, dir);
      }
    });
  }
}