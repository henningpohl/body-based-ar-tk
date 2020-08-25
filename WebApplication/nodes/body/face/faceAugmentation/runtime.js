class FaceAugmentationNode extends Node {
  constructor(id) {
    super(id);
    this.face = new Input();
    this.lip_color = new Input();
    this.eye_shade = new Input();
    this.blushing = new Input();
  }

  update(ctx) {
    super.update(ctx);
    
    if(this.face.isDirty || this.lip_color.isDirty || this.eye_shade.isDirty || this.blushing.isDirty) {
      
      artk.augmentFace(this.id, this.face.get(0), this.lip_color.value, this.eye_shade.value, this.blushing.value);
    }
  }
}