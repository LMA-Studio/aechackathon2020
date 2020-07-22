const fs = require('fs');
const path = require('path');

module.exports = function(req, res) {
  const filename = req.params.name;
  
  const returnType = !!req.query.details ? 'details.json' 
    : !!req.query.v ? `${req.query.v}.obj`
    : 'model.obj';

  let returnPath = path.join(__dirname, '../../../../models', filename, 'model.obj')

  if (req.query.details)
  {
    returnPath = path.join(__dirname, '../../../../models', filename, 'details.json')
  }
  else if (!!req.query.v)
  {
    returnPath = path.join(__dirname, '../../../../models', filename, `${req.query.v}.obj`)
    if (!fs.existsSync(returnPath))
    {
      returnPath = path.join(__dirname, '../../../../models', filename, 'model.obj')
    }
  }
  
  res.sendFile(
    returnPath,
    (err) => {
      if (err) {
        console.error(err);
      }
    }
  );
}
