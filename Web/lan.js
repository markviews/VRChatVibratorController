/*
This Lovense's LAN API Script with pcToys removed to prevent unnecessary network requests and console spam
also I added a call to searchForToys() on line 29
https://api.lovense.com/api/lan/v1/lan.js
*/

;(function() {
  window.lovense = window.Lovense = window.lovense || window.Lovense || {}
  var lovense = window.lovense
  var mobileToys = {}
  var mobileData = null

  setInterval(function() {
    if (mobileData && mobileData.domain && mobileData.httpsPort) {
      ajax({
        url: 'http://' + mobileData.domain + ':' +mobileData.httpsPort + '/GetToys',
        success: function(response) {
          var data = response
          if (typeof response === 'string')
            data = JSON.parse(response)

          var toysChanged = false
          if (JSON.stringify(mobileToys) != JSON.stringify(data.data))
            toysChanged = true

          mobileToys = data.data

          if (toysChanged)
            searchForToys()

        },
      })
    }
  }, 3 * 1000)

  var formatParams = function(data) {
    var arr = []
    for (var name in data) {
      arr.push(encodeURIComponent(name) + '=' + encodeURIComponent(data[name]))
    }
    //arr.push(('vrandom=' + Math.random()).replace('.', ''))
    return arr.join('&')
  }

  var ajax = function(options) {
    options = options || {}
    options.type = (options.type || 'GET').toUpperCase()
    options.dataType = options.dataType || 'json'
    options.timeout = options.timeout || 10000 //超时处理，默认10s
    params = formatParams(options.data)

    var xhr
    var xmlHttp_timeout = null
    if (window.XMLHttpRequest) {
      xhr = new XMLHttpRequest()
    } else {
      xhr = ActiveXObject('Microsoft.XMLHTTP')
    }

    xhr.onreadystatechange = function() {
      if (xhr.readyState == 4) {
        clearTimeout(xmlHttp_timeout)
        var status = xhr.status
        if (status >= 200 && status < 300) {
          options.success && options.success(xhr.responseText, xhr.responseXML)
        } else {
          options.error && options.error(status)
        }
      }
    }

    if (options.type == 'GET') {
      xhr.open('GET', options.url + '?' + params, true)
      xhr.send(null)
    } else if (options.type == 'POST') {
      xhr.open('POST', options.url, true)
      xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded')
      xhr.send(params)
    }
    xmlHttp_timeout = setTimeout(function() {
      xhr.abort()
    }, options.timeout)
  }

  lovense.setConnectCallbackData = function(mobileCallbackData) {
    mobileData = mobileCallbackData
    if (mobileCallbackData && mobileCallbackData.toys) {
      mobileToys = mobileCallbackData.toys
    }
  }

  lovense.getToys = function() {
    var toys = []
    for (var key in mobileToys) {
      toys.push(mobileToys[key])
    }
    return toys
  }

  lovense.getOnlineToys = function() {
    var toys = []
    for (var key in mobileToys) {
      var toy = mobileToys[key]
      if (toy.status) {
        toys.push(toy)
      }
    }
    return toys
  }

  lovense.isToyOnline = function() {
    return lovense.getOnlineToys().length > 0
  }

  lovense.sendCommand = function(command, data) {
    if (lovense.isToyOnline())
      ajax({ url: 'http://' + mobileData.domain + ':' + mobileData.httpsPort + '/' + command, data: data, })
  }

})()
