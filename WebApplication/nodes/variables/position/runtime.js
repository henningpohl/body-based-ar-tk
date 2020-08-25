class PositionNode extends Node {
  constructor(id, position) {
    super(id);
    
    if(Array.isArray(position)) {
    	this.position = new Output(position.map(function(x) {
    		return new Position(x);
    	}));
    } else {
    	this.position = new Output(new Position(position));
	}
  }

  update(ctx) {
    super.update(ctx);
  }
  setValue(name, value) {
    if(name == 'position') {
      if(Array.isArray(value)) {
        this.position.value = value.map(function(x) { return new Position(x); });
      } else {
        this.position.value = new Position(value);
      }
    }
  }
}