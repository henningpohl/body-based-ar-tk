class RandomNode extends Node {
  constructor(params) { super("random", params || {}) }
  init() {
    super.init()
    var id = this.id
    $('#'+this.id+' .nav-tabs a').each(function () {
      var active = $(this).hasClass('active');
      var type = $(this).data('type');
      $('div[class*="endpoint-type-'+type+' endpoint-out-'+id+'"]').each(function () {
        if(active) {
          $(this).show(); 
        } else { 
          $(this).hide(); 
        } 
        
      })
    });
    
    var activeType = $('#'+this.id+' .nav-tabs').data('value');
    if(activeType != '') {
      var selector = 'a[data-type="'+activeType+'"]'
    } else {
      var selector = '.nav-tabs li:first-child a'
    }
    var href = $('#'+this.id+' '+selector+'').addClass('active').attr('href');
    $(href).addClass('active');
    
    super.repaintEndpoints();
    this.setValue('type', 'number')
  }
  addChangeEvents() {
    var id = this.id
    this.elem.find('.nav-link').each((i, e) => {
      $(e).on('shown.bs.tab', event => {
        // delete connections from/to old endpoint
        var type = $(event.relatedTarget).data('type');
        var endpoint = $('#out-'+id+'-'+type+'')
        inst.deleteConnectionsForElement(endpoint);
        this.repaintEndpoints();
        
        // hide all other endpoints
        type = $(event.target).data('type');
        classNodes[id].setValue('type', type)
        $('div[class*="endpoint-out-'+id+'"]').each(function () {
          if($(this).hasClass('endpoint-type-'+type+'')) { 
            $(this).show(); 
          } else { 
            $(this).hide(); 
          } 
        });
      });
      
      saveScene();
      onDesync();
    });
    super.addChangeEvents()
  }
}
