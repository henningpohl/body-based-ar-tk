class SpriteNode extends Node {
  constructor(id) {
    super(id);
    this.image = new Input();
    this.position = new Input();
    this.show = new Input();
    this.maxCount = 0;
  }

  update(ctx) {
    super.update(ctx);
    if(this.image.value === undefined) { return; }
    if((this.image.isDirty || this.position.isDirty || this.show.isDirty) && this.position.value !== undefined) { 
      var show = this.show.isConnected ? this.show.value : true;
      this.maxCount = Math.max(this.maxCount, this.image.valueCount, this.position.valueCount);
      for(var i = 0; i < this.maxCount; ++i) {
        let image = this.image.get(i);
        let pos = this.position.get(i);
        artk.updateSprite(this.id + '-' + i, pos, image, show);
      }
    }
  }
}