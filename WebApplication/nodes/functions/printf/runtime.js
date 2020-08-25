class PrintfNode extends Node { 
 
  constructor(id, format, num_inputs) {
    super(id);

    this.format = format;
    this.num_inputs = num_inputs;
    this.isDirty = false;

    for (let i = 0; i < num_inputs; i++) {
      var index = i==0 ? '' : '_'+i;
      var input = new Input();
      this['input'+index] = input;
    }
    this.output = new Output();
  }
  
  update(ctx) {
    super.update(ctx);

    var isDirty = this.isDirty;
    if(!isDirty) {
      for (let i = 0; i < this.num_inputs; i++) {
        var index = i==0 ? '' : '_'+i;
        var input = this['input'+index];
        isDirty = isDirty || (input.isDirty && input.value !== undefined);
      }
    }

    if(isDirty) {
      var values = []
      for (let i = 0; i < this.num_inputs; i++) {
        var index = i==0 ? '' : '_'+i;
        var input = this['input'+index];
        values.push(input.value)
      }

      values = flat(values).filter(o => o);
      try {
        this.output.value = sprintf(this.format, ...values);
      } catch (error) {
        this.output.value = this.format.replace('%d', '0');
      }
      // console.log(this.output.value);
    }
    this.isDirty = false;
  }

  setValue(name, value) {
    this.format = value[0][0]
    this.isDirty = true;
  }
}