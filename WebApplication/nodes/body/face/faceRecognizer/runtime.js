class FaceRecognizerNode extends Node {
  constructor(id, images) {
    super(id);
    this.in_face = new Input();
    this.out_face = new Output();

    this.images = images;
    for(var key in images) {
      artk.registerFace(
        this.id, 
        images[key]['src'],
        images[key]['caption']);
    }
  }

  update(ctx) {
    super.update(ctx);
    if(this.in_face.isDirty) {
      if(this.in_face.value.length == 0) {
        this.out_face.value = [];
      } else {
        artk.recognizeFaces(
          this.id,
          this.in_face.value,
          this.onRecognizedFaces.bind(this));
      }
    }
  }
  
  onRecognizedFaces(faces) {
    this.out_face.value = faces;
  }
}