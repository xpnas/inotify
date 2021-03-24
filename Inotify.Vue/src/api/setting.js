import request from '@/utils/request'

export function getSendTemplates(data) {
  return request({
    url: '/setting/getSendTemplates',
    method: 'get'
  })
}

export function getSendAuths(authInfo) {
  return request({
    url: '/setting/getSendAuths',
    method: 'get',
    params: { authInfo }
  })
}

export function getSendKey(authInfo) {
  return request({
    url: '/setting/getSendKey',
    method: 'get',
    params: { authInfo }
  })
}

export function reSendKey(authInfo) {
  return request({
    url: '/setting/reSendKey',
    method: 'get',
    params: { authInfo }
  })
}

export function sendMessage(message) {
  return request({
    url: '/send',
    method: 'get',
    params: message
  })
}

export function deleteAuthInfo(sendAuthId) {
  return request({
    url: '/setting/deleteSendAuth',
    method: 'post',
    params: { sendAuthId: sendAuthId }
  })
}

export function activeAuthInfo(sendAuthId, state) {
  return request({
    url: '/setting/activeSendAuth',
    method: 'post',
    params: { sendAuthId: sendAuthId, state: state }
  })
}

export function addAuthInfo(template) {
  return request({
    url: '/setting/addSendAuth',
    method: 'post',
    data: template
  })
}

export function modifySendAuth(template) {
  return request({
    url: '/setting/modifySendAuth',
    method: 'post',
    data: template
  })
}

export function deepClone(data) {
  let d
  if (typeof data === 'object') {
    if (data == null) {
      d = null
    } else {
      if (data.constructor === Array) {
        d = []
        for (const i in data) {
          d.push(deepClone(data[i]))
        }
      } else {
        d = {}
        for (const i in data) {
          d[i] = deepClone(data[i])
        }
      }
    }
  } else {
    d = data
  }
  return d
}
