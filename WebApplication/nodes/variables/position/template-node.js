class PositionNode extends ArrayableNode {
  constructor(params) { super("position", params); }
  populateArrayInput(e, i) {
    var form = this.elem.find('form:nth-child('+(i+1)+')');
    form.find('input[name="x"]').val(e[0]);
    form.find('input[name="y"]').val(e[1]);
    form.find('input[name="z"]').val(e[2]);
  }
  arrayValuesChange(id) {
    this.values[id] = [];
    this.elem.find('.endpoint-field[data-eid="'+id+'"] form').each((i, f) => {
      var data = $(f).serializeArray();
      this.values[id].push(data.map(d => d.value ));
    });
  }
  addChangeEvents() {
    super.addChangeEvents('form');
  }
}