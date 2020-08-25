class PoseDetectionNode extends Node {
  constructor(id, pose) {
    super(id);
    this.input = new Input();
    this.output = new Output();
    this.track_pose = pose;
    this.isDirty = false;
  }

  update(ctx) {
    super.update(ctx);
    if(!this.input.isDirty && !this.isDirty) { return; }
    this.output.value = this.input.value.filter((pose) =>  {
      switch(this.track_pose) {
        case "hand_raised":
          return this.isHandUp(pose);
        case "hand_not_raised":
          return this.isHandDown(pose);
        default:
          return false;
      }

      /*
      var detected = this.getPoseTypes(p);
      console.log("detected", detected, Object.keys(p.getKeypoint('leftWrist')), p.getKeypoint('leftWrist'))
      return detected.includes(this.track_pose)
      */
    });
  }

  setValue(name, value) {
    this.track_pose = value[0];
    this.isDirty = true;
  }

  isHandUp(pose, up = true) {
    let leftWrist = pose['keypoints']['leftWrist'].position;
    let leftElbow = pose['keypoints']['leftElbow'].position;
    let rightWrist = pose['keypoints']['rightWrist'].position;
    let rightElbow = pose['keypoints']['rightElbow'].position;

    
    let leftUp = leftWrist.y > (leftElbow.y + 0.2);
    let rightUp = rightWrist.y > (rightElbow.y + 0.2);
    console.log(up ? 'up' : 'down', leftWrist.y, leftElbow.y, rightWrist.y, rightElbow.y, leftUp, rightUp)
    return leftUp || rightUp;
  }

  isHandDown(pose) {
    return !this.isHandUp(pose, false);
  }

  getPoseTypes(pose) {
    var found_poses = [];
    var defs = this.getPoseDefinitions();
    /* run the pose detector */
    for (const pose_def in defs) {
      var def = defs[pose_def]; 
      var found = def.detect(pose)
      if(found) { found_poses.push(pose_def); }
    }
    return found_poses;
  }

  getPoseDefinitions() {
    /* 
      nose, leftEye, rightEye, leftEar, rightEar, leftShoulder, rightShoulder, 
      leftElbow, rightElbow, leftWrist, rightWrist, leftHip, rightHip, leftKnee, 
      rightKnee, leftAnkle, rightAnkle
    */
    return {
      'hand_raised': new PoseQuery()
        .orQuery([
          new PoseQuery()
            .higher('leftWrist', 'leftElbow'),
          new PoseQuery()
            .higher('rightWrist', 'rightElbow'),
        ])
      ,
      'hand_not_raised': new PoseQuery()
        .andQuery([
          new PoseQuery()
            .lower('leftWrist', 'leftElbow'),
          new PoseQuery()
            .lower('rightWrist', 'rightElbow'),
        ])
      ,
      'sitting': new PoseQuery()
        .andQuery([
          new PoseQuery()
            .lower('leftAnkle', 'leftKnee'),
          new PoseQuery()
            .range('leftKnee', 'leftHip', 'lower', 5),
          new PoseQuery()
            .lower('leftHip', 'leftShoulder'),
        ])
      ,
    }

  }
}

class PoseQuery {
  
  constructor() {
    this.tree = {}
  }
  
  orQuery(queries) {
    this.tree.bool_op = 'or';
    this.tree.queries = queries;
    return this;
  }
  
  andQuery(queries) {
    this.tree.bool_op = 'and';
    this.tree.queries = queries;
    return this;
  }
  
  threshold(t) { 
    this.tree.threshold = t; 
    return this;
  }

  lower(key0, key1) { return this.positionOperator('lower', key0, key1) }
  higher(key0, key1) { return this.positionOperator('higher', key0, key1) }
  left(key0, key1) { return this.positionOperator('left', key0, key1) }
  right(key0, key1) { return this.positionOperator('right', key0, key1) }
  
  range(key0, key1, fn='lower', t=5) {
    return this
    .orQuery([
      new PoseQuery()
        [fn](key0, key1)
        .threshold(t),
      new PoseQuery()
        [fn](key1, key0)
        .threshold(t)
    ])
  }
  
  positionOperator(op, key0, key1) {
    this.tree.pos_op = op;
    this.tree.args = { 0: key0, 1: key1 }
    return this;
  }
  
  build() { return this.tree; }

  detect(pose) {
    /* in case of boolean operator, compute subquery and collect booleans  */
    if("bool_op" in this.tree) {
      var op = this.tree['bool_op'] == 'or' ? 'some' : 'every';
      return this.tree.queries.map(def=>def.detect(pose))[op](e=>e);
    }
    
    /* in case of binary position operator */
    if("pos_op" in this.tree){
      var key0 = pose.getKeypoint(this.tree.args[0]);
      var key1 = pose.getKeypoint(this.tree.args[1]);
      if( 
        key0 !== undefined && key1 !== undefined &&
        'position' in key0 && 'position' in key1
      ) {
        var threshold = this.tree['threshold'] || 0
        var bool = (this.tree['pos_op'] == 'distance') ?
          key0.position.distance(key1.position) > threshold :
          key0.position[this.tree['pos_op']](key1.position, threshold);
        return bool;
      }
      return false;
    }
  }
}