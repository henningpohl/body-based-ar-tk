import socket
from emotion_detector import EmotionPredictor
from PIL import Image
import struct
import numpy as np
import face_recognition
import pose_detection
from collections import defaultdict
import tensorflow as tf
import json
import io

emotion_predictor = EmotionPredictor()
broken_images_indx = 0

class ServerInterface:

    # Initializing the socket.
    def __init__(self, client_address, port):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.bind((client_address, port))

    # Listening for clients and accepting client connections.
    def accept_incoming_clients(self):
        self.sock.listen(1)
        conn, addr = self.sock.accept()

        return tuple((conn, addr))

    def close_socket(self):
        self.sock.close()

# Reads image data from given client connection and returns a PIL image.
def read_image_bytes(client_connection, image_length):
    received_data_length = 0
    received_data = b''
    while received_data_length < image_length:
        bytes_to_read = image_length - received_data_length
        bytes = 4096 if bytes_to_read > 4096 else bytes_to_read
        data = client_connection.recv(bytes)
        received_data += data
        received_data_length += len(data)

    image = Image.open(io.BytesIO(received_data))
    return image

def get_images(json, client_connection):
    images = []
    for image in json["MessageInfo"]:
        if image["Type"] == "Binary":
            image_length = int(image["Length"])
            images.append(read_image_bytes(client_connection, image_length))
        elif image["Type"] == "URL":
            url = image["Url"]

    return images

def get_names_for_faces(known_face_encodings, known_face_names, image):
    image_arr = np.array(image)
    face_locations = face_recognition.face_locations(image_arr)
    face_encodings = face_recognition.face_encodings(image_arr, face_locations)
    face_results = []

    index = 0
    for face_encoding in face_encodings:
        matches = face_recognition.compare_faces(known_face_encodings, face_encoding)
        name = "FaceUnknown"

        face_distances = face_recognition.face_distance(known_face_encodings, face_encoding)
        if face_distances.size == 0:
            face_results.append(tuple((list(face_locations[index]), name)))
            index += 1
        else:
            best_match_index = np.argmin(face_distances)
            if matches[best_match_index]:
                name = known_face_names[best_match_index]

            face_results.append(tuple((list(face_locations[index]), name)))
            index += 1

    return face_results

def handle_cropped_images(images, json_obj, face_encodings, face_labels):

    json_result = defaultdict(list)

    for request in json_obj["RequestPerFace"]:
        request_type = int(request["Type"])
        image_id = int(request["ImageID"])
        image = images[image_id]
        response = dict()

        if request_type == 1 or request_type == 3:
            # Emotion recognition
            results = emotion_predictor.predict_img(image)[0]
            response["Emotion"] = results[1]
            response["FaceBox"] = results[0]
        else:
            response["Emotion"] = "None"
            response["FaceBox"] = []

        if request_type == 2 or request_type == 3:
            # Face Recognition
            encoding = face_recognition.face_encodings(np.array(image))
            if encoding:
                result = face_recognition.compare_faces(face_encodings, encoding[0])
                if True in result:
                    ind = result.index(True)
                    name = face_labels[ind]
                    response["FaceRecognition"] = name
                else:
                    response["FaceRecognition"] = "FaceUnknown"
            else:
                image.save('broken/{}.jpg'.format(str(broken_images_indx)), 'JPEG')
                # global broken_images_indx
                broken_images_indx += 1

        else:
            response["FaceRecognition"] = "FaceUnknown"

        request_label = request["Label"]
        if request_type == 8 and request_label:
            # Saving face
            encoding = face_recognition.face_encodings(np.array(image))
            if encoding:
                face_labels.append(request_label)
                face_encodings.append(encoding[0])
                response["FaceSaved"] = 1
            else:
                image.save('broken/{}.jpg'.format(str(broken_images_indx)), 'JPEG')
                # global broken_images_indx
                broken_images_indx += 1
        else:
            response["FaceSaved"] = 0

        json_result["ResponsePerFace"].append(response)
        print(json_result)
    json_result["PoseDetection"] = []
    return json.dumps(json_result)

def calculate_distance(li1, li2):
    result = [abs(li1[0] - li2[0]), abs(li1[1] - li2[1]), abs(li1[2] - li2[2]), abs(li1[3] - li2[3])]
    return sum(result)

def calculate_index(faceboxes, rect):
    sums = []
    for facebox in faceboxes:
        sums.append(calculate_distance(facebox[0], rect))

    min_sum = min(sums)
    return sums.index(min_sum)

def get_encoding_index(face_locations, rect):
    sums = []
    for location in face_locations:
        sums.append(calculate_distance(location, rect))

    min_sum = min(sums)
    return sums.index(min_sum)

def handle_single_image(image, json_obj, face_encodings, face_labels):

    json_result = defaultdict(list)
    faces_calculated = False
    faceboxes_and_emotions = emotion_predictor.predict_img(image)
    faceboxes_and_names = []

    for request in json_obj["RequestPerFace"]:
        request_type = int(request["Type"])
        response = dict()

        if request_type == 2 or request_type == 3 and not faces_calculated:
            faceboxes_and_names = get_names_for_faces(face_encodings, face_labels, image)
            faces_caculated = True

        if request_type == 1 or request_type == 3:
            face_rect = request["Rect"]
            if len(faceboxes_and_emotions) > 0:
                index = calculate_index(faceboxes_and_emotions, face_rect)
                emotion = faceboxes_and_emotions[index][1]
                response["Emotion"] = emotion    
            else:
                response["Emotion"] = "None"
            response["FaceBox"] = face_rect
        else:
            response["Emotion"] = "None"
            response["FaceBox"] = []

        if request_type == 2 or request_type == 3:
            face_rect = request["Rect"]
            if len(faceboxes_and_emotions) > 0:
                index = calculate_index(faceboxes_and_names, face_rect)
                name = faceboxes_and_names[index][1]
                response["FaceRecognition"] = name
            else:
                response["FaceRecognition"] = "FaceUnknown"
        else:
            response["FaceRecognition"] = "FaceUnknown"

        request_label = request["Label"]
        if request_type == 8 and request_label:
            face_rect = request["Rect"]
            image_arr = np.array(image)
            face_locations = face_recognition.face_locations(image_arr)
            encodings = face_recognition.face_encodings(image_arr, face_locations)
            face_labels.append(request_label)
            index = get_encoding_index(face_locations, face_rect)
            face_encodings.append(encodings[index])
            response["FaceSaved"] = 1
        else:
            response["FaceSaved"] = 0

        json_result["ResponsePerFace"].append(response)

        # Pose detection on the picture
    if json_obj["RequestPose"]:
        json_result["PoseDetection"] = pose_detection.get_pose_coordinates(image)
    else:
        json_result["PoseDetection"] = []

    return json.dumps(json_result)


if __name__ == "__main__":

    # Needed in order for the GPU not to run out of memory.
    config = tf.ConfigProto()
    config.gpu_options.allow_growth = True
    sess = tf.Session(config=config)

    PORT = 43002

    current_face_encodings = []
    current_face_labels = []

    local_ip = socket.gethostbyname(socket.gethostname())
    print("Server running at", local_ip)
    print("Port", PORT)

    server_interface = ServerInterface(local_ip, PORT)
    print('Server started. Waiting for incoming connections.')

    while True:
        client_conn, _ = server_interface.accept_incoming_clients()
        print('HoloLens connected.')

        with client_conn:
            while True:
                try:
                    print('Waiting for message.')
                    length = client_conn.recv(4)
                    json_length = struct.unpack('i', length)
                    json_object = json.loads(client_conn.recv(json_length[0]).decode())
                    print(json_object)
                    cropped = json_object["CroppedImg"]
                    received_images = get_images(json_object, client_conn)

                    if cropped:
                        # Handling cropped images.
                        message_to_send = handle_cropped_images(received_images, json_object, current_face_encodings,
                                                                current_face_labels)
                        print(message_to_send)
                        client_conn.send(message_to_send.encode())
                    else:
                        # Handling full sized image.
                        message_to_send = handle_single_image(received_images[0], json_object, current_face_encodings,
                                                              current_face_labels)
                        print(message_to_send)
                        client_conn.send(message_to_send.encode())

                except Exception as e:
                    print('Error occurred.', e)
                    break