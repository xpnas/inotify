const Mock = require('mockjs')

const data = Mock.mock({
  'items|30': [
    {
      id: '@id',
      title: '@sentence(10, 20)',
      'status|1': ['published', 'draft', 'deleted'],
      author: 'name',
      display_time: '@datetime',
      pageviews: '@integer(300, 5000)'
    }
  ]
})
const sendTemeplateData = [
  {
    name: '邮件推送',
    key: 'EA2B43F7-956C-4C01-B583-0C943ABB36C3',
    values: [
      {
        name: 'FromAddress',
        description: '\u53D1\u4EF6\u5730\u5740',
        default: 'abc@qq.com',
        type: 0,
        order: 0
      },
      {
        name: 'FromPasssWord',
        description: '\u53D1\u4EF6\u5BC6\u7801',
        default: '123456789',
        type: 0,
        order: 1
      },
      {
        name: 'FromServer',
        description: '\u53D1\u4EF6SMTP',
        default: 'stmp.qq.com',
        type: 0,
        order: 2
      },
      { name: 'EnableSSL', description: 'SSL', default: 'true|false', type: 1, order: 3 },
      {
        name: 'ToAddress',
        description: '\u6536\u4EF6\u7BB1',
        default: 'abcd@qq.com',
        type: 0,
        order: 4
      }
    ]
  },
  {
    key: '409A30D5-ABE8-4A28-BADD-D04B9908D763',
    name: '企业微信',
    values: [
      {
        name: 'Corpid',
        description: '\u4F01\u4E1AID',
        default: 'Corpid',
        type: 0,
        order: 0
      },
      {
        name: 'Corpsecret',
        description: '\u5BC6\u94A5',
        default: 'Corpsecret',
        type: 0,
        order: 1
      },
      {
        name: 'AgentID',
        description: '\u5E94\u7528ID',
        default: 'AgentID',
        type: 0,
        order: 2
      }
    ]
  }
]
const sendAuths = {
  code: 200,
  data: [
    {
      key: '409A30D5-ABE8-4A28-BADD-D04B9908D763',
      type: '微信推送',
      name: '测试',
      isActive: true,
      auth: '{\u0022Corpid\u0022:\u0022ww4199b1ecd7dcecba\u0022,\u0022Corpsecret\u0022:\u0022kZUQf52AMYAMsxPGXEiQsHISLwjHhHAnyPXYKLPdoo4\u0022,\u0022AgentID\u0022:\u00221000002\u0022}',
      values: [
        { name: 'Corpid', description: '企业ID', default: 'Corpid', type: 1, order: 0, value: 'ww4199b1ecd7dcecba' },
        { name: 'Corpsecret', description: '密钥', default: 'Corpsecret', type: 1, order: 1, value: 'kZUQf52AMYAMsxPGXEiQsHISLwjHhHAnyPXYKLPdoo4' },
        { name: 'AgentID', description: '应用ID', default: 'AgentID', type: 1, order: 2, value: '1000002' }
      ]
    }
  ]
}

const sendKeyData = '3015679CB0DC462C89F2E37779540894'

module.exports = [
  // user login
  {
    url: '/setting/getSendTemplates',
    type: 'get',
    response: config => {
      const items = sendTemeplateData
      return {
        code: 200,
        data: sendTemeplateData
      }
    }
  },
  {
    url: '/setting/getSendAuths',
    type: 'get',
    response: config => {
      return sendAuths
    }
  },
  {
    url: '/setting/getSendKey',
    type: 'get',
    response: config => {
      return {
        code: 200,
        data: sendKeyData
      }
    }
  },
  {
    url: '/setting/reSendKey',
    type: 'get',
    response: config => {
      return {
        code: 200,
        data: true
      }
    }
  },
  {
    url: '/send',
    type: 'get',
    response: config => {
      return {
        code: 200,
        data: true
      }
    }
  },
  {
    url: '/setting/deleteSendAuth',
    type: 'get',
    response: config => {
      return {
        code: 200,
        data: true
      }
    }
  },
  {
    url: '/setting/ActiveSendAuth',
    type: 'get',
    response: config => {
      return {
        code: 200,
        data: true
      }
    }
  },
  {
    url: '/setting/addSendAuth',
    type: 'post',
    response: config => {
      return {
        code: 200,
        data: true
      }
    }
  },
  {
    url: '/setting/modifySendAuth',
    type: 'post',
    response: config => {
      return {
        code: 200,
        data: true
      }
    }
  }
]
