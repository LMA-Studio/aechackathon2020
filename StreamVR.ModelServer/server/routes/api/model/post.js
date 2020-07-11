const fs = require('fs')
const path = require('path')

module.exports = function(req, res) {
  const modelname = req.params.name;
  const currentLocation = path.join(__dirname, '../../../..', req.file.path);
  const destination = path.join(__dirname, '../../../../models', modelname);
  
  if (!fs.existsSync(destination)) {
    fs.mkdirSync(destination);
  }
  
  fs.renameSync(currentLocation, path.join(destination, 'model.obj'));
  fs.writeFileSync(path.join(destination, 'details.json'), Buffer.from(
    JSON.stringify({
      name: req.body.name,
      tag: req.body.tag,
      description: req.body.description,
      publisher: req.body.publisher,
      date: new Date().toISOString()
    })
  ))
  
  res.status(200).send('OK');
}