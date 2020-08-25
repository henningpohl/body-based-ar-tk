function initEditor(elem) {
  var editor = ace.edit(elem.attr('id'));
  editor.session.setOptions({ tabSize: 2, useSoftTabs: true });
  editor.setTheme("ace/theme/monokai");    
  editor.session.setMode("ace/mode/javascript");
  editor.session.setNewLineMode("unix");
  editor.clearSelection();
  var startDate = -1;
  editor.session.on('change', function() {
    var endDate = new Date();
    var seconds = (endDate - startDate) / 1000;
    if(seconds >= 5 || startDate < 0) {
      setTimeout(() => saveScene(), 2000)
      startDate = new Date();
      onDesync();
    }
  });
}

class ScriptNode extends ArrayableNode {
  constructor(params) { super("script", params || {}) }
  init() {
    $('#'+this.id+' .editor').each((i, e) => initEditor($(e)) );
    super.init();
  }
  addChangeEvents() {
    $('#'+this.id+' input, #'+this.id+' select').each((i, e) => {
      $(e).unbind('change');
      $(e).change(() => {
        var eid = $(e).closest('.endpoint-field').data('eid');
        this.arrayValuesChange(eid);
        this.emitDataChange();
        saveScene();
      });
    });

    $('#'+this.id+' .editor').each((i, e) => {
      var editor = ace.edit($(e).attr('id'));
      var startDate = -1;
      editor.session.on('change', f => {
        var val = editor.getValue().trim()
        val = val.replace(/[\n]/g, "\r");
        this.values[$(e).data('eid')] = val

        var endDate = new Date();
        var seconds = (endDate - startDate) / 1000;
        if(seconds >= 5 || startDate < 0) {
          setTimeout(() => saveScene(), 2000)
          startDate = new Date();
          onDesync();
        }
      });
    });

    this.addDeleteInputEvent();
  }
  addDeleteInputEvent() {
    super.addDeleteInputEvent('.list-group-item', false);
  }
  copyInput(copy, click=false, elements='input') {
    super.copyInput(copy, click, elements, false);
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
