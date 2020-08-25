class DistanceNode extends Node {
  constructor(id, mode) {
    super(id);
    this.mode = mode;
    this.inputA = new Input();
    this.inputB = new Input();
    this.distance = new Output();
  }

  update(ctx) {
    super.update(ctx);
		if(!this.inputA.isDirty && !this.inputB.isDirty) {
			return;
    }
    
    var a = this.inputA.value
    var b = this.inputB.value
    if(typeof a != typeof b) { return; }
    
    switch (typeof a) {
      case "number":
        this.distance = a - b;
        break;
      case "string":
        if(a.startsWith("#") && b.startsWith("#")) {
          a = Color(a); b = Color(b);
          this.euclidean([a.r, a.b, a.g], [b.r, b.b, b.g]);
        }
        this.distance = this.levenshtein_distance(a, b)
        break;
      default:
        if(Array.isArray(a)) {
          this.distance = this.euclidean(a, b)
        }
        break;
    }
  }

  // https://gist.github.com/andrei-m/982927
  levenshtein_distance (a, b) {
    if(a.length == 0) return b.length; 
    if(b.length == 0) return a.length; 
    var matrix = [];
    // increment along the first column of each row
    var i;
    for(i = 0; i <= b.length; i++){ matrix[i] = [i]; }
    // increment each column in the first row
    var j;
    for(j = 0; j <= a.length; j++){ matrix[0][j] = j; }
    // Fill in the rest of the matrix
    for(i = 1; i <= b.length; i++){
      for(j = 1; j <= a.length; j++){
        if(b.charAt(i-1) == a.charAt(j-1)){
          matrix[i][j] = matrix[i-1][j-1];
        } else {
          matrix[i][j] = 
                      // substitution               // insertion          // deletion
            Math.min(matrix[i-1][j-1] + 1, Math.min(matrix[i][j-1] + 1, matrix[i-1][j] + 1));
        }
      }
    }
    return matrix[b.length][a.length];
  }
  euclidean(a, b) {
    var sum = 0; var n;
    for (n = 0; n < a.length; n++) {
      sum += Math.pow(a[n] - b[n], 2)
    }
    return sum;
  }
}