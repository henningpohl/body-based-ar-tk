class PlaySoundNode extends Node { 
  constructor(params) { super("playSound", params || { }) } 

  init() {
    super.init();

    var el = $('#'+this.id+' .drop-target')
    var contents = el.find('audio').attr('src');
    if('sound_file' in this.values) {
      el.find('.filename').html(`${this.values.sound_file.id}.wav`);
    }
    this.addAudControl(el.get(0), contents);
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
    if(files[0].type != "audio/wav") {
      $(el).indicateError(".wav files only");
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
          if('sound_file' in this.values) {
            $.ajax({
              type: "DELETE",
              url: '/edit/delete/'+pid+'/sounds/'+this.values.sound_file.id
            })
          }

          var id = files[0].name
          var params = {id: id, src: '/static/uploads/'+pid+'/sounds/'+id }
  
          $.ajax({
            type: "POST",
            url: '/edit/save/'+pid+'/sounds/'+params.id, 
            data: JSON.stringify({ resource: reader.result }),
            contentType: "application/json",
            dataType: "json",
          });

          if(!this.values.hasOwnProperty($(el).data('eid'))) {
            this.values[$(el).data('eid')] = {};
          }
          this.values[$(el).data('eid')] = params;
        }

        $(el).find('.filename').html(id);
        
        this.addAudControl(el, contents)
        saveScene();
      }
    };
    reader.readAsDataURL(files[0]);
  }

  addAudControl(el, contents) {
    var aud = $(el).find('audio')[0];
    var audButton = $(el).find('a');
    var audIcon = $(el).find('i');
    
    aud.pause();
    aud.src = contents;
    aud.load();

    aud.onended = function() {
      audIcon.html('play_circle_outline')
    };
    audButton.click(function() {
      if(aud.paused) {
        aud.play();
        audIcon.html('pause_circle_outline')
      } else {
        aud.pause();
        audIcon.html('play_circle_outline')
      }
    });
  }
}