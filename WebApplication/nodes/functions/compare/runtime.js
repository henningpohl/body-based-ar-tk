class CompareNode extends Node {
  constructor(id, mode) {
    super(id);
    this.mode = mode;
    this.inputA = new Input();
    this.inputB = new Input();
	this.result = new Output();
	this.isDirty = false;
  }

  update(ctx) {
    super.update(ctx);
		if(!this.inputA.isDirty && !this.inputB.isDirty && !this.isDirty) {
			return;
		}

		// most of these probably don't work right now. Also don't support array inputs
		switch(this.mode) {
			case 'lt':
				this.result.value = this.inputA.value < this.inputB.value;
				break;
			case 'gt':
				this.result.value = this.inputA.value > this.inputB.value;
				break;
			case 'leq':
				this.result.value = this.inputA.value <= this.inputB.value;
				break;
			case 'geq':
				this.result.value = this.inputA.value >= this.inputB.value;
				break;
			case 'eq':
				this.result.value = this.inputA.value == this.inputB.value;
				break;
			case 'neq':
				this.result.value = this.inputA.value != this.inputB.value;
				break;
			case 'closer':
				this.result.value =  Position(this.inputA.value).closer(Position(this.inputB.value));
				break;
			case 'further':
				this.result.value =  Position(this.inputA.value).further(Position(this.inputB.value));
				break;
			case 'higher':
				this.result.value =  Position(this.inputA.value).higher(Position(this.inputB.value));
				break;
			case 'lower':
				this.result.value =  Position(this.inputA.value).lower(Position(this.inputB.value));
				break;
			case 'left':
				this.result.value =  Position(this.inputA.value).left(Position(this.inputB.value));
				break;
			case 'right':
				this.result.value =  Position(this.inputA.value).right(Position(this.inputB.value));
				break;
			case 'brighter':
				this.result.value = Color(this.inputA.value).brighter(Color(this.inputB.value));
				break;
			case 'darker':
				this.result.value = Color(this.inputA.value).darker(Color(this.inputB.value));
				break;
		}

		this.isDirty = false;
	}
	
	setValue(name, value) {
		this.op = value[0];
		this.isDirty = true;
	}
}