class StringNode extends Node {
  constructor(id, string) {
    super(id);
      this.string = new Output(string);
  }

  update(ctx) {
    super.update(ctx);
    // Just for testing 
    /*
    if(ctx.frame == 500) {
      this.string.value = "New String Value!";
    }
    */
  }
}