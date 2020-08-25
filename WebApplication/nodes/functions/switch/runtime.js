class SwitchNode extends Node { 
  constructor(id, cases) {
    super(id);

    this.input = new Input();

    this.cases = []
    for (let i = 0; i < cases.length; i++) {
      var n = cases[i];
      var index = i==0 ? '' : '_'+i;
      var switchcase = new Input();
      this['case'+index] = switchcase;
      this.cases[i] = n;
    }

    this.output = new Output();
  }

  update(ctx) {
    super.update(ctx);

    if(this.input.isDirty) {
      for (let i = 0; i < this.cases.length; i++) {
        var _switch = {
          x: this.input.value,
          fn: this.cases[i]
        }
        if(this.cases[i] == 'default' || _switch.fn()) {
          var index = i==0 ? '' : '_'+i;
          var input = this['case'+index];
          this.output.value = input.value;
          break;
        }
      }

    }
  }
}