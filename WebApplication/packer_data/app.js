{% include 'types.js' %}
{% include 'util.js' %}

function init() {
  {% include 'base.js' %}
  {%- if load_mockup %}
    {% include 'artk.js' %}
  {%- endif %}
  {{node_definitions}}

  app = {
    nodes: {
      {%- for node in scene['nodes'] %}
        {{node.id}}: new {{node.type}}Node("{{node.id}}"{% if node.params %}, {{node.params}} {% endif %}),
      {%- endfor %}
    },
    frame: 0,
    order: {{order|tojson}}
  }

  function connect(output, input) {
    input.connection = output;
    output.connections.push(input);
  }
  
  {%- for connection in scene['connections'] %}
  connect(app.nodes['{{connection.from_id}}'].{{connection.from_field}}, app.nodes['{{connection.to_id}}'].{{connection.to_field}}); 
  {%- endfor %}

  app.update = function(debug) {
    if(debug === true) {
      console.log("Frame " + this.frame);
    }

    {%- if load_mockup %}
    artk.frame = this.frame;
    artk.localTime = Date.now();
    artk.frameTime = 40.0; // simulate 25Hz
    artk.appTime = artk.appTime + artk.frameTime;
    {%- endif %}
  
    var ctx = {frame: this.frame};
    for(var i in this.order) {
      var nid = this.order[i];
      var node = this.nodes[nid];
      node.update(ctx);
    }
    
    this.frame++;
  }
  
  return app;
}

var app = init();