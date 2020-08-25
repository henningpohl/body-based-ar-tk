import os
import json
import functools

def __on_files(name, op, fullname=True):
	base_dir = os.path.dirname(__file__)
	for root, _, files in os.walk(base_dir):
		if not name in files:
			continue

		if not fullname:
			# throw the root path away for relative path for web-access
			ssplit = root.split(os.sep)
			indexOfRoot = ssplit.index('nodes')
			root = os.sep.join(ssplit[indexOfRoot+1:])

		fp = os.path.join(root, name)
		yield op(fp)

def __get_file_contents(name):
	def read(fp):
		with open(fp, 'r') as f:
			return f.read()
	return __on_files(name, read)

def __get_file_json(name):
	def read(fp):
		with open(fp, 'r') as f:
			return json.load(f)
	return __on_files(name, read)

def __get_file(category, name, filename):
	category = category.lower()
	base_dir = os.path.dirname(__file__)
	fp = os.path.join(base_dir, category)
	fp = os.path.join(fp, name)
	fp = os.path.join(fp, filename)
	if(not os.path.exists(fp)): return ''
	with open(fp, 'r') as f:
		return f.read()

def __set_file(string, category, name, filename):
	category = category.lower()
	base_dir = os.path.dirname(__file__)
	fp = os.path.join(base_dir, category)
	fp = os.path.join(fp, name)
	if not os.path.exists(fp):
		os.makedirs(fp)
	fp = os.path.join(fp, filename)

	if string.strip() == '':
	  if os.path.exists(fp):
	  	os.remove(fp)
	  return # no need to save empty files

	with open(fp, 'w') as f:
		f.write(string)

def __delete_node(category, name):
	category = category.lower()
	base_dir = os.path.dirname(__file__)
	fp = os.path.join(base_dir, category)
	fp = os.path.join(fp, name)
	for root, dirs, files in os.walk(fp, topdown=False):
		for name in files:
			os.remove(os.path.join(root, name))
		for name in dirs:
			os.rmdir(os.path.join(root, name))
	os.rmdir(fp)

# @functools.lru_cache(maxsize=1)
def get_runtime_code():
	return '\n'.join((x for x in __get_file_contents('runtime.js')))

# @functools.lru_cache(maxsize=1)
def get_node_templates():
	return '\n'.join((x for x in __get_file_contents('template.html')))	

# @functools.lru_cache(maxsize=1)
def get_node_templates_js():
	return '\n'.join((x for x in __get_file_contents('template-node.js')))	

# @functools.lru_cache(maxsize=1)
def get_node_definitions():
	return {x['id']: x for x in __get_file_json('definition.json')}

def get_building_blocks_definitions():
	return [x for x in __get_file_json('building-blocks.json')][0]

def clearCache():
	pass
	# get_runtime_code.cache_clear()
	# get_node_templates.cache_clear()
	# get_node_templates_js.cache_clear()
	# get_node_definitions.cache_clear()

def get_categories():
	base_dir = os.path.dirname(__file__)
	fp = os.path.join(base_dir, 'categories.json')
	with open(fp, 'r') as f:
		return json.load(f)

def set_categories(string):
	base_dir = os.path.dirname(__file__)
	fp = os.path.join(base_dir, 'categories.json')
	with open(fp, 'w') as f:
		f.write(string)