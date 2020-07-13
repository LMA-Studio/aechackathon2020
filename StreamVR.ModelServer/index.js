const path = require('path');
const express = require('express');
const compression = require('compression');
const bodyParser = require('body-parser');
const multer  = require('multer');
require('dotenv').config();
require('./server/logic/modelsList').load();

const upload = multer({ dest: 'temp/' });
const app = express();

let {
  BASE_PATH,
  APP_HOST,
  APP_PORT,
} = process.env;

if (!BASE_PATH) {
  BASE_PATH = '';
}

// Configure express app
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, '/server/views'));
app.use(compression({
  filter: function (req, res) {
    return true;
  }
}));

const publicDir = path.join(__dirname, 'server/public')
require('./server/logic/cache').InitCache(publicDir);
app.use('/public', express.static(publicDir, {
  maxAge: 0 // 86400000
}));
app.use(bodyParser.urlencoded());

app.get(BASE_PATH, require('./server/routes/home/get'));
app.post(BASE_PATH, require('./server/routes/home/post'));

app.get(BASE_PATH + '/api/model/:name', require('./server/routes/api/model/get'));
app.post(BASE_PATH + '/api/model/:name', upload.single('file'), require('./server/routes/api/model/post'));

// Fallback to 404
app.get(BASE_PATH + '/404', require('./server/routes/404/get'));
app.get('*', (req, res) => res.redirect(BASE_PATH + '/404'));

app.listen(
  APP_PORT,
  APP_HOST,
  () => console.log(`Http server listening on ${APP_HOST}:${APP_PORT}`)
);  