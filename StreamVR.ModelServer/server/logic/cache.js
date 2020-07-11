const crypto = require('crypto')
const fs = require('fs')
const cacheIds = {}

let publicDir = '../public'

function InitCache(dir) {
  publicDir = dir
}
function GetCache(url) {
  if (process.env.PRODUCTION !== 'false') {
    url = url.replace(/\.css/, '.min.css')
    url = url.replace(/\/css/, '/css/min')
    url = url.replace(/\.js/, '.min.js')
    url = url.replace(/\/js/, '/js/min')
  }

  if (cacheIds[url]) {
    return cacheIds[url]
  }
  const file = fs.readFileSync(`${publicDir}${url.replace('/public', '')}`)
  const hashFunc = crypto.createHash('md5')
  hashFunc.write(file)
  const hash = hashFunc.digest('hex')
  return url + '?tag=' + hash
}

module.exports = {
  InitCache,
  GetCache
}