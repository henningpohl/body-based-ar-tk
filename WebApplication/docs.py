from flask import Blueprint, render_template, request, url_for, redirect, jsonify
import itertools
import re

bp = Blueprint('docs', __name__)

# https://docs.python.org/3/library/itertools.html
def grouper(iterable, n, fillvalue=None):
    "Collect data into fixed-length chunks or blocks"
    # grouper('ABCDEFG', 3, 'x') --> ABC DEF Gxx"
    args = [iter(iterable)] * n
    return itertools.zip_longest(*args, fillvalue=fillvalue)

comment_re = re.compile(r"\/\*\*(.+?)\*\/", re.DOTALL | re.MULTILINE)
param_re = re.compile(r"@param (?:{(\w+)} )?(\w+) (.+)")
type_re = re.compile(r"@type {(\w+)}")

def parse_comment(comment):
	result = {}
	params = []

	for line in comment.split('\n'):
		line = line.strip(' *')
		if len(line) == 0:
			continue
		if line.startswith('@param'):
			m = param_re.match(line)
			params.append({
				'type': m.group(1) if m.group(1) else 'Object',
				'name': m.group(2),
				'description': m.group(3)
			})
			result['type'] = 'function'
		elif line.startswith('@type'):
			m = type_re.match(line)
			result['vartype'] = m.group(1)
			result['type'] = 'variable'
		else:
			result['description'] = line

	if len(params) > 0:
		result['parameters'] = params
	return result

def parse_definition(definition):
	definition = definition.strip().split('\n')[0].strip(' {')
	name, definition = definition.split(': ')
	return name, definition.strip(' ,')

def parse_artk(script):
	splits = comment_re.split(script)

	results = []
	for comment, definition in grouper(splits[1:], 2):
		comment = parse_comment(comment)
		name, definition = parse_definition(definition)
		
		comment['name'] = name
		comment['definition'] = definition
		results.append(comment)
	return results

@bp.route('/')
def home():
	return "TODO"

@bp.route('/artk')
def artk():
	with open('packer_data/artk.js', 'r') as f:
		docs = parse_artk(f.read())
		return render_template('docs.html', docs=docs)