class RandomNode extends Node {
  constructor(id, min, max) {
    super(id);

    this.color = new Output(randomColor());
    this.number = new Output(randomInt(min, max));
    this.position = new Output(randomPosition())
  }

  update(ctx) {
    super.update(ctx);
  }
}