# Body LayARs toolkit, web application

## Installation

The visual programming interface is implemented in Python, using the framework Flask. Read more about Flask at <https://flask.palletsprojects.com/en/1.1.x/>. To install Flask and other requirements run:

```shell
pip install -r requirements.txt
```

Toolkit projects are saved in an SQLite database. We have implemented a utility command `init-db`, that initializes the database. To run this utility, execute:

```shell
python -m flask init-db
```

## Run the application

Run the web application:

```shell
python ./app.py
```

The visual programming server serves a website at <http://127.0.0.1:5000/> as per default.

## Citation

Henning Pohl, Tor-Salve Dalsgaard, Vesa Krasniqi, and Kasper Hornbæk. 2020. Body LayARs: A Toolkit for Body-Based Augmented Reality. In 26th ACM Symposium on Virtual Reality Software and Technology (VRST '20), November 1–4, 2020, Virtual Event, Canada. ACM, New York, NY, USA, 11 pages. <https://doi.org/10.1145/3385956.3418946>

## Licence

MIT Licence

Copyright 2020 Henning Pohl, Tor-Salve Dalsgaard, Vesa Krasniqi

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
