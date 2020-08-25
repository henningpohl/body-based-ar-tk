class AheadNode extends Node {
  constructor(id) {
    super(id);
    this.meters = new Input();
    this.position = new Output();

    artk.registerUserTracker(this.onMyself.bind(this));
  }

  update(ctx) {
    super.update(ctx);
  }

  onMyself(pos, forward, up, right) {
    var delta = forward;
    if(this.meters.isConnected) {
      delta = forward.multiply(this.meters.value);
    } 
    
    this.position.value = pos.add(delta);
  }
}