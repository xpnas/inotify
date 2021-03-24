import request from '@/utils/request'

export function login(data) {
  return request({
    url: '/oauth/login',
    method: 'post',
    params: {
      username: data.username,
      password: data.password
    }
  })
}

export function githubLogin() {
  return request({
    url: '/oauth/githublogin',
    method: 'get'
  })
}

export function githubEnable() {
  return request({
    url: '/oauth/GithubEnable',
    method: 'get'
  })
}

export function resetPassword(password) {
  return request({
    url: '/oauth/resetPassword',
    method: 'post',
    params: {
      password: password
    }
  })
}

export function getInfo(token) {
  return request({
    url: '/oauth/info',
    method: 'get',
    params: { token }
  })
}

export function logout() {
  return request({
    url: '/oauth/logout',
    method: 'post'
  })
}
