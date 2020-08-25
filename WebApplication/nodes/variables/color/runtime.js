class ColorNode extends Node {
  constructor(id, color) {
    super(id);

    if(Array.isArray(color)) {
    	this.color = new Output(color.map(function(x) {
    		return new Color(x);
    	}));
    } else {
    	this.color = new Output(new Color(color));
	  }
  }

  update(ctx) {
    super.update(ctx);
  }

  setValue(name, value) {
    if(name == 'color') {
      if(Array.isArray(value)) {
        this.color.value = value.map(function(x) { return new Color(x); });
      } else {
        this.color.value = new Color(value);
      }
    }
  }
}