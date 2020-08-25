// https://stackoverflow.com/questions/12043187/how-to-check-if-hex-color-is-too-black
function isDark(color) {
  var c = color.substring(1, color.length-2);  // strip # and opacity
  var rgb = parseInt(c, 16);   // convert rrggbb to decimal
  var r = (rgb >> 16) & 0xff;  // extract red
  var g = (rgb >>  8) & 0xff;  // extract green
  var b = (rgb >>  0) & 0xff;  // extract blue
  var luma = 0.2126 * r + 0.7152 * g + 0.0722 * b; // per ITU-R BT.709
  return luma < 128;
}
    
class ColorNode extends ArrayableNode {
  constructor(params) {
    super("color", params);
    this.colorpickers = {};
  }
  addChangeEvents() {
    $('#'+this.id+' .btn-colorpicker').each((i, p) => {
      if(!this.colorpickers.hasOwnProperty($(p).attr('id'))) {
        var parent = document.querySelector('#'+$(p).attr('id'));
        var picker = new Picker({
          parent: parent,
          popup: 'top',
          onDone: color => {
            this.onChangeColor(p, color.hex);
            saveScene();
          },
        });
        this.colorpickers[$(p).attr('id')] = picker;
        this.repaintEndpoints();
      }
    });
    this.addDeleteInputEvent();
  }
  addDeleteInputEvent() {
    $('#'+this.id+' .delete-input').each((i, e) => {
      $(e).unbind('click');
      $(e).click(() => {
        var orgEid = $(e).closest('.endpoint-field').data('eid');
        var cid = $(e).siblings('.btn-colorpicker').attr('id');
        $(e).closest('.btn-group').remove();
        delete this.colorpickers[cid];
        this.arrayValuesChange(orgEid);
        saveScene();
        this.repaintEndpoints();
        return false;
      });
    });
  }
  onChangeColor(p, color) {
    if(color != '') {
      $(p).text(color)
      $(p).css('background', color);
      $(p).css('color', isDark(color) ? '#EBEBEB' : '#4E5D6C');
      $(p).data('value', color);
    }
    var orgEid = $(p).closest('.endpoint-field').data('eid');
    this.arrayValuesChange(orgEid);
    this.emitDataChange();
  }
  arrayValuesChange(id) {
    this.values[id] = [];
    this.elem.find('.endpoint-field[data-eid="'+id+'"] .btn-colorpicker').each((i, f) => {
      this.values[id].push($(f).data('value'));
    });
  }
  populateArrayInput(e, i) {
    if(e.split(",").filter(i =>i.length > 0).length == 0) { return; }
    var p = this.elem.find('.btn-group:nth-child('+(i+1)+') .btn-colorpicker');
    if(p.attr('id') in this.colorpickers) {
      this.colorpickers[p.attr('id')].setColor(e, true);
      this.onChangeColor(p, e);
    }
  }
  copyInput(copy, click=false) {
    super.copyInput(copy, click, '.btn-colorpicker');
  }
}