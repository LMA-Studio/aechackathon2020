const fs = require('fs')
const path = require('path')

const modelsDir = path.join(__dirname, '../../materials');

let modelsList = []

function load() {
  if (!fs.existsSync(modelsDir)) {
    fs.mkdirSync(modelsDir);
  }

  const dir = fs.readdirSync(modelsDir);

  Promise.all(
    dir.map(async (m) => {

      const details = await new Promise((res, rej) => {
        fs.readFile(path.join(modelsDir, m, 'details.json'), (err, data) => {
          if (err) return rej(err)
          return res(JSON.parse(data.toString('utf8')))
        })
      })

      return Object.assign(details, {
        filename: m
      })

    })
  ).then(models => modelsList = models)
}

function add(name) {

}

function get(name) {
  return modelsList.find(m => m.name === name)[0];
}

function getAll() {
  return modelsList
}


module.exports = {
  load,
  add,
  get,
  getAll
}
