const fs = require('fs')
const path = require('path')

module.exports = function(req, res) {
  const modelname = req.params.name;
  const currentLocation = path.join(__dirname, '../../../..', req.file.path);
  const destination = path.join(__dirname, '../../../../materials', modelname);

  console.log(modelname);
  
  if (!fs.existsSync(destination)) {
    fs.mkdirSync(destination);
  }
  if (!fs.existsSync(destination + '/preview')) {
    fs.mkdirSync(destination + '/preview');
  }

  fs.renameSync(currentLocation, path.join(destination, 'material.bin'));
  fs.copyFileSync(path.join(destination, 'material.bin'), path.join(destination, 'preview', req.body.fileName));
  fs.writeFileSync(path.join(destination, 'details.json'), Buffer.from(
    JSON.stringify({
      materialName: req.body.materialName,
      fileName: req.body.fileName,
      date: new Date().toISOString()
    })
  ))
  
  res.status(200).send('OK');
}