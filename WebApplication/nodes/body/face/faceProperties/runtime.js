class FacePropertiesNode extends Node {
  constructor() {
    super();
    this.input = new Input();
    this.position = new Output();
    this.name = new Output();
  }
  
  update(ctx) {
    super.update(ctx);
    
    if(this.input.isDirty) {
      let faces = this.input.value;
      let pos = faces.map(function(x) { return x.position; })
      let name = faces.map(function(x) { return x.name; });
  	  this.position.value = pos;
  	  this.name.value = name;
    }
  }
}