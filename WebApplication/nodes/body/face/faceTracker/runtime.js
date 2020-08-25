class FaceTrackerNode extends Node {
  constructor(id) {
    super(id);
    this.face = new Output();
    artk.registerFaceTracker(this.onFaces.bind(this));
  }

  update(ctx) {
    super.update(ctx);
  }

  onFaces(faces) {
  	this.face.value = faces;
  }
}