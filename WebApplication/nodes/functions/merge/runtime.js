class MergeNode extends Node {
  constructor(id, num_inputs) {
    super(id);
    for (let i = 0; i < num_inputs; i++) {
      var index = i==0 ? '' : '_'+i;
      var input = new Input();
      this['input'+index] = input;
    }
    this.output = new Output();
    this.num_inputs = num_inputs;
  }

  update(ctx) {
    super.update(ctx);

    var isDirty = false;
    for (let i = 0; i < this.num_inputs; i++) {
      var index = i==0 ? '' : '_'+i;
      var input = this['input'+index];
      isDirty = isDirty || input.isDirty;
    }

    if(isDirty) {
      var output = []
      for (let i = 0; i < this.num_inputs; i++) {
        var index = i==0 ? '' : '_'+i;
        var input = this['input'+index];
        output.push(input.value)
      }

      this.output.value = flat(output);
    }
  }
}