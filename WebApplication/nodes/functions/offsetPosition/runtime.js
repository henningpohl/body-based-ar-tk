class OffsetPositionNode extends Node {
  constructor(id, offset) {
    super(id);

    this.input = new Input();
    this.output = new Output();

    this.offset = offset;
    this._angle = {
      'above': -0.5,
      'below':  0.5,
      'left':  -0.5,
      'right':  0.5
    } 
    this.angle = this._angle[offset];
    artk.registerUserTracker(this.onMove.bind(this));
  }
  
  update(ctx) {
    super.update(ctx);
  }

  onMove(pos, forward, up, right) {
    if(this.input.value === undefined) { return; }
    this.output.value = this.input.value.map(p => {
      let dir = pos.to(p);
      let invDist = 1/(pos.distance(p) + 0.01);
      let angle = this.angle * invDist; // todo: maybe clamp in some nice way
      let m = undefined;
      if(this.offset == 'left' || this.offset == 'right') {
        m = Matrix.fromAxisAngle(up, angle);
      } else if(this.offset == 'above' || this.offset == 'below') {
        m = Matrix.fromAxisAngle(right, angle);
      }
      let dir2 = m.transform(dir)
      return pos.add(dir2);
    });
  }

  setValue(name, value) {
    this[name] = value
    this.angle = this._angle[value];
  }

}