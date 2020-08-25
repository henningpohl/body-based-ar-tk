class OrNode extends Node {
  constructor(id) {
    super(id);
    this.inputA = new Input();
    this.inputB = new Input();
    this.output = new Output();
  }

  update(ctx) {
    super.update(ctx);
    if(!this.inputA.isDirty && !this.inputB.isDirty) { return; }
    this.output.value = this.inputA.value || this.inputB.value;
  }
}