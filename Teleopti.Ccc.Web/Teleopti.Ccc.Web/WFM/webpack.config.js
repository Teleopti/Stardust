var path = require('path');

module.exports = {
  entry: './dist/temp/app/app.js',
  output: {
    filename: 'bundle.js',
    path: path.resolve(__dirname, 'dist')
  }
};