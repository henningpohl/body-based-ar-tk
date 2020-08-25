class DistanceNode extends Node {
  constructor(params) { super("distance", params || {}) }
  onConnectionDelete(info) {
    this.elem.find('.nin').each((i, e) => {
      var ttype = $(e).data('orgtype');
      if(ttype == prim_type) {
        var stype = $(e).data('type');
        changeEndpointType(e, stype, ttype)
      }
    });
    this.elem.find('.endpoint-field[data-eid="distance"]').text("distance");
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

    var type = $(info.source).data('type')
    var distance = '';
    switch (type) {
      case "position":
      case "color": distance = 'Euclidean distance'; break;
      case "number": distance = 'numeric distance'; break;
      case "string": distance = 'Levenshtein distance'; break;
    } 
    this.elem.find('.endpoint-field[data-eid="distance"]').text(distance);
  }
}