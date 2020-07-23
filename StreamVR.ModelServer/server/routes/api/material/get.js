const fs = require('fs');
const path = require('path');

module.exports = function(req, res) {
  const filename = req.params.name;
  
  const returnPath = path.join(__dirname, '../../../../materials', filename, 'material.bin')

  if(!fs.existsSync(returnPath))
  {
    res.status(404).send('Not found');
  }
  else
  {
    res.sendFile(
      returnPath,
      (err) => {
        if (err) {
          console.error(err);
        }
      }
    );
  }
  
}
