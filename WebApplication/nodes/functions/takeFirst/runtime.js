class TakefirstNode extends Node { 
 constructor(id) {
    super(id);
    this.input = new Input();
    this.output = new Output();
  }

  update(ctx) {
    super.update(ctx);

    if(this.input.isDirty) {
      this.output.value = this.input.get(0);
    }
  }
 }