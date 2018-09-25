const Fs = require("fs");

var data = null;
var dataPath = null;

exports.init = function (path) {
    dataPath = path;
    if (Fs.existsSync(dataPath)) {
        data = JSON.parse(Fs.readFileSync(dataPath));
    } else {
        data = {};
    }
}

exports.get = function () {
    return data;
}

exports.edit = function (f) {
    const res = f(data);
    Fs.writeFileSync(dataPath, JSON.stringify(data, null, 4));
    return res;
}
