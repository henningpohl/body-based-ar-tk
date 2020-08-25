class SingleArrayOpNode extends Node {
  constructor(id, op) {
    super(id);

    this.input = new Input();
    this.output = new Output();

    this.op = op;
    this.isDirty = false;
  }
  
  update(ctx) {
    super.update(ctx);

    if(!this.input.isDirty && !this.isDirty) { return; }
    
    switch (this.op) {
      case "min":
        this.output.value = Math.min(...this.input.value);
        break;
      case "max":
        this.output.value = Math.max(...this.input.value);
        break;
      case "sum":
        this.output.value = this.input.value.reduce((a, b) => a + b, 0);
        break;
      case "count":
        this.output.value = this.input.value.length;
        break;
    }
  }

  setValue(name, value) {
    this.op = value[0];
    this.isDirty = true;
  }
}