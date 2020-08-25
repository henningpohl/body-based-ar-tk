class EmotionOpNode extends ArrayOpNode {
  constructor(params) { super("emotionOp", params || {}) } 

  addChangeEvents() {
    $('#'+this.id+' select[data-eid*="op"]').each((i, e) => {
      $(e).unbind('change');
      $(e).change(() => {

        this.elem.find('.list-group-item:nth-child(n+3):not(:last-child)').remove();

        var endpoint = {}
        this.values[$(e).data('eid')] = $(e).val();
        if("emotion_confidence" == $(e).val()) {
          endpoint = {
            "parentId": this.id,
            "id": 'emotion_confidence',
            "name": 'emotion confidence',
            "scope": "", "type": "", "direction": "",
            "field": {
              "type": "select",
              "values": this.values,
              "options": { 
                "Angry": "Angry", "Disgust": "Disgust",
                "Fear": "Fear", "Happy": "Happy", "Neutral": "Neutral",
                "Sad": "Sad", "Surprise": "Surprise"
              }
            }
          }
          this.elem.find('.nout').each((i, e) => {
            var ttype = $(e).data('type');
            $(e).data('orgtype', ttype);
            var stype = 'number';
            changeEndpointType(e, ttype, stype, 'out')
          });
        } else if("most_confident_emotion" == $(e).val()) {


          this.elem.find('.nout').each((i, e) => {
            var ttype = $(e).data('type');
            $(e).data('orgtype', ttype);
            var stype = 'string';
            changeEndpointType(e, ttype, stype, 'out')
          });
        } else {
          this.elem.find('.nout').each((i, e) => {
            var ttype = $(e).data('orgtype');
            var stype = $(e).data('type');
            changeEndpointType(e, stype, ttype, 'out')
          });
        }

        if(endpoint != {} ) {
          var template = Handlebars.compile('{{dynamic \'genericEndpoint\' this}}');
          this.elem.find('.endpoint-field[data-eid="op"]')
            .parent()
            .after(template(endpoint));
          this.addEmotionChangeEvents();
        }

        this.repaintEndpoints();
        onDesync();
        saveScene();
      });
    });
  }
  addEmotionChangeEvents() {
    $('#'+this.id+' select[data-eid*="emotion_confidence"]').each((i, e) => {
      $(e).unbind('change');
      $(e).change(() => {
        this.values[$(e).data('eid')] = $(e).val();
        this.emitDataChange();
        saveScene();
      });
    });
  }
  onConnectionDelete(info) {
    var type = $(info.target).data('type')
    this.renderOperator(type, false)
  }
  onConnectionConnect(info) {
    var type = $(info.source).data('type')
    this.renderOperator(type)
  }
}
