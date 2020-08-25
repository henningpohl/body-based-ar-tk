from keras.models import model_from_json
import face_recognition
import numpy as np
import cv2
from PIL import Image


EMOTIONS_LIST = ["Angry", "Disgust", "Fear", "Happy", "Sad", "Surprise", "Neutral"]


class EmotionPredictor:
	def __init__(self):
		with open('model/model.json', 'r') as json_file:
			json_data = json_file.read()
			self.loaded_model = model_from_json(json_data)

		self.loaded_model.load_weights('weights/weights.h5')

	def predict_img(self, data):
		# Give a PIL image
		image = np.array(data)
		face_list = face_recognition.face_locations(image)

		face_imgs = []
		labels = []
		for entry in face_list:
			top, right, bottom, left = entry
			#print('top, right, bottom, left: ', entry)
			face = image[top:bottom, left:right]
			face_imgs.append(face)

		index = 0
		for face in face_imgs:
			face = cv2.resize(face, (48, 48))
			face = cv2.cvtColor(face, cv2.COLOR_BGR2GRAY)
			face = np.reshape(face, [1, face.shape[0], face.shape[1], 1])
			predicted_class = np.argmax(self.loaded_model.predict(face))
			if predicted_class:
				predicted_label = EMOTIONS_LIST[predicted_class]
			else:
				predicted_label = "NoEmotion"

			labels.append(tuple((list(face_list[index]), predicted_label)))
			index += 1
		return labels
