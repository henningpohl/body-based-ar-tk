from flask import Blueprint, render_template, redirect, abort, request, url_for, jsonify
# from db import db.get_db
# from . import db
import db
import json

bp = Blueprint('projects', __name__)

@bp.route('/create', methods=('GET', 'POST'))
def create():
	if request.method == 'GET':
		return render_template('projects/create.html')
	else:
		pname = request.form['name']
		database = db.get_db()
		database.execute('INSERT INTO projects (name) VALUES (?)', (pname,))
		database.commit()
		return redirect(url_for('projects.list'))

@bp.route('/delete/<int:pid>', methods=('GET', 'POST'))
def delete(pid):
	database = db.get_db()

	if request.method == 'GET':
		project = database.execute('SELECT * FROM projects WHERE id = ?', (pid,)).fetchone()
		if project is None:
			abort(404, 'Project does not exist')
		return render_template('projects/delete.html', project=project)
	else:
		database.execute('UPDATE projects SET deleted = 1 WHERE id = ?', (pid,))
		database.commit()
		return redirect(url_for('projects.list'))

@bp.route('/rename/<int:pid>', methods=('POST',))
def rename(pid):
	pname = request.form['name']
	database = db.get_db()
	database.execute('UPDATE projects SET name = (?) WHERE id = (?)', (pname, pid))
	database.commit()
	return "OK"

@bp.route('/')
@bp.route('/list')
def list():
	database = db.get_db()
	projects = database.execute("""
		SELECT * FROM projects
		WHERE deleted = 0
		ORDER BY id ASC
		""").fetchall()

	return render_template('projects/list.html', projects=projects)

def getScene(pid, abort_on_error=True):
	database = db.get_db()
	scene = database.execute("""
		SELECT * FROM content
		WHERE project_id = ?
		""", (pid, )).fetchone()
	if scene == None and abort_on_error: abort(404)
	if(scene != None and scene['data'] != ''): scene = json.loads(scene['data'])
	else: scene = None

	return scene

