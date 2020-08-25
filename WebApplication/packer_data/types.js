class Color { 
  constructor(v) {
    this.r = 255;
    this.g = 255;
    this.b = 255;
    this.a = 255;

    if(typeof(v) === 'string') {
      let s = v.replace('#', '');
      if(s.length >= 6) {
        this.r = parseInt(s.substring(0, 2), 16);
        this.g = parseInt(s.substring(2, 4), 16);
        this.b = parseInt(s.substring(4, 6), 16);
      } 
      if(s.length >= 8) {
        this.a = parseInt(s.substring(6, 8), 16);
      }
    } else if(typeof(v) === 'object') {
      if(v.r !== undefined) {
        this.r = v.r;
      }
      if(v.g !== undefined) {
        this.g = v.g;
      }
      if(v.b !== undefined) {
        this.b = v.b;
      }
      if(v.a !== undefined) {
        this.a = v.a;
      }
    }
  }

  get brightness() {
    return 0.2126 * this.r + 0.7152 * this.g + 0.0722 * this.b;
  }

  toString() {
    return '#' + 
      toHexString(this.r).padStart(2, '0') + 
      toHexString(this.g).padStart(2, '0') + 
      toHexString(this.b).padStart(2, '0') + 
      toHexString(this.a).padStart(2, '0');
  }

  brighter(b) { return this.brightness > b.brightness; }
  darker(b) { return this.brightness < b.brightness; }
}

class Position {
  constructor(x, y, z) {
    if(x !== undefined && y !== undefined && z !== undefined) {
      this.x = x;
      this.y = y;
      this.z = z;
      return;
    }
    
    switch(true) {
      case Array.isArray(x):
        this.x = Number(x[0]);
        this.y = Number(x[1]);
        this.z = Number(x[2]);
        break;
      case x instanceof Position:
        this.x = x.x;
        this.y = x.y;
        this.z = x.z;
        break;
      case typeof(x) === 'object' && 'x' in x && 'y' in x && 'z' in x:
        this.x = x.x;
        this.y = x.y;
        this.z = x.z;
        break;
      default:
        this.x = 0.0;
        this.y = 0.0;
        this.z = 0.0;
    }
  }
  
  toString() {
    return this.x + ", " + this.y + ", " + this.z;
  }

  distance(b) {
    var a = [this.x, this.y, this.z];
    var b = [b.x, b.y, b.z];
    var sum = 0; var n;
    for (n = 0; n < a.length; n++) {
      sum += Math.pow(a[n] - b[n], 2)
    }
    return sum;
  }

  cross(b) {
    return new Position(
      this.y * b.z - this.z * b.y,
      this.z * b.x - this.x * b.z,
      this.x * b.y - this.y * b.x
    );
  }

  dot(b) {
    return this.x * b.x + this.y * b.y + this.z * b.z; 
  }

  to(b) {
    return new Position(b.x - this.x, b.y - this.y, b.z - this.z);
  }

  add(b) {
    if(b instanceof Position) {
      return new Position(this.x + b.x, this.y + b.y, this.z + b.z);
    } else {
      return new Position(this.x + b, this.y + b, this.z + b);
    }
  }

  multiply(b) {
   if(b instanceof Position) {
      return new Position(this.x * b.x, this.y * b.y, this.z * b.z);
    } else {
      return new Position(this.x * b, this.y * b, this.z * b);
    } 
  }

  get length() {
    return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
  }

  normalize() {
    let l = this.length;
    if(l != 0) {
      this.x /= l;
      this.y /= l;
      this.z /= l;
    }
  }

  /* TODO: maybe rework to use "myself"-position, instead of origin */
  /* b = other position, t = "more than t away from b" */
  lower(b, t=0) { return this.y - t < b.y; }
  higher(b, t=0) { return this.y + t > b.y; }
  left(b, t=0) { return this.x - t < b.x; }
  right(b, t=0) { return this.x + t > b.x; }
  closer(b, t=0) { return this.z - t < b.z; }
  further(b, t=0) { return this.z + t > b.z; }
}

class Matrix {
  constructor(values) {
    if(values === undefined) {
      this.values = [0, 0, 0,  0, 0, 0,  0, 0, 0];
    } else {
      this.values = values;
    }
  }

  transform(v) {
    return new Position(
      this.values[0] * v.x + this.values[1] * v.y + this.values[2] * v.z,
      this.values[3] * v.x + this.values[4] * v.y + this.values[5] * v.z,
      this.values[6] * v.x + this.values[7] * v.y + this.values[8] * v.z
    );
  }

  // https://en.wikipedia.org/wiki/Rotation_matrix#Conversion_from_and_to_axis%E2%80%93angle
  static fromAxisAngle(axis, angle) {
    let c = Math.cos(angle);
    let s = Math.sin(angle);
    return new Matrix([
      c + axis.x * axis.x * (1 - c),
      axis.x * axis.y * (1 - c) - axis.z * s,
      axis.x * axis.z * (1 - c) + axis.y * s,
      axis.y * axis.x * (1 - c) + axis.z * s,
      c + axis.y * axis.y * (1 - c),
      axis.y * axis.z * (1 - c) - axis.x * s,
      axis.z * axis.x * (1 - c) - axis.y * s,
      axis.z * axis.y * (1 - c) + axis.x * s,
      c + axis.z * axis.z * (1 - c)]);
  }
}

class Face {
  constructor(position) {
    this.position = new Position(position);
    this.name = "Unknown";
  }
}

class Pose {
  constructor(ref, score, keypoints) {
    this.id = ref;
    this.score = score;

    this.keypoints = {};

    var _keypoints = {};
    for (const keypoint in keypoints) {
      if(!isNaN(keypoint)) {
        const i = keypoints[keypoint];
        _keypoints = Object.assign(_keypoints, i);
      }
    }
    if(Object.keys(_keypoints).length === 0) {
      _keypoints = keypoints;
    }
    
    for (const keypoint in _keypoints) {
      const k = _keypoints[keypoint];
      k.position = new Position(k.position);
      this.keypoints[keypoint] = k;
    }
  }

  getKeypoint(keypointLabel) {
    return this.keypoints[keypointLabel];
  }
}

class Emotion {
  constructor(emotion, score) {
    this.emotions = { 
      "Angry": 0, "Disgust": 0,
      "Fear": 0, "Happy": 0, "Neutral": 0,
      "Sad": 0, "Surprise": 0
    }
    this.emotions[emotion] = score;
  }

  getMaxEmotion() {
    return Object.keys(this.emotions).reduce((a, b) => this.emotions[a] > this.emotions[b] ? a : b);
  }

  getEmotionConfidence(emotion) {
    return this.emotions[emotion];
  }
}