from flask import Blueprint, render_template, request, url_for, redirect, jsonify
from flask_socketio import join_room, leave_room, emit
from datetime import datetime
import json
import db
from projects import getScene
from nodes import clearCache, get_categories, get_node_definitions, get_node_templates, get_node_templates_js, get_building_blocks_definitions
import base64
import os
import mimetypes
from io import BytesIO
import re

bp = Blueprint('editor', __name__)

def dict_factory(cursor, row):
	d = {}
	for idx,col in enumerate(cursor.description):
		d[col[0]] = row[idx]
	return d

def getBlocks():
	database = db.get_db()
	raw_blocks = database.execute("""
		SELECT * FROM blocks
		""").fetchall()
	blocks = {}
	for raw_block in raw_blocks:
		blocks[raw_block["id"]] = {
			"id": raw_block["id"],
			"name": raw_block["name"],
			"data": json.loads(raw_block["data"])
		}
	return blocks

@bp.route('/<int:pid>')
def edit(pid):
	clearCache()
	categories = get_categories()
	nodes = get_node_definitions()
	scene = getScene(pid, abort_on_error=False)
	node_templates = get_node_templates()
	node_templates_js = get_node_templates_js()
	figurecss = True if request.args.get('figure') == None else False

	building_blocks = get_building_blocks_definitions()
	blocks = getBlocks()
	blocks = {**building_blocks, **blocks}
	blocks = json.dumps(blocks)

	flatten = lambda l: [item for sublist in l for item in sublist]
	endpoints = [ n['endpoints'] for n in nodes.values() ]
	scopes = [ e['scope'] for e in flatten(endpoints) ]
	any_scope = list(set(filter(None, flatten([ s.split(" ") for s in scopes ]))))

	return render_template('editor/editor.html', project={'name': 'Project#'+str(pid)}, 
		categories=categories, nodes=nodes, scene=scene, pid=pid, 
		node_templates=node_templates, node_templates_js=node_templates_js, 
		any_scope=any_scope, blocks=blocks, figurecss=figurecss)

@bp.route('/save/block', methods=['POST'])
def save_block():
	request_data = request.get_json(True)
	name = str(request_data['name'])
	block = request_data['data']
	connection = db.get_db()
	database = connection.cursor()
	database.execute('INSERT INTO blocks (name, data) VALUES (?, ?)', (name, json.dumps(block)))
	lastid = database.lastrowid
	connection.commit()
	return jsonify({"id": lastid})

@bp.route('/delete/block/<int:bid>', methods=['DELETE'])
def delete_block(bid):
	database = db.get_db()
	database.execute('DELETE FROM blocks WHERE id = %d' % bid)
	database.commit()
	return ('', 204)

@bp.route('/save/<int:pid>', methods=['POST'])
def save(pid):
	request_data = request.get_json(True)
	scene = json.dumps(request_data["scene"])
	database = db.get_db()

	content = database.execute("""
		SELECT * FROM content
		WHERE project_id = ?
		""", (pid, )).fetchall()
	
	if(len(content) > 0):
		database.execute('UPDATE content SET data = ? where project_id = ?', (str(scene), pid))
	else:
		database.execute('INSERT INTO content (project_id, data) VALUES (?, ?)', (pid, str(scene)))

	database.commit()
	date = datetime.now().strftime("%d/%m/%Y, %H:%M:%S")
	data = { "clientId": request_data['clientId'], "timestamp": date }
	## namespaces need to be provided
	emit('saved', data, room=pid, namespace='/')
	return ('', 204)

@bp.route('/save/<int:pid>/<string:folder>/<string:filename>', methods=['POST'])
def save_resource(pid, folder, filename):
	directory = os.path.join(os.getcwd(), 'static', 'uploads', str(pid), folder)
	if not os.path.exists(directory):
		os.makedirs(directory)
	upload_path = upload_path = '%s/static/uploads/%s/%s/%s' % (os.getcwd(), pid, folder, filename)
	if os.path.exists(upload_path):
		os.remove(upload_path)

	data = request.get_json(True)
	base64_resource = re.sub(r'^data:[\S]+?;base64,', '', data['resource'])
	resource = bytearray(base64.b64decode(base64_resource))

	with open('%s/%s' % (directory, filename), "bx") as f: 
		f.write(resource) 
	return ('', 204)

@bp.route('/delete/<int:pid>/<string:folder>/<string:filename>', methods=['DELETE'])
def delete_resource(pid, folder, filename):
	upload_path = '%s/static/uploads/%s/%s/%s' % (os.getcwd(), pid, folder, filename)
	if os.path.exists(upload_path):
		os.remove(upload_path)
		return ('', 204)
	return ('', 404)

@bp.route('/scene/<int:pid>')
def fetchScene(pid):
	scene = getScene(pid)
	return jsonify(scene)

def init_sockets(socketio):	
	@socketio.on('join')
	def handleJoin(message):
		pid = message['pid']
		join_room(pid)

@bp.route('/test')
def test():
	return render_template('test2.html')