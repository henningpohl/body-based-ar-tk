function randStr() {
  return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
}

class FaceRecognizerNode extends Node {
  constructor(params) { super("faceRecognizer", params || {}) }
  addChangeEvents(fileEvent = true){
    if(fileEvent) {
      $('#'+this.id+'-collectionModal input[type="file"]').each((i, e) => {
        $(e).change(() => {
          // https://stackoverflow.com/questions/36280818/how-to-convert-file-to-base64-in-javascript/36281449
          var files = $(e).prop('files')
          if(files.length > 0) {
            Array.from(files).forEach(f => {
              var reader = new FileReader();
              reader.readAsDataURL(f);
              reader.onload = () => {
    
              }
              reader.onloadend = () => {
                var caption = $(e).closest('.card').find('input[type="text"]').val();
                var id = f.name
                var params = {id: id, src: '/static/uploads/'+pid+'/faces/'+id, caption: caption }

                $.ajax({
                  type: "POST",
                  url: '/edit/save/'+pid+'/faces/'+id,
                  data: JSON.stringify({ resource: reader.result }),
                  contentType: "application/json",
                  dataType: "json",
                });

                if(!this.values.hasOwnProperty($(e).data('eid'))) {
                  this.values[$(e).data('eid')] = {};
                }
                this.values[$(e).data('eid')][params.id] = params;
      
                var template = Handlebars.compile('{{dynamic "collection-known_face" this}}');
                $(e).closest('.modal-body').find('.face-images').append(template(params))
                $(e).val('');
                
                $(e).closest('.modal-body').find('.delete-known-face').unbind('click');
                $(e).closest('.modal-body').find('.delete-known-face').click(f => {
                  this.onDeleteFace(f, $(e).data('eid'))
                })
                this.addChangeEvents(false);
                saveScene();
              }
              reader.onerror = function (error) {
                console.log('Error: ', error);
              };

            })
          }
        })
      });
    }

    $('#'+this.id+'-collectionModal input[type="text"]').each((i, e) => {
      $(e).unbind('change');
      $(e).change(() => {
        var eid = $(e).closest('.modal').data('eid');
        var id = $(e).closest('.card').attr('id');
        var type = $(e).data('type');
        this.values[eid][id][type] = $(e).val();
        saveScene();
      })
    })

  }
  onDeleteFace(f, eid) {
    var id = $(f.currentTarget).parent().attr('id');
    if(this.values.hasOwnProperty(eid) && this.values[eid].hasOwnProperty(id)) {
      delete this.values[eid][id]
    }
    $(f.currentTarget).parent().remove();
    saveScene();

    $.ajax({
      type: "DELETE",
      url: '/edit/delete/'+pid+'/image/'+id+'', 
    });
  }
  render() {
    super.render();
    var template = Handlebars.compile($("#collectionModalTemplate").html());
    var endpoints = this.endpoints.filter(e => e.hasOwnProperty("field") && e.field.type == 'collection')
    endpoints.forEach(e => {
      e.parentId = this.id;
      $("body").append(template(e));

      $('#'+this.id+'-collectionModal').find('.delete-known-face').click(f => {
        this.onDeleteFace(f, e.id)
      })
    }); 
  }
}