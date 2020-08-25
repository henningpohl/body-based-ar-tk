# Body LayARs toolkit, Machine Learning Server

This server runs some face recognition, pose and emotion detection. The AR application sends images/video to this server for recognition and detection. The face recognition and emotion detection is based on the ``face_recognition`` (<https://github.com/ageitgey/face_recognition>), while pose detection is based on ``posenet`` (<https://github.com/tensorflow/tfjs-models/tree/master/posenet>).

## Installation

This application is implemented in Python and depends a few packages, that are listed in the *requirements.txt*-file. Install them using the command:

```shell
pip install -r requirements.txt
```

## Run the application

Run the machine learning application:

```shell
python ./server_interface.py
```

The server runs at your local ip on port 43002 (the address is printed when run).

## Citation

Henning Pohl, Tor-Salve Dalsgaard, Vesa Krasniqi, and Kasper Hornbæk. 2020. Body LayARs: A Toolkit for Body-Based Augmented Reality. In 26th ACM Symposium on Virtual Reality Software and Technology (VRST '20), November 1–4, 2020, Virtual Event, Canada. ACM, New York, NY, USA, 11 pages. <https://doi.org/10.1145/3385956.3418946>

## Licence

MIT Licence

Copyright 2020 Henning Pohl, Tor-Salve Dalsgaard, Vesa Krasniqi

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
