class EmotionOpNode extends Node { 
  constructor(id, op, conf=null) {
    super(id);

    this.op = op;
    this.conf = conf;

    this.input = new Input();
    this.output = new Output();

    this.isDirty = false;
  }

  update(ctx) {
    super.update(ctx);

    if(!this.input.isDirty && this.isDirty) { return; }

    if(this.input.value === undefined) { return; }
    if(this.op == 'emotion_confidence') {
      this.output.value = this.input.value.getEmotionConfidence(this.conf);
    } else {
      this.output.value = this.input.value.getMaxEmotion();
    }
  }
}