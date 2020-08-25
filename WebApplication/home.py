from flask import Blueprint, render_template, request, url_for
# from . import db
import db

bp = Blueprint('home', __name__)

@bp.route('/')
def home():
	database = db.get_db()
	projects = database.execute("""
		SELECT * FROM projects
		WHERE deleted = 0
		ORDER BY created DESC
		LIMIT 5
		""").fetchall()
	return render_template('home/home.html', projects=projects)