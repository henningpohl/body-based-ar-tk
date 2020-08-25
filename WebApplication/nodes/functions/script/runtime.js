class ScriptNode extends Node {
  constructor(id, setupFunc, loopFunc, inputNames) {
    super(id);

    this.inputs = {};
    for (let i = 0; i < inputNames.length; i++) {
      var n = inputNames[i];
      var index = i==0 ? '' : '_'+i;
      var input = new Input();
      this['input'+index] = input;
      this.inputs[n] = input;
    }
    this.output = new Output();
    
    this.script = {
      setup: setupFunc,
      loop: loopFunc,
      hasValue: inName => this.inputs[inName].isDirty,
      getValue: inName => this.inputs[inName].value,
      setOutput: value => this.output.value = value,
    };
    this.script.setup();
  }

  update(ctx) {
    super.update(ctx);
    this.script.loop(ctx);
  }
}