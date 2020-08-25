class ModelNode extends Node { 
  constructor(params) { super("model", params || { }) } 

  init() {
    super.init();
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

    this.elem.find('.help').click((e) => {
      $.confirm({
        animation: 'none',
        title: 'Orientation help',
        backgroundDismiss: true,
        columnClass: 'col-md-6',
        content: `
        <h6><b>Orientation usage<b><h6>
        <div>
          Use the position as a vector to define the x, y, z rotation of the model.
        </div>
        `,
        buttons: {
          ok: () => {}
        }
      });
    })
  }

  addFile(el, files, save=true) {
    $(el).removeClass('drop-highlight');
    if(files.length != 1) {
      $(el).indicateError("One file only");
      return;
    }
    if(!files[0].name.endsWith('.mdl')) {
      $(el).indicateError(".mdl files only");
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
          if('model' in this.values) {
            $.ajax({
              type: "DELETE",
              url: '/edit/delete/'+pid+'/models/'+this.values.model.id
            })
          }

          var id = files[0].name
          var params = {id: id, src: '/static/uploads/'+pid+'/models/'+id }
  
          $.ajax({
            type: "POST",
            url: '/edit/save/'+pid+'/models/'+params.id, 
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
        saveScene();
      }
    };
    reader.readAsDataURL(files[0]);
  }
}