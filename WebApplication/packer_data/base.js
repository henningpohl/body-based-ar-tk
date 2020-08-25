class Input {
  constructor() {
    this._connection = undefined;
    this._isDirty = false;
  }

  notify() {
    this._isDirty = true;
  }

  get value() {
    if(this._connection) {
      this._isDirty = false;
      return this._connection.value;
    } else {
      return undefined; 
    }
  }

  get valueCount() {
    if(this._connection) {
      if(Array.isArray(this._connection.value)) {
        return this._connection.value.length;
      } else {
        return 1;
      }
    } else {
      return 0;
    }
  }

  get(i, wrap=false) {
    if(this._connection) {
      this._isDirty = false;
      if(Array.isArray(this._connection.value)) {
        if(wrap) {
          return this._connection.value[i % this._connection.value.length];
        } else {
          return this._connection.value[i];
        }
      } else {
        return this._connection.value;
      }
    } else {
      return undefined;
    }
  }

  get isDirty() {
    return this._isDirty && typeof this.value !== 'undefined';
  }

  get isConnected() {
    return this._connection !== undefined;
  }

  set connection(value) {
    this._connection = value;
    this._isDirty = true;
  }
}

class Output {
  constructor(value) {
    this.connections = [];
    this._value = value;
  }

  set value(value) {
    this._value = value;
    for(var i = 0; i < this.connections.length; ++i) {
      let c = this.connections[i];
      c.notify();
    }
  }

  get value() {
    return this._value;
  }
}

class Node {
  constructor(id) {
    this.id = id;
  }

  update(ctx) { 
    // implement in derived classes
  }

  setValue(name, value) {
    this[name].value = value;
  }
}