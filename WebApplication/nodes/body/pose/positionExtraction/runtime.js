class PositionExtractionNode extends Node {
  constructor(id, keypoint) {
    super(id);
    this.input = new Input();
    this.output = new Output();
    this.keypoint = keypoint;
    this.isDirty = false;
  }

  update(ctx) {
    super.update(ctx);

    if(!this.input.isDirty && !this.isDirty) { return; }
    this.output.value = flat(
        this.input.value
        .filter(v => v) 
        .map(p => {
          var k = p.getKeypoint(this.keypoint);
          if(k == null) { return null; }
          return k.position;
        })
      ).filter(v => v);
    this.isDirty = false;
  }

  setValue(name, value) {
    this.keypoint = value[0];
    this.isDirty = true;
  }
}