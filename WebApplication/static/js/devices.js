function updateDevices () {
  successClass = "success"; warningClass = "warning"; 
  dangerClass = "danger"; defaultClass = "default";
  for(k in devices.devices) {
    device = devices.devices[k];
    switch(device.status) {
      case "disconnected":
        $class = dangerClass; break;
      case "ready":
        $class = successClass; break;
      case "running":
          $class = warningClass; break;
      default:
        $class = defaultClass; break;
    }
    device.class = $class;
    devices.devices[k] = device;
  }

  var template = Handlebars.compile($("#deviceTemplate").html());
  html = template(devices);
  $("#deviceManager form").html('')
  $("#deviceManager form").append(html);
}

function deviceShowConnectionStatus(did) {
  device = devices.devices[did]
  var getDeviceConnectionStatus = function(d) {
    attributes = {
      "name": "device",
      "status": "status",
      "ws": "websocket", 
    }

    html = '<ul class="list-group">';
    for (var k in attributes) {
      v = attributes[k]
      str = '<li class="list-group-item" style="background-color: white;">';
      str += v + ": " + d[k];
      str += '</li>';
      html += str;
    }
    html += '</ul>';
    return html;
  }

  $.confirm({
    animation: 'none',
    title: 'Connection status',
    content: getDeviceConnectionStatus(device),
    backgroundDismiss: true,
    buttons: { 
      ok: {
        keys: ['enter'],
        action: function () { } 
      }
    },
  });
}

function runOnDevice(did) {
  if(!devices.devices.hasOwnProperty(did) || devices.devices[did].status == 'disconnected') {
    $.alert('Device was disconnected');
  } else if(devices.devices[did].sync) {
    onDesync();
  } else {
    devices.devices[did].sync = true;
    socket.emit('runOnDevice', {did: did, pid: pid});
  }
}

function onDesync() {
  for (var k in devices.devices){
    devices.devices[k].sync = false;
  }
  updateDevices();
}