class NumberNode extends Node {
  constructor(id, number) {
    super(id);
    this.number = new Output(number);
  }

  update(ctx) {
    super.update(ctx);
  }
}