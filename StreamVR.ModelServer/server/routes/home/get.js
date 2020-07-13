const { getAll } = require('../../logic/modelsList');

module.exports = function(req, res) {
  const allModels = getAll();

  res.status(200).render('index', {
    Models: allModels.slice(0, 4)
  });
}