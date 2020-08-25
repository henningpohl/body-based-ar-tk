class MultiArrayOpNode extends Node {
  constructor(id, op, fn) {
    super(id);

    this.input = new Input();
    this.output = new Output();

    /* op is map or filter */
    this.op = op;
    this.script = {
      fn: fn,
      getValue: () => { return; },
      setOutput: (v) => { return; }
    };
  }
  
  update(ctx) {
    super.update(ctx);

    if(!this.input.isDirty) { return; }
    this.output.value = this.input.value[this.op]((x) => {
      this.script.getValue = () => { return x; };
      var result;
      this.script.setOutput = (v) => { result = v; };
      this.script.fn();
      return result;
    });
  }
}