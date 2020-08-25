# Body LayARs: A Toolkit for Body-Based Augmented Reality

## Overview

Users work with the Body LayARs toolkit via a web application. The application server holds all project files and enables project management, versioning, editing, and sharing. Prototype development primarily happens within a visual programming editor. In addition to the visual flow-based editing, users can also write JavaScript inside scripting nodes.Assets (such as 3d models or audio files) can be uploaded and thenused within a project.

While development happens inside the web application, projects run on AR devices or in a standalone desktop application. In either case, after starting the Body LayARs application on a device, it automatically registers with the webserver. Users can see currently connected devices and their status at the bottom of the project editor. Once ready to test a project, users only need to click on oneof the available devices to run their project on it.

To run a project, the webserver transforms the flow-based representation of the project into a JavaScript application package. Each node is translated into a corresponding JavaScript object and the resulting node graph linearized. Assets are stored on the server, and then referenced from the application package so applications can fetch them later. When starting a project, the host application on the selected device receives this package and runs the contained project. During execution, host devices are asked to call an update function of the packaged application every time a frame is rendered. While an application is executing, users can still make changes in the editor and update the application state. For example, they can change the color of a label that is being shown at runtime.

Because of the differences between AR devices, each device currently requires its own implementation of the host application. For example, there are different SDKs for the Microsoft HoloLens andthe Magic Leap. While environments like Unity or the Unreal Engine provide some abstraction, there remain some fundamental architectural differences (such as the HoloLens only running UWP applications). We envision that the future *OpenXR* standard will soon enable more device-agnostic development. We built host applications for the Microsoft HoloLens as well as for the Windows desktop. The latter allows for convenient prototyping but is limited in its capabilities due to running on a desktop. However, both implement the full Body LayARs JavaScript API andthus can run the same application bundles.

## Design

This repository consists of three parts:

1. **Web application**. The web-based interface can be found in the *"WebApplication"*-folder.
2. **Machine Learning Server**. A small server running some machine learning models for face recognition, emotion and pose detection is implemented in the *"MachineLearningServer"*-folder. The server is seperate from the web application, such that it can be run on a secondary machine for performance.
3. **AR application**. The AR application (here implemented for the Microsoft HoloLens) can be found in the *"ARApplication"*-folder. In addition to the HoloLens implementation, a desktop application for local testing is implemented.

Consult the *"README"* in the corresponding folders for more information. Note also that the Machine learning server needs to be running for certain features to work.

[![Watch the video](https://img.youtube.com/vi/teJdm4_QjUY/maxresdefault.jpg)](https://youtu.be/teJdm4_QjUY)
Watch the video: <https://youtu.be/teJdm4_QjUY>

## Citation

Henning Pohl, Tor-Salve Dalsgaard, Vesa Krasniqi, and Kasper Hornbæk. 2020. Body LayARs: A Toolkit for Body-Based Augmented Reality. In 26th ACM Symposium on Virtual Reality Software and Technology (VRST '20), November 1–4, 2020, Virtual Event, Canada. ACM, New York, NY, USA, 11 pages. <https://doi.org/10.1145/3385956.3418946>

## Licence

MIT Licence

Copyright 2020 Henning Pohl, Tor-Salve Dalsgaard, Vesa Krasniqi

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
