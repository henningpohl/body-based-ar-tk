class EmotionNode extends Node {
  constructor(id) {
    super(id);
    this.face = new Input();
    this.pose = new Input();
    this.output = new Output();
  }
  
  update(ctx) {
    super.update(ctx);
    if(this.face.isDirty) {
      artk.getEmotionForFaces(
        this.face.value, 
        this.onEmotions.bind(this)
      );
    } else if(this.pose.isDirty) {
      artk.getEmotionForPoses(
        this.pose.value, 
        this.onEmotions.bind(this)
      );
    }
  }
  
  getEmoFunc(emo) {
    return function(x) {
      if(emo == x) {
        return 1.0;
      } else {
        return 0.0;
      }  
    }
  }
  
  onEmotions(emotions) {
    if (Array.isArray(emotions) && emotions.length > 0) {
      emotions = emotions[0];
    }
    this.output.value = new Emotion(emotions, 1);
  }
}