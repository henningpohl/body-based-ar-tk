class SwitchNode extends ArrayableNode { 
	constructor(params) { super("switch", params || { }) } 
	copyInput(copy, click=false) {
    super.copyInput(copy, click, '', false);
  }
  addChangeEvents(){
		super.addChangeEvents('.list-group-item', false);
		
		this.elem.find('.help').click((e) => {
			$.confirm({
				animation: 'none',
				title: 'Switch help',
				backgroundDismiss: true,
				columnClass: 'col-md-6',
				content: `
				<h6><b>Usage<b><h6>
				<div>
					Write a JavaScript expression in the field(s) below. 
					Use <tt>this.x</tt> to get expression value. 
					You can write <tt>default</tt> to catch anything that does not match other expressions.
					Note that strings need to be wrapped in quotation marks.
				</div>
				`,
				buttons: {
					ok: () => {}
				}
			});
		})
  }
	onConnectionConnect(info) {
    var stype = $(info.source).data('type')
    var ttype = $(info.target).data('type')
    $(info.target).data('orgtype', ttype)

    if($(info.target).data('eid').startsWith('input')) {
      var customTo = $(info.source).siblings('.endpoint-type').text();
      changeEndpointType(info.target, ttype, stype, null, customTo)
    } else {
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
  onConnectionDelete(info) {
    var stype = $(info.target).data('orgtype')
    var ttype = $(info.target).data('type')

    if($(info.target).data('eid').startsWith('input')) {
      changeEndpointType(info.target, ttype, stype)
    } else {
			this.elem.find('.nin, .nout').each((i, e) => {
				var ttype = $(e).data('orgtype');
				if(ttype == any_type) {
					var stype = $(e).data('type');
					var dir = $(e).hasClass('nin') ? 'in' : 'out';
					changeEndpointType(e, stype, ttype, dir);
				}
			});
		}
  }
}