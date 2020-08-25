class ModelNode extends Node { 
  constructor(id, file_id, file_path) {
    super(id);
    
    this.show = new Input();
    this.position = new Input();
    this.orientation = new Input();
    this.scale = new Input();
    this.color = new Input();

    this.file_id = file_id
    this.file_path = file_path
    
    artk.loadModel(this.id, file_path);
  }
  
  update(ctx) {
    super.update(ctx);

    var show = this.show.value === true;
    artk.updateModel(this.id, this.id, show);

    if(this.color.isDirty) {
      artk.updateModelTint(this.id, this.color.get(0));
    }

    if(this.position.isDirty || this.orientation.isDirty || this.scale.isDirty) {
      var position = this.position.isConnected ? this.position.get(0) : new Position(0,0,0);
      var orientation = this.orientation.isConnected ? this.orientation.get(0) : new Position(0,0,0);
      var scale = this.scale.isConnected ? this.scale.get(0) : 1;
      artk.updateModelTransform(this.id, position, orientation, scale);
    }

  }
}