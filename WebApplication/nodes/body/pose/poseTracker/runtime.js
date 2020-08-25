class PoseTrackerNode extends Node {
  constructor(id) {
    super(id);
    this.pose = new Output();
    artk.registerPoseTracker(this.onPose.bind(this));
  }

  update(ctx) {
    super.update(ctx);
  }

  onPose(poses) {
    this.pose.value = poses
  }
}