class PathNode extends Node {
  constructor(id) {
    super(id);
    this.points = new Input();
    this.color = new Input();
  }

  update(ctx) {
    super.update(ctx);
    if(this.points.isDirty || this.color.isDirty) {
      artk.updatePath(this.id, this.points.value, this.color.get(0));
    }

    // TODO: this is only a fake path for testing
    // artk.updatePath(this.id, [[0, 0, 0], [1,1,1], [0,0,1]], {r:255, g:0, b:0});
  }
}