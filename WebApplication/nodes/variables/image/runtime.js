class ImageNode extends Node { 
 constructor(id, image_src) {
    super(id);
    this.image = new Output();
    artk.loadImage(this.id, image_src);
    this.image.value = this.id;
  }

  update(ctx) {
    super.update(ctx);
  }
}