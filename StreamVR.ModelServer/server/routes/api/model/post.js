const fs = require('fs')
const path = require('path')

module.exports = function(req, res) {
  const modelname = req.params.name;
  const currentLocation = path.join(__dirname, '../../../..', req.file.path);
  const destination = path.join(__dirname, '../../../../models', modelname);

  console.log(modelname, req.query.v);
  
  if (!fs.existsSync(destination)) {
    fs.mkdirSync(destination);
  }

  // Base Model
  if (!req.query.v)
  {
    fs.renameSync(currentLocation, path.join(destination, 'model.obj'));
    fs.writeFileSync(path.join(destination, 'details.json'), Buffer.from(
      JSON.stringify({
        familyName: req.body.familyName,
        symbolName: req.body.symbolName,
        modelName: req.body.modelName,
        tag: req.body.tag,
        description: req.body.description,
        publisher: req.body.publisher,
        date: new Date().toISOString()
      })
    ))
  }
  // Family Instance Variant (parameterized)
  else
  {
    fs.renameSync(currentLocation, path.join(destination, req.query.v + '.obj'));
  }
  
  res.status(200).send('OK');
}