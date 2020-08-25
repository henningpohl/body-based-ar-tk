import socket
import struct
from PIL import Image
from collections import defaultdict
import json
import io

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

def handle_cropped_images(images, json_obj):

    result = defaultdict(list)

    for request in json_obj["RequestPerFace"]:
        request_type = int(request["Type"])
        image_id = int(request["ImageID"])
        image = images[image_id]

        response = dict()
        if request_type == 1 or request_type == 3:
            # Emotion recognition
            response["Emotion"] = "Happy"
            response["FaceBox"] = [10, 10, 10, 10]
        else:
            response["Emotion"] = "None"
            response["FaceBox"] = []

        if request_type == 2 or request_type == 3:
            # Face Recognition
            response["FaceRecognition"] = "John"
        else:
            response["FaceRecognition"] = "FaceUnknown"

        request_label = request["Label"]
        if request_type == 8 and request_label:
            response["FaceSaved"] = 1
        else:
            response["FaceSaved"] = 0

        result["ResponsePerFace"].append(response)

    result["PoseDetection"] = []
    print(result)
    return json.dumps(result)

def handle_single_image(image, json_obj):

    result = defaultdict(list)

    # Pose detection on the picture
    result["PoseDetection"] = []

    for request in json_obj["RequestPerFace"]:
        request_type = int(request["Type"])
        response = dict()

        if request_type == 1 or request_type == 3:
            response["Emotion"] = "happy"
            response["FaceBox"] = [10, 10, 10, 10]
        else:
            response["Emotion"] = "None"
            response["FaceBox"] = []

        if request_type == 2 or request_type == 3:
            response["FaceRecognition"] = "John"
        else:
            response["FaceRecognition"] = "FaceUnknown"

        request_label = request["Label"]
        if request_type == 8 and request_label:
            response["FaceSaved"] = 1
        else:
            response["FaceSaved"] = 0

        result["ResponsePerFace"].append(response)

    return json.dumps(result)

if __name__ == "__main__":

    PORT = 43002

    LOCAL_IP = socket.gethostbyname(socket.getfqdn())
    print("Server running at", LOCAL_IP)
    print("Port", PORT)

    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind((LOCAL_IP, PORT))

    print('Server started. Waiting for incoming connections.')

    sock.listen(1)
    client_conn, _ = sock.accept()
    print('Client connected.')

    with client_conn:
        while True:
            print('Waiting for message.')
            length = client_conn.recv(4)
            json_length = struct.unpack('i', length)
            json_object = json.loads(client_conn.recv(json_length[0]).decode())
            print(json_object)
            cropped = json_object["CroppedImg"]
            received_images = get_images(json_object, client_conn)

            if cropped:
                # Handling cropped images.
                message_to_send = handle_cropped_images(received_images, json_object)
                client_conn.send(message_to_send.encode())
            else:
                # Handling full sized image.
                message_to_send = handle_single_image(received_images[0], json_object)
                client_conn.send(message_to_send.encode())