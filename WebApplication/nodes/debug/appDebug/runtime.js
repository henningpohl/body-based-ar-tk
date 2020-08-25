class AppDebug extends Node {
  constructor(id) {
    super(id);
    this.dbg = new Input();
  }
  update(ctx) {
    super.update(ctx);
    if(this.dbg.isDirty) {
      artk.debugOut(this.dbg.value);
    }
  }
}