class PlaySoundNode extends Node { 
  constructor(id, file_id, file_path) {
    super(id);
    
    this.trigger = new Input();
    this.position = new Input();

    this.file_id = file_id
    this.file_path = file_path
    
    artk.loadSound(this.id, file_path);
  }
  
  update(ctx) {
    super.update(ctx);
    if(this.trigger.value === true) {
      if(this.position.isConnected) {
        for(var i = 0; i < this.position.valueCount; ++i) {
          var pos = this.position.get(i);
          if(pos === undefined) {
            continue;
          }
          artk.playSound(this.id, pos);
        }
      } else {
        artk.playSound(this.id);
      }
    }
  }
}