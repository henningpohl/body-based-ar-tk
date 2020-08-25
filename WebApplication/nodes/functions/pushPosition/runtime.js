class PushPositionNode extends Node { 
  constructor(id, offset) {
    super(id);

    this.input = new Input();
    this.output = new Output();

    this.offset = new Position(0, 0, 0.2);
  }
  
  update(ctx) {
    super.update(ctx);
    
    if(this.input.isDirty) {
      this.output.value = this.input.get(0).add(this.offset);
    }
  }
}