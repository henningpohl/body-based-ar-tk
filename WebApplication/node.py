from flask import Blueprint, render_template, Response, request, redirect, url_for
from nodes import __on_files, get_categories, get_node_definitions, __get_file, __set_file, set_categories, __delete_node, clearCache
import os
import json

bp = Blueprint('node', __name__, static_folder='nodes')

base_folder = os.path.dirname(os.path.relpath(__file__))
edit_map = {
	'editor': ('Common editor code', os.path.join(base_folder, 'static', 'js', 'node_helpers.js')),
	'artk': ('Runtime interface', os.path.join(base_folder, 'packer_data', 'artk.js')),
	'types': ('Runtime types', os.path.join(base_folder, 'packer_data', 'types.js')),
	'util': ('Runtime utility code', os.path.join(base_folder, 'packer_data', 'util.js')),
}

@bp.route('/build/editor/css')
def css():
	node_definitions=__on_files('style.css', lambda s: s, False)
	app_script = render_template('css/nodes.css', node_definitions=node_definitions)
	return Response(app_script, content_type='text/css; charset=utf-8')

@bp.route('/list')
def node_list():
	shared_edits = [(k, edit_map[k][0]) for k in edit_map]
	return render_template('node/list.html', categories=get_categories(), shared_edits=shared_edits)

@bp.route('/shared/edit/<string:name>')
def edit_shared(name):
	if name not in edit_map:
		abort(404)

	label, filepath = edit_map[name]
	with open(filepath, 'r') as f:
		content = f.read()

	return render_template('node/shared_edit.html', label=label, content=content, name=name)


@bp.route('/shared/save/<string:name>', methods=['POST'])
def save_shared(name):
	if name not in edit_map:
		abort(404)

	_, filepath = edit_map[name]
	with open(filepath, 'w') as f:
		f.write(request.data.decode('utf8'))

	clearCache()
	return 'OK'

@bp.route('/edit/<path:category>/<string:name>')
def edit(category, name):
	definitions = get_node_definitions()
	definition = definitions[name]
	template_node = __get_file(category, name, 'template-node.js')
	runtime = __get_file(category, name, 'runtime.js')
	style = __get_file(category, name, 'style.css')
	template = __get_file(category, name, 'template.html')
	return render_template('node/edit.html', category=category, name=name, 
		categories=get_categories(), definition=definition, runtime=runtime, 
		template_node=template_node, style=style, template=template)

@bp.route('/create')
def create():
	name = request.args.get('name')
	category = request.args.get('category')

	categories = get_categories()
	for cat in categories:
		if(cat['name'] == category):
			cat['items'].append({"name": name})
	set_categories(json.dumps(categories, indent=2))

	definition = '{\n\t"id": "'+name+'", \n\t"name": "'+name+'", \n\t"endpoints": []\n}'
	runtime = 'class '+name.capitalize()+'Node extends Node { \n \n }'
	template_node = 'class '+name.capitalize()+'Node extends Node { \n\t constructor(params) { super("'+name+'", params || { }) } \n }'

	__set_file(definition, category, name, 'definition.json')
	__set_file(template_node, category, name, 'template-node.js')
	__set_file(runtime, category, name, 'runtime.js')
	return redirect(url_for('node.node_list'))

@bp.route('/duplicate/<path:ogcategory>/<string:ogname>/<string:category>/<string:name>')
def duplicate(ogcategory, ogname, category, name):
	categories = get_categories()
	for cat in categories:
		if(cat['name'] == category):
			cat['items'].append({"name": name})
	set_categories(json.dumps(categories, indent=2))

	style = __get_file(ogcategory, ogname, 'style.css')
	runtime = __get_file(ogcategory, ogname, 'runtime.js')
	template = __get_file(ogcategory, ogname, 'template.html')
	definition = __get_file(ogcategory, ogname, 'definition.json')
	template_node = __get_file(ogcategory, ogname, 'template-node.js')
	
	definition = json.loads(definition)
	definition['id'] = name
	definition['name'] = name
	definition = json.dumps(definition, indent=2)

	template_node = template_node.replace(ogname.capitalize()+'Node', name.capitalize()+'Node')
	runtime = runtime.replace(ogname.capitalize()+'Node', name.capitalize()+'Node')

	__set_file(style, category, name, 'style.css')
	__set_file(runtime, category, name, 'runtime.js')
	__set_file(template, category, name, 'template.js')
	__set_file(definition, category, name, 'definition.json')
	__set_file(template_node, category, name, 'template-node.js')
	return redirect(url_for('node.edit', category=category, name=name))

@bp.route('/save', methods=['POST'])
def save():
	request_data = request.get_json(True)
	data = request_data
	category = data['category']
	name = data['name']
	__set_file(data['definition'], category, name, 'definition.json')
	__set_file(data['template_node'], category, name, 'template-node.js')
	__set_file(data['runtime'], category, name, 'runtime.js')
	__set_file(data['style'], category, name, 'style.css')
	__set_file(data['template'], category, name, 'template.html')
	clearCache()
	return 'OK'

@bp.route('/delete/<string:category>/<string:name>') #maybe make this methods=['DELETE']) and use javascript for the request
def delete(category, name):
	__delete_node(category, name)
	categories = get_categories()
	for cat in categories:
		if(cat['name'] == category):
			cat['items'].remove({"name": name})
	set_categories(json.dumps(categories, indent=2))
	return redirect(url_for('node.node_list'))