class ImageNode extends Node { 
	constructor(params) { super("image", params || { }) } 
  init() {
    super.init();

    var el = $('#'+this.id+' .drop-target')
    var contents = el.find('img').attr('src');
    if('image_file' in this.values) {
      //contents = this.values.sound_file
      //el.find('.filename').html(`${this.values.sound_file.id}.wav`);
    }
  }

  addChangeEvents() {
    $('#'+this.id+' .drop-target').on('dragenter', function(e) {
      e.stopPropagation();
      e.preventDefault();
    });
    $('#'+this.id+' .drop-target').on('dragleave', function(e) {
      $(this).removeClass('drop-highlight');
    });
    $('#'+this.id+' .drop-target').on('dragover', function(e) {
      e.stopPropagation();
      e.preventDefault();
      $(this).addClass('drop-highlight');
    });
    $('#'+this.id+' .drop-target').on('drop', (e) => {
      e.preventDefault();
      var el = e.currentTarget;
      var files = e.originalEvent.dataTransfer.files;
      this.addFile(el, files);
    });
  }
  
  addFile(el, files, save=true) {
    $(el).removeClass('drop-highlight');
    if(files.length != 1) {
      $(el).indicateError("One file only");
      return;
    }
    if(!files[0].type.startsWith("image/")) {
      $(el).indicateError("Image files only");
      return;
    }
    if(!(files[0].type.endsWith("/jpeg") || files[0].type.endsWith("/png"))) {
      $(el).indicateError("JPEG and PNG files only");
      return;
    }
    
    const reader = new FileReader();
    reader.onloadend = (event) => { 
      var contents = event.target.result;
      var error = event.target.error;
      if(error != null) {
        $(el).indicateError("Couldn't read file:" + error.code);
      } else {

        if(save) {
          if('image_file' in this.values) {
            $.ajax({
              type: "DELETE",
              url: '/edit/delete/'+pid+'/images/'+this.values.image_file.id
            })
          }
          var id = files[0].name;
          var params = {id: id, src: '/static/uploads/'+pid+'/images/'+id }
  
          $.ajax({
            type: "POST",
            url: '/edit/save/'+pid+'/images/'+params.id, 
            data: JSON.stringify({ resource: reader.result }),
            contentType: "application/json",
            dataType: "json",
          });

          if(!this.values.hasOwnProperty($(el).data('eid'))) {
            this.values[$(el).data('eid')] = {};
          }
          this.values[$(el).data('eid')] = params;
          this.elem.find('img').attr('src', params.src);
          console.log(this.elem.find('#img-'+$(el).data('eid')+'.image-wrapper'), '#'+$(el).data('eid')+'.image-wrapper')
          this.elem.find('#img-'+$(el).data('eid')+'.image-wrapper').show(0);
        }

        saveScene();
      }
    };
    reader.readAsDataURL(files[0]);
  }
}