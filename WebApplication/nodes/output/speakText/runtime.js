class SpeakTextNode extends Node { 
  constructor(id) {
    super(id);
    
    this.text = new Input();
    this.position = new Input();
    this.trigger = new Input();
  }

  update(ctx) {
    super.update(ctx);
    
    // TODO: decide on behavior when
    // there is already text being spoken
    if(this.trigger.isConnected) {
      if(this.trigger.isDirty) {
        if(this.trigger.value == true) {
          this.speak(this.text.value);
        }
      }
    } else {
      if(this.text.isDirty) {
    	  this.speak(this.text.value);
      }
    }
  }
  
  speak(text) {
    if(this.position.isConnected) {
      artk.speakText(text, this.position.value);
    } else {
      artk.speakText(text); 
    }
  }
}