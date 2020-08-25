class MergeNode extends ArrayableNode {
  constructor(params) { super("merge", params || {}); }
  run() {
		super.run();
		this.inputToCopy = { id: 'input', values: this.values.input || [] };
		if(!isEmpty(this.inputToCopy)) { this.addArrayInput(); }
	}
  copyInput(copy, click=false) {
    super.copyInput(copy, click, '', false);
  }
  addChangeEvents(){
    super.addChangeEvents('.list-group-item', false);
  }
  arrayValuesChange(id) {
    super.arrayValuesChange(id, f => '', '');
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
  }
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

}