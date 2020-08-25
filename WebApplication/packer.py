from flask import Response, Blueprint, render_template, redirect, abort, request, url_for, jsonify
import db
import json
from nodes import get_runtime_code, clearCache
from projects import getScene
from topological import Graph


bp = Blueprint('packer', __name__, template_folder='packer_data')

def tofloat(x):
    try:
        return float(x)
    except:
        return 0.0

def transform_values(ntype, values, toJson=False):
    if not values: return ''
    pack = lambda s: '"'+str(s)+'"' if not toJson else str(s)
    pack_fn = lambda f: 'function () {\n %s \n}\n' % f
    pack_args = lambda xs: ', '.join(xs) if not toJson else list(xs)

    def arrayable():
        return values[ntype] if len(values[ntype]) > 1 else pack(values[ntype][0])
    def input_values():
        return pack_args(values.values())
    def select_values():
        return pack_args([ pack(v) for v in values.values()])
    def position():
        return [ { 'x':tofloat(p[0]), 'y':tofloat(p[1]), 'z':tofloat(p[2]) } for p in values[ntype] ]
    def number():
        vs = list(map(tofloat, values[ntype]))
        return vs if len(vs) > 1 else vs[0]
    def random():
        if(values['type'] != 'number'): return pack(values['type'])
        if('number' not in values): values['number'] = ''
        minMax = [s.strip() for s in values['number'].split('-')]
        minN, maxN = (minMax[0], minMax[1]) if len(minMax) >= 2 else ("0", "10")
        return pack_args([pack(values['type']), minN, maxN])
    def multiarrayop():
        op = values['op']
        if(op in ['map', 'filter']):
            func = pack_fn(values['function'])
            return pack_args([pack(op), func])
        else:
            return pack(op)
    def script():
        defaults = {'loop':'', 'setup':'', 'input':[]}
        vs = {**defaults, **values}
        loopFunc = pack_fn(vs['loop'])
        setupFunc = pack_fn(vs['setup'])
        inputNames = str(vs['input']) if not toJson else vs['input']
        return pack_args([setupFunc, loopFunc, inputNames])
    def facerecognizer():
        return { iid: values['known_faces'][iid] for iid in values['known_faces'] }
    def playsound():
        return pack_args(map(pack, [values['sound_file']['id'], values['sound_file']['src']]))
    def model():
        return pack_args(map(pack, [values['model']['id'], values['model']['src']]))
    def merge():
        return len(values['input'])
    def printf():
        return pack_args([pack(values['format'][0]), str(len(values['input']))])
    def switch():
        functions = [ pack_fn('return %s;' % x) for x in values['case'] ]
        return '[' + ','.join(functions) + ']' if not toJson else functions
    def image():
        return pack(values['image_file']['src'])

    ntype_switch = {
        'string': arrayable,
        'color': arrayable,
        'position': position,
        'number': number,
        'random': random,
        'multiArrayOp': multiarrayop,
        'script': script,
        'poseDetection': select_values,
        'singleArrayOp': select_values,
        'offsetPosition': select_values,
        'emotionOp': select_values,
        'positionExtraction': select_values,
        'compare': select_values,
        'faceRecognizer': facerecognizer,
        'playSound': playsound,
        'model': model,
        'merge': merge,
        'printf': printf,
        'switch': switch,
        'image': image,
    }
    fn = ntype_switch.get(ntype, input_values)
    return fn()

def transform_scene(scene):
    nodes = []
    for n in scene['nodes']:
        nodes.append({
            'id': n['id'],
            'type': n['type'][:1].upper() + n['type'][1:],
            'params': transform_values(n['type'], n['values'])
        })

    connections = []
    for c in scene['connectors']:
        src, sf = c['source'].split('-')
        dst, df = c['target'].split('-')
        connections.append(dict(from_id=src, from_field=sf, to_id=dst, to_field=df))

    return dict(nodes=nodes, connections=connections)

def filter_empty_nodes(scene):
    ## flatten list function
    flatten = lambda l: [item for sublist in l for item in sublist]
    ## all ids-types that have a connection
    nonEmptyNodes = flatten([list(x.values()) for x in scene['connectors']])
    ## unique ids that have a connection
    nonEmptyNodes = set([s.split("-")[0] for s in nonEmptyNodes])
    ## check that the node id has a connection
    scene['nodes'] = [ n for n in scene['nodes'] if n['id'] in nonEmptyNodes ]
    return scene

def scene_order(scene):
    # list all node ids
    g = Graph(list({n['id'] for n in scene['nodes']}))
    for c in scene['connectors']:
        src, _ = c['source'].split('-')
        dst, _ = c['target'].split('-')
        # add edges to the graph
        g.addEdge(src, dst)
    # find the topological order of the graph
    return g.topologicalSort()

@bp.route('/pack/<int:pid>')
def pack(pid):
    clearCache()
    scene = getScene(pid)
    scene = filter_empty_nodes(scene)
    order = scene_order(scene)
    scene = transform_scene(scene)

    minify = request.args.get('minify') != None
    runtime_only = request.args.get('runtime-only') != None
    mock_debug = request.args.get('mock-debug') != None

    params = { 
        'node_definitions': get_runtime_code(),
        'scene': None if runtime_only else scene,
        'load_mockup': mock_debug,
        'order': order
    }

    if runtime_only:
        app_script = render_template('runtime.js', **params)
    else:
        app_script = render_template('app.js', **params)

    return Response(app_script, content_type='text/javascript; charset=utf-8')

@bp.route('/pack/exec/<int:pid>')
def exec_pack(pid):
    return render_template('exec.html', pid=pid)