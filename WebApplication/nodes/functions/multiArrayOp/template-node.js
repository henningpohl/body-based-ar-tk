class MultiArrayOpNode extends ArrayOpNode {
  constructor(params) { super("multiArrayOp", params || {}) } 
  addChangeEvents() {
    $('#'+this.id+' select').each((i, e) => {
      $(e).unbind('change');
      $(e).change(() => {
        this.values[$(e).data('eid')] = $(e).val();
        
        this.elem.find('.editor').closest('.list-group-item').remove();
        if(['map', 'filter'].includes($(e).val())) {
          var endpoint = {
            "id": 'function',
            "name": $(e).val()+' function',
            "scope": "",
            "type": "editor",
            "direction": "",
            "field": {
              "type": "editor",
              "values": {
                "function": 
                  'function' in this.values ?
                  this.values.function :
                    $(e).val() == 'filter' ?
                      "// get element with this.getValue();\r// return a boolean with this.setOutput(bool);\rvar x = this.getValue();" :
                      "// get element with this.getValue();\r// return a value with this.setValue(val);\rvar x = this.getValue();" 
              }
            }
          }
          var template = Handlebars.compile('{{dynamic \'genericEndpoint\' this}}');
          this.elem.find('.endpoint-field[data-eid="op"]')
            .parent()
            .after(template(endpoint));

          initEditor(this.elem.find(".editor"));

          $('#'+this.id+' .editor').each((i, e) => {
            var editor = ace.edit($(e).attr('id'));
            var startDate = -1;
            var node = this
            editor.session.on('change', function(f) {
              node.values[$(e).data('eid')] = editor.getValue().trim()
              var endDate = new Date();
              var seconds = (endDate - startDate) / 1000;
              if(seconds >= 5 || startDate < 0) {
                setTimeout(() => saveScene(), 2000)
                startDate = new Date();
                onDesync();
              }
            });
            node.values[$(e).data('eid')] = editor.getValue().trim()
          });
        }

        this.repaintEndpoints();
        onDesync();
        saveScene();
      });
    });

    this.elem.find('.help').click((e) => {
			$.confirm({
				animation: 'none',
				title: 'map & filter help',
				backgroundDismiss: true,
				columnClass: 'col-md-6',
				content: `
				<h6><b>Map usage<b><h6>
        <div>
  				Write some JavaScript in the editor below. 
					Use <tt>this.getValue()</tt> to get the current value value and <tt>this.setValue(val)</tt> to set the return value.
        </div>
        
        <h6><b>Filter usage<b><h6>
        <div>
					Write some JavaScript in the editor below. 
					Use <tt>this.getValue()</tt> to get the current value value and <tt>this.setValue(bool)</tt> to indicate wether the value should be included.
				</div>
				`,
				buttons: {
					ok: () => {}
				}
			});
		})
  }
}