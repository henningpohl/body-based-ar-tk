from flask import Blueprint, request
from flask_socketio import emit, join_room
from datetime import datetime
import db
import json
import uuid
from packer import transform_values

'''
SocketIO message handler
'''

available_devices = {}
active_connections = []
device_room_name = 'devices'

def init_sockets(socketio):

    ## on connect send available devices
    @socketio.on('connect')
    def handleConnect():
        emit('devices', {'devices': available_devices})

    @socketio.on('disconnect')
    def handleDisconnect():
        for d in list(available_devices):
            if available_devices[d]['sid'] == request.sid:
                del available_devices[d]
        emit('devices', { 'devices': available_devices }, broadcast=True)

    ## add a new device
    @socketio.on('addDevice')
    def handleAddDevice(message):
        device = {
            'id': message['did'],
            'name': message['name'],
            'status': message['status'],
            'sid': request.sid
        }
        available_devices[message['did']] = device
        join_room(device_room_name)
        emit('devices', { 'devices': available_devices }, broadcast=True)

    @socketio.on('updateDeviceStatus')
    def handleUpdateDeviceStatus(message):
        did = message['did']
        available_devices[did]['status'] = message['status']
        emit('devices', { 'devices': available_devices }, broadcast=True)

    ## the user clicked run on a device
    @socketio.on('runOnDevice')
    def handleRunOnDevice(message):
        did = message['did']
        pid = message['pid']
        emit('run', {'did': did, 'pid': pid}, room=device_room_name)
        available_devices[did]['status'] = 'running'
        emit('devices', {'devices': available_devices}, broadcast=True)

    ## get pid of latest project
    @socketio.on('latest project')
    def handleLatestProject(message):
        database = db.get_db()
        pid = database.execute("SELECT id FROM projects WHERE deleted = 0 ORDER BY created DESC LIMIT 1").fetchone()
        emit('latest project', {"status": "ok", "pid": pid['id']})
    
    @socketio.on('updateValue')
    def handleUpdateValue(message):
        update = {
            'pid': message['pid'],
            'nid': message['nid'],
            'ntype': message['ntype'],
            'nvalue': transform_values(message['ntype'], message['value'], True)
        }
        emit('updatedValue', update, room=device_room_name)
