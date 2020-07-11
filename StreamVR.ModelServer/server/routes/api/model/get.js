const fs = require('fs');
const path = require('path');

module.exports = function(req, res) {
  const filename = req.params.name;

  const returnType = !!req.query.details ? 'details.json' : 'model.obj';
  
  res.sendFile(
    path.join(__dirname, '../../../../models', filename, returnType),
    (err) => {
      if (err) {
        console.error(err);
      }
    }
  );
}