// Run through https://documentation.js.org for docs

// NOTE: the actual functionality is implemented in the Hololens application. Here the boilerplate code is shown and the code is able to help debugging.
let artk = {
     /**
      * Renders one or more labels in 3d space
      * @param {string} id The id of the label
      * @param {Position} pos The 3d position of the label
      * @param {Color} color The color of the label text
      * @param {string} label The label text
      * @param {boolean} show show/hide the label
      */
    updateLabel: function(id, pos, color, text, show) {
      console.log("updateLabel:", id, pos, color, text, show);
    },

    /**
      * Renders one or more sprites in 3d space
      * @param {string} id The id of the label
      * @param {Position} pos The 3d position of the label
      * @param {image} image The image to show on the sprite
      * @param {boolean} show show/hide the label
      */
    updateSprite: function(id, pos, image, show) {
      console.log("updateSprite:", id, pos, image, show);
    },    

    /**
     * Renders a barplot anchored in 3d space
     * @param {string} id The id of the barplot
     * @param {Position} pos The 3d position of the barplot
     * @param data An array of objects with 'value' (number), 'label' (string, optional), and 'color' (Color, optional) fields
     */
    updateBarplot: function(id, pos, data) {
      console.log("updateBarplot:", id, pos, data);
    },

    /**
     * Renders a colored path in 3d space
     * @param {string} id The id of the path
     * @param {Position} points An array of points defining the path
     * @param {Color} color The color of the path
     */
    updatePath: function(id, points, color) {
      console.log("updatePath:", id, color, points);
    },

    /**
      * Load a model into the runtime for later use
      * @param {string} id Identifier used to later refer to this model
      * @param {string} data The link to the model data
      */
    loadModel: function(id, url) {
       console.log(`Model loaded (${id}) from ${url}`);
    },

    /**
     * Renders a 3d model in 3d space
     * @param {string} id Identifier of the is model instance
     * @param {string} model Identifier of the model to show
     * @param {boolean} show show/hide the model (optional)
     */
    updateModel: function(id, model, show) {
      console.log('updateModel:', id, model, show); 
    },

    /**
     * Change the tint of a model
     * @param {string} id Identifier of this model instance
     * @param {Color} tint Tint to apply to the model
     */
    updateModelTint: function(id, tint) {
      console.log('updateModelTint:', id, tint); 
    },

    /**
     * Change the 3d position, orientation and scale of a model
     * @param {string} id Identifier of this model instance
     * @param {Position} position Spatial location of the model
     * @param {Position} orientation x/y/z rotation of the model
     * @param {number} scale Scale of the model
     */
    updateModelTransform: function(id, position, orientation, scale) {
      console.log('updateModelTransform:', id, position, orientation, scale); 
    },

    /**
     * Register callback to receive user position and orientation
     * @param {function} func The function to call once the position of the user is available
     */
    registerUserTracker: function(func) {
      let time = 0;
      window.setInterval(function() {
        time += 1000.0;
        var pos = new Position([Math.sin(time / 500.0), 0, Math.cos(time / 500.0)]);
        var forward = new Position([0,0,1]);
        var up = new Position([0,1,0]);
        var right = new Position([1,0,0]);
        func(pos, forward, up, right);
      }, 1000);
    },

    /**
     * Register callback to receive face tracking information
     * @param {function} func The function to call once face tracker information is available
     */
     registerFaceTracker: function(func) {
      window.setInterval(function() {
        var face = new Face([0,1,1]);
        func([face]);
      }, 1000);
     },

    /**
     * Register callback to receive pose detection information
     * @param {function} func The function to call once pose detection information is available
     */
    registerPoseTracker: function(func) {

      /* sample data */
      window.setInterval(function() {
        keypoints0 = {
          nose: { score: 0.9, position: new Position(0, 1, 0) },
          leftElbow: { score: 0.8, position: new Position(2, 1, 0) }
        };
        keypoints1 = {
          leftElbow: { score: 0.7, position: new Position(5, 2, 0) }
        };
        keypoints2 = {
          leftElbow: { score: 0.7, position: new Position(4, 2, 0) },
          leftWrist: { score: 0.7, position: new Position(5, 4, 0) },
          rightElbow: { score: 0.7, position: new Position(2, 2, 0) },
          rightWrist: { score: 0.7, position: new Position(5, 1, 0) }
        };
        keypoints3 = {
          leftElbow: { score: 0.7, position: new Position(5, 6, 0) },
          leftWrist: { score: 0.7, position: new Position(5, 1, 0) },
          rightElbow: { score: 0.7, position: new Position(2, 2, 0) },
          rightWrist: { score: 0.7, position: new Position(5, 4, 0) }
        };
        keypoints4 = {
          leftElbow: { score: 0.7, position: new Position(1, 2, 0) },
          leftWrist: { score: 0.7, position: new Position(5, 4, 0) },
          rightElbow: { score: 0.7, position: new Position(2, 2, 0) },
          rightWrist: { score: 0.7, position: new Position(5, 4, 0) }
        };
        keypoints5 = {
          leftElbow: { score: 0.7, position: new Position(2, 2, 0) },
          leftWrist: { score: 0.7, position: new Position(5, 1, 0) },
          rightElbow: { score: 0.7, position: new Position(2, 2, 0) },
          rightWrist: { score: 0.7, position: new Position(5, 1, 0) }
        };
        keypoints6 = {
          leftAnkle: { score: 0.7, position: new Position(0, 1, 0) },
          leftKnee: { score: 0.7, position: new Position(0, 4, 0) },
          leftHip: { score: 0.7, position: new Position(0, 5, 0) },
          leftShoulder: { score: 0.7, position: new Position(0, 10, 0) }
        };
        keypoints7 = {
          leftAnkle: { score: 0.7, position: new Position(0, 7, 0) },
          leftKnee: { score: 0.7, position: new Position(0, 4, 0) },
          leftHip: { score: 0.7, position: new Position(0, 5, 0) },
          leftShoulder: { score: 0.7, position: new Position(0, 10, 0) }
        };
        var poses = [
          // { PoseID: 0, PoseScore: 0.7, keypoints: keypoints0 },
          // { PoseID: 1, PoseScore: 0.7, keypoints: keypoints1 },
          { PoseID: 'raised_left', PoseScore: 0.7, keypoints: keypoints2 },
          { PoseID: 'raised_right', PoseScore: 0.7, keypoints: keypoints3 },
          { PoseID: 'raised_both', PoseScore: 0.7, keypoints: keypoints4 },
          { PoseID: 'raised_none', PoseScore: 0.7, keypoints: keypoints5 },
          // { PoseID: 'sitting', PoseScore: 0.7, keypoints: keypoints6 },
          // { PoseID: 'not_sitting', PoseScore: 0.7, keypoints: keypoints7 },
        ];
        poses = poses.map(p => {
          return new Pose(p.PoseID, p.PoseScore, p.keypoints)
        });
        func(poses);
      }, 1000);
    },

     /**
      * Register a face image and a name with the face recognizer
      * @param {string} id The id of the face recognizer
      * @param {string} imgUrl Link to the image
      * @param {string} name Name of who is shown in that image
      */
     registerFace: function(id, imgUrl, name) {
       console.log("Registered face:", name, imgUrl) ;
     },

     /**
      * Request recognition of people from faces
      * @param {string} id The id of the face recognizer 
      * @param {Face} faces The faces to recognize
      * @param {function} func The function to call once people were recognized
      */
     recognizeFaces: function(id, faces, func) {
        console.log('Recognize faces');
        faces = faces.map(function(x) {
          x.name = "John Doe";
          return x
        });
        func(faces);
     },

     /**
      * Request emotion recognition based persons' face
      * @param {Face} face Array of faces to detect emotions from
      * @param {function} func The function to call once the emotions were inferred
      */
     getEmotionForFaces: function(faces, func) {
      func(
        Array(faces.length).fill("Sad")
      );
     },

     /**
      * Request emotion recognition based on persons' pose
      * @param {Pose} pose The poses to detect emotion from
      * @param {function} func The function to call once the emotions were inferred
      */
     getEmotionForPoses: function(pose, func) {
       // TODO: NOT IMPLEMENTED
     },

     /**
      * Augment (with overlays) the face of a person in view of the user. One or multiple properties can be set.
      * @param {string} id The id of the face augmentation
      * @param {Face} face The face to augment
      * @param {Color} lipColor The color of lipstick to apply
      * @param {Color} eyeShadow The color of eye shadow to apply
      * @param {number} blushing The amount of blushing to apply (0-1)
      */
     augmentFace: function(id, face, lipColor, eyeShadow, blushing) {
       console.log("Augment face:", id, face, lipColor, eyeShadow, blushing);
     },

      /**
      * Load an image into the runtime for later use
      * @param {string} id Identifier used to later refer to this image
      * @param {string} data The image data as either a link, a data URL, or an ArrayBuffer
      */
     loadImage: function(id, data) {
       console.log(`Image loaded (${id}) from ${data}`);
     },

     /**
      * Load a sound into the runtime for later playback
      * @param {string} id Identifier used to later refer to this sound
      * @param {string} data The audio data as either a link, a data URL, or an ArrayBuffer
      */
     loadSound: function(id, data) {
       console.log(`Sound loaded (${id}) from ${data}`);
     },

     /**
      * Play a sound on the AR device's speakers
      * @param {string} id Identifier of the sound to play
      * @param {Position} position Spatial location of the sound (optional)
      */
     playSound: function(id, position, color) {
       if(position === undefined) {
         console.log('Playing sound:', id, 'at', position); 
       } else {
         console.log('Playing sound:', id, 'at', position, 'with color', color);
       }
     },

     /**
      * Use text-to-speech to speak a text
      * @param {string} text The text to speak
      * @param {Position} position Spatial location of the sound (optional)
      */
     speakText: function(text, position) {
       if(position === undefined) {
         console.log('Text-to-speech:', text); 
       } else {
         console.log('Text-to-speech at', position, ':', text);
       }
     },

     /**
      * Write to the debug log
      * @param {string} text The text to write to debug out
      */
    debugOut: function(text) {
      console.log("debugOut:", text);
    },

    /**
     * Send a message back to the editor
     * @param {string} channel Which channel to post this to
     * @param {string} message What message to send
     */
    sendToEditor: function(channel, message) {
      console.log("sendToEditor:", channel, message);
    },

    /**
     * The id of the currently running project
     * @type {number}
     */
    projectID: 0,

    /**
     * The current frame number
     * @type {number}
     */
    frame: 0,

    /**
     * The current local time
     * @type {Date}
     */
    localTime: Date.now(),

    /**
     * The number of milliseconds since app start
     * @type {number}
     */
    appTime: 0.0,

    /**
     * The number of milliseconds since the last frame
     * @type {number}
     */
    frameTime: 0.0
};