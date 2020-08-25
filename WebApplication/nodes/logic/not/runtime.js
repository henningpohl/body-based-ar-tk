class NotNode extends Node {
  constructor(id) {
    super(id);
    this.input = new Input();
    this.output = new Output();
  }

  update(ctx) {
    super.update(ctx);
    if(!this.input.isDirty) { return; }
    this.output.value = !this.input.value;
  }
}