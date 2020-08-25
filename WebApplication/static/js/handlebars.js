// https://stackoverflow.com/questions/34252817/handlebarsjs-check-if-a-string-is-equal-to-a-value
Handlebars.registerHelper('ifEquals', function(arg1, arg2, options) {
  return (arg1 == arg2) ? options.fn(this) : options.inverse(this);
});

Handlebars.registerHelper('dynamic', function(slug, context, opts) {
  var template = Handlebars.partials[slug],
    fnTemplate = null;
  if(template === null) {
    console.log('template not found: ' + slug);
  } else if (typeof template === 'function') {
    fnTemplate = template;
  } else {
    // Compile the partial
    fnTemplate = Handlebars.compile(template);
    // Register the compiled partial
    Handlebars.registerPartial(slug, fnTemplate);  
  }
  return new Handlebars.SafeString(fnTemplate(context));
});

Handlebars.registerHelper('strContains', function(arg1, arg2, options) {
  if(arg1 === undefined || arg2 === undefined) { return options.inverse(this); }
  return (arg1.includes(arg2)) ? options.fn(this) : options.inverse(this);
});

Handlebars.registerHelper('any', function(arg1, arg2, options) {
  return (arg1.includes(arg2)) ? options.fn(this) : options.inverse(this);
});

Handlebars.registerHelper('ifCond', function (v1, operator, v2, options) {
switch (operator) {
  case '==':
      return (v1 == v2) ? options.fn(this) : options.inverse(this);
  case '===':
      return (v1 === v2) ? options.fn(this) : options.inverse(this);
  case '!=':
      return (v1 != v2) ? options.fn(this) : options.inverse(this);
  case '!==':
      return (v1 !== v2) ? options.fn(this) : options.inverse(this);
  case '<':
      return (v1 < v2) ? options.fn(this) : options.inverse(this);
  case '<=':
      return (v1 <= v2) ? options.fn(this) : options.inverse(this);
  case '>':
      return (v1 > v2) ? options.fn(this) : options.inverse(this);
  case '>=':
      return (v1 >= v2) ? options.fn(this) : options.inverse(this);
  case '&&':
      return (v1 && v2) ? options.fn(this) : options.inverse(this);
  case '||':
      return (v1 || v2) ? options.fn(this) : options.inverse(this);
  default:
      return options.inverse(this);
}
});

// https://stackoverflow.com/questions/24736938/is-it-possible-to-assign-a-parameter-value-within-handlebars-templates-without-u
Handlebars.registerHelper('assign', function (varName, varValue, options) {
  if (!options.data.root) {
      options.data.root = {};
  }
  options.data.root[varName] = varValue;
});

Handlebars.registerHelper('json', function(context) {
  return JSON.stringify(context);
});

Handlebars.registerHelper('randStr', function() {
  return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
});