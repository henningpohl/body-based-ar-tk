class BargraphNode extends Node {
  constructor(id) {
    super(id);
    this.string = new Input();
	  this.number = new Input();
    this.color = new Input();
	  this.position = new Input();
  }

  update(ctx) {

    super.update(ctx);
	
    if(this.string.isDirty || this.number.isDirty || this.position.isDirty || this.color.isDirty) {
      let pos = this.position.value;
      let node = this;
      if(this.number.value == undefined || !(Array.isArray(this.number.value) && this.number.value.length)) { return; }
      let data = this.number.value.map(function(v, i) {
        return {
          value: v,
          label: node.string.get(i, true),
          color: node.color.get(i, true)
        };
      });
      artk.updateBarplot(this.id, pos, data);
    }
  }
}