function isUploadValid(files, majorMimes, minorMimes) {
  if(files.length != 1) {
    return {
      status: false,
      reason: 'length'
    };
  }
  
  var f = files[0];
  if(majorMimes.length > 0) {
    console.log(majorMimes);
    if(!majorMimes.some(m => f.type.startsWith(m + '/'))) {
      return {
        status: false,
        reason: 'major'
      };
    }
  }
  
  if(minorMimes.length > 0) {
    if(!minorMimes.some(m => f.type.endsWith('/' + m))) {
      return {
        status: false,
        reason: 'minor'
      };
    }
  }
  
  return {
    status: true,
    reason: ''
  };
}