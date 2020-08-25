class FalseNode extends Node {
  constructor(id) {
    super(id);
    this.output = new Output(false);
  }
  update(ctx) {
    super.update(ctx);
  }
}