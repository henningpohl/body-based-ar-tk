import os
import argparse
import mimetypes
from jinja2 import select_autoescape
from flask import Flask, render_template, send_from_directory
from flask_socketio import SocketIO, emit, send
from flask_jsglue import JSGlue
import db, docs, projects, home, editor, packer, node
import messages

mimetypes.add_type('text/css', '.css')
mimetypes.add_type('text/javascript', '.js')

socketio = SocketIO()

def create_app(test_config=None, debug=False):
    # create and configure the app
    app = Flask(__name__)
    app.debug = debug
    app.jinja_options = dict(autoescape=select_autoescape(default=True), trim_blocks=True)
    app.config.from_mapping(
        SECRET_KEY='dev',
        DATABASE=os.path.join(app.root_path, 'database/database.sqlite'),
    )
    app.config['SECRET_KEY'] = 'ARTK'
    jsglue = JSGlue(app)

    if test_config is None:
        # load the instance config, if it exists, when not testing
        app.config.from_pyfile('config.py', silent=True)
    else:
        # load the test config if passed in
        app.config.from_mapping(test_config)

    db.init_app(app)
    app.register_blueprint(home.bp)
    app.register_blueprint(packer.bp)
    app.register_blueprint(docs.bp, url_prefix='/docs')
    app.register_blueprint(node.bp, url_prefix='/node')
    app.register_blueprint(projects.bp, url_prefix='/projects')
    app.register_blueprint(editor.bp, url_prefix='/edit')

    socketio.init_app(app)
    messages.init_sockets(socketio)
    editor.init_sockets(socketio)

    return app

app = create_app(debug=True)

if __name__ == '__main__':
    socketio.run(app, use_reloader=True, host='0.0.0.0')