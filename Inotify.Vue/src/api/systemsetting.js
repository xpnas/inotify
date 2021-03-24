import request from '@/utils/request'

export function getGlobal() {
  return request({
    url: '/settingsys/GetGlobal',
    method: 'get'
  })
}

export function setGlobal(data) {
  return request({
    url: '/settingsys/setGlobal',
    method: 'post',
    params: data
  })
}

export function getJWT() {
  return request({
    url: '/settingsys/getJWT',
    method: 'get'
  })
}

export function setJWT(data) {
  return request({
    url: '/settingsys/setJWT',
    method: 'post',
    data: data
  })
}

export function getGithubEnable() {
  return request({
    url: '/settingsys/getGithubEnable',
    method: 'get'
  })
}

export function getUsers(query, page, pagesize) {
  return request({
    url: '/settingsys/getUsers',
    method: 'get',
    params: { query: query, page: page, pagesize: pagesize }
  })
}

export function activeUser(userName, active) {
  return request({
    url: '/settingsys/activeUser',
    method: 'post',
    params: { userName: userName, active: active }
  })
}

export function deleteUser(userName) {
  return request({
    url: '/settingsys/deleteUser',
    method: 'post',
    params: { userName: userName }
  })
}

export function getSendTypeInfos(start, end) {
  return request({
    url: '/settingsys/GetSendTypeInfos',
    method: 'get',
    params: { start: start, end: end }
  })
}

export function getSendInfos(start, end) {
  return request({
    url: '/settingsys/GetSendInfos',
    method: 'get',
    params: { start: start, end: end }
  })
}

