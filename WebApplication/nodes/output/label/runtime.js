class LabelNode extends Node {
  constructor(id) {
    super(id);
    this.string = new Input();
    this.position = new Input();
    this.color = new Input();
    this.show = new Input();
    this.maxCount = 0;
  }

  update(ctx) {
    super.update(ctx);
    if(this.string.value === undefined) { return; }
    if((this.string.isDirty || this.position.isDirty || this.color.isDirty || this.show.isDirty) && this.position.value !== undefined) { 
      var show = this.show.isConnected ? this.show.value : true;
      this.maxCount = Math.max(this.maxCount, this.string.valueCount, this.position.valueCount, this.color.valueCount);
      for(var i = 0; i < this.maxCount; ++i) {
        let text = this.string.get(i);
        let pos = this.position.get(i);
        let color = this.color.get(i, true);
        artk.updateLabel(this.id + '-' + i, pos, color, text, show);
      }
    }
  }
}