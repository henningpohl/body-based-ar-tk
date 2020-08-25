class PrintfNode extends ArrayableNode { 
	constructor(params) { super("printf", params || { }) } 
	run() {
    super.run();
    this.values.input = this.values.input || [""]
    this.inputToCopy = { id: 'input', values: this.values.input || [""] };
		if(!isEmpty(this.inputToCopy)) { this.addArrayInput(); }
	}
	copyInput(copy, click=false) {
    super.copyInput(copy, click, '', false);
  }
  addChangeEvents(){
    super.addChangeEvents('.list-group-item', false);
  }
	onConnectionConnect(info) {
    var stype = $(info.source).data('type')
    var ttype = $(info.target).data('type')
    $(info.target).data('orgtype', ttype)

    if($(info.target).data('eid').startsWith('input')) {
      var customTo = $(info.source).siblings('.endpoint-type').text();
      changeEndpointType(info.target, ttype, stype, null, customTo)
    }
  }
  onConnectionDelete(info) {
    var stype = $(info.target).data('orgtype')
    var ttype = $(info.target).data('type')

    if($(info.target).data('eid').startsWith('input')) {
      changeEndpointType(info.target, ttype, stype)
    }
  }
}