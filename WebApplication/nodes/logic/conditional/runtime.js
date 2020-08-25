class ConditionalNode extends Node {
  constructor(id) {
    super(id);
    this.input = new Input();
    this.onif = new Input();
    this.onelse = new Input();
    this.output = new Output();
  }
  
  update(ctx) {
    super.update(ctx);
    if(!this.input.isDirty) { return; }
    if(this.input.value) {
      this.output.value = this.onif.value
    } else {
      this.output.value = this.onelse.value
    }
  }
}