import sys
sys.path.append('..')
import unittest
import json
from packer import transform_values

def is_jsonable(x):
	try:
		json.dumps(x)
		return True
	except:
		return False

class TestPacker(unittest.TestCase):

	def helper_test(self, toTest):
		js_version = transform_values(toTest['type'], toTest['input'], False)
		self.assertEqual(js_version, toTest['expected_js'])
		json_version = transform_values(toTest['type'], toTest['input'], True)
		self.assertTrue(is_jsonable(json_version))
		self.assertEqual(json_version, toTest['expected_json'])
	
	def test_string(self):
		toTest = {
			'type': 'string',
			'input': {'string': ['Test']},
			'expected_js': '"Test"',
			'expected_json': 'Test'
		}
		self.helper_test(toTest)

	def test_string_array(self):
		toTest = {
			'type': 'string',
			'input': {'string': ['Test', 'Test2']},
			'expected_js': ['Test', 'Test2'],
			'expected_json': ['Test', 'Test2']
		}
		self.helper_test(toTest)

	def test_number(self):
		toTest = {
			'type': 'number',
			'input': {'number': ['1']},
			'expected_js': 1,
			'expected_json': 1
		}
		self.helper_test(toTest)

	def test_number_array(self):
		toTest = {
			'type': 'number',
			'input': {'number': ['1', '2']},
			'expected_js': [1,2],
			'expected_json': [1,2]
		}
		self.helper_test(toTest)

	def test_position(self):
		toTest = {
			'type': 'position',
			'input': {'position': [['1', '2', '2']]},
			'expected_js': [{'x':1, 'y':2, 'z':2}],
			'expected_json': [{'x':1, 'y':2, 'z':2}]
		}
		self.helper_test(toTest)

	def test_color(self):
		toTest = {
			'type': 'color',
			'input': {'color': ['#fff11122', '#bbb11122']},
			'expected_js': ['#fff11122','#bbb11122'],
			'expected_json': ['#fff11122','#bbb11122']
		}
		self.helper_test(toTest)

	def test_random_string(self):
		toTest = {
			'type': 'random',
			'input': {'type': 'string'},
			'expected_js': '"string"',
			'expected_json': 'string'
		}
		self.helper_test(toTest)

	def test_random_number(self):
		toTest = {
			'type': 'random',
			'input': {'type': 'number', 'number': '24-50'},
			'expected_js': '"number", 24, 50',
			'expected_json': ['number', '24', '50']
		}
		self.helper_test(toTest)

	def test_multiArrayOp(self):
		toTest = {
			'type': 'multiArrayOp',
			'input': {'op': 'map', 'function': 'x = 1;'},
			'expected_js': '"map", function () {\n x = 1; \n}\n',
			'expected_json': ["map", 'function () {\n x = 1; \n}\n']
		}
		self.helper_test(toTest)

	def test_script(self):
		toTest = {
			'type': 'script',
			'input': {'loop': 'y = 2;', 'setup': 'x = 1;', 'input':['a', 'b', 'c']},
			'expected_js': 'function () {\n x = 1; \n}\n, function () {\n y = 2; \n}\n, [\'a\', \'b\', \'c\']',
			'expected_json': ['function () {\n x = 1; \n}\n', 'function () {\n y = 2; \n}\n', ['a', 'b', 'c']]
		}
		self.helper_test(toTest)
	
	def test_poseDetection(self):
		toTest = {
			'type': 'poseDetection',
			'input': {'pose': 'sitting'},
			'expected_js': '"sitting"',
			'expected_json': ['sitting']
		}
		self.helper_test(toTest)
	
	def test_singleArrayOp(self):
		toTest = {
			'type': 'singleArrayOp',
			'input': {'op': 'count'},
			'expected_js': '"count"',
			'expected_json': ['count']
		}
		self.helper_test(toTest)
	
	def test_offsetPosition(self):
		toTest = {
			'type': 'offsetPosition',
			'input': {'offset': 'above'},
			'expected_js': '"above"',
			'expected_json': ['above']
		}
		self.helper_test(toTest)
	
	def test_emotionOp_confidence(self):
		toTest = {
			'type': 'emotionOp',
			'input': {'op': 'emotion_confidence', 'emotion_confidence': 'anger'},
			'expected_js': '"emotion_confidence", "anger"',
			'expected_json': ['emotion_confidence', 'anger']
		}
		self.helper_test(toTest)

	def test_emotionOp_most_confident(self):
		toTest = {
			'type': 'emotionOp',
			'input': {'op': 'most_confident_emotion'},
			'expected_js': '"most_confident_emotion"',
			'expected_json': ['most_confident_emotion']
		}
		self.helper_test(toTest)

	def test_positionExtraction(self):
		toTest = {
			'type': 'offsetPosition',
			'input': {'offset': 'above'},
			'expected_js': '"above"',
			'expected_json': ['above']
		}
		self.helper_test(toTest)
			
	def test_compare(self):
		toTest = {
			'type': 'compare',
			'input': {'op': 'eq'},
			'expected_js': '"eq"',
			'expected_json': ['eq']
		}
		self.helper_test(toTest)
	
	def test_faceRecognizer(self):
		known_faces = {
			1: { 'id': 1, 'caption': 'a', 'src': 'static/images/1.png' },
			2: { 'id': 2, 'caption': 'b', 'src': 'static/images/2.png' },
		}
		toTest = {
			'type': 'faceRecognizer',
			'input': { 'known_faces': known_faces },
			'expected_js': known_faces,
			'expected_json': known_faces
		}
		self.helper_test(toTest)

	def test_playSound(self):
		toTest = {
			'type': 'playSound',
			'input': {'sound_file': { 'id': 1, 'src': 'static/sounds/1.wav' }},
			'expected_js': '"1", "static/sounds/1.wav"',
			'expected_json': ['1', 'static/sounds/1.wav']
		}
		self.helper_test(toTest)
	
	def test_model(self):
		toTest = {
			'type': 'model',
			'input': {'model': { 'id': 1, 'src': 'static/models/1.mdl' }},
			'expected_js': '"1", "static/models/1.mdl"',
			'expected_json': ['1', 'static/models/1.mdl']
		}
		self.helper_test(toTest)
	
	def test_merge(self):
		toTest = {
			'type': 'merge',
			'input': {'input': ['', '', '']},
			'expected_js': 3,
			'expected_json': 3
		}
		self.helper_test(toTest)

	def test_printf(self):
		toTest = {
			'type': 'printf',
			'input': {'format': ['your number is %d and this your name is %s'], 'input': ['', '']},
			'expected_js': '"your number is %d and this your name is %s", 2',
			'expected_json': ['your number is %d and this your name is %s', '2']
		}
		self.helper_test(toTest)

	def test_switch(self):
		functions = ['this.x == 1', '1 < 2', 'default']
		expected = [ 'function () {\n return %s; \n}\n' % f for f in functions ]
		toTest = {
			'type': 'switch',
			'input': {'case': functions},
			'expected_js': '[' + ','.join(expected) + ']',
			'expected_json': expected
		}
		self.helper_test(toTest)

if __name__ == '__main__':
	unittest.main()

