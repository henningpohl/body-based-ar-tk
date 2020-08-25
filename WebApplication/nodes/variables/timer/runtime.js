class TimerNode extends Node {
  constructor(id, images) {
    super(id);
    this.interval = new Input();
    this.active = new Input();
    this.trigger = new Output();
    this.time = 0.0;
  }
  
  update(ctx) {
    super.update(ctx);
    if(this.active.isConnected && this.active.value === false) {
      return;
    }
    
    if(!this.interval.isConnected) {
      return;
    }
    
    this.time += artk.frameTime * 0.001;
    if(this.time >= this.interval.value) {
      this.time -= this.interval.value;
      this.trigger.value = true;
    } else {
      if(this.trigger.value === true) {
        this.trigger.value = false;
      }
    }
  }
}