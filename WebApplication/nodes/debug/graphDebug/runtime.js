class GraphDebug extends Node {
  constructor(id) {
    super(id);
    this.dbg = new Input();
  }
  update(ctx) {
    super.update(ctx);
    if(this.dbg.isDirty) {
      let msg = JSON.stringify({
        pid: artk.projectID,
        nid: this.id,
        ntype: "graphDebug",
        value: this.dbg.value
      });
      artk.sendToEditor("updatedValue", msg);
    }
  }
}