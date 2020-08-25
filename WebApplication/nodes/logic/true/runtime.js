class TrueNode extends Node {
  constructor(id) {
    super(id);
    this.output = new Output(true);
  }
  update(ctx) {
    super.update(ctx);
  }
}