class MyselfNode extends Node {
  constructor(id) {
    super(id);
    this.position = new Output();
    this.forward = new Output();
    this.up = new Output();
    this.right = new Output();

    artk.registerUserTracker(this.onMyself.bind(this));
  }

  update(ctx) {
    super.update(ctx);
  }

  onMyself(pos, forward, up, right) {
  	this.position.value = pos;
  	this.forward.value = forward;
  	this.up.value = up;
  	this.right.value = right;
  }
}