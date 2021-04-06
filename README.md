# inotify

[![docker](https://github.com/xpnas/inotify/actions/workflows/docker.yml/badge.svg)](https://github.com/xpnas/inotify/actions/workflows/docker.yml)

一个简易的消息通知系统，支持企业微信、电报机器人、邮件推送

类似Server酱、容易扩展

示例站点：https://inotify.cf

## 功能支持

- [x] 通道设置  
- [x] 用户管理
- [x] 系统状态 
- [x] 代理设置
- [x] Github登陆
- [ ] 外部日志

## 通道支持

- [x] 企业微信应用消息
- [x] 电报机器人消息
- [x] SMTP邮箱消息
- [x] BARK
- [x] 钉钉群机器人
- [x] 飞书群机器人
- [x] 自定义

## 更新日志

  * V1.0
    * 支持企业微信应用、电报、SMTP消息
  * V2.0.0.1
    * 支持自定义Get、POST
  * V2.0.0.2
    * 支持BARK
  * V2.0.0.3
    * 支持钉钉群消息
    * 支持飞书群消息
  * V2.0.0.4
    * 支持通道独立消息推送

## 使用方法
  1. Docker安装 
   * 稳定版V2.0
  ```
  docker run --name=inotify -d -p 8000:80 -v inotify_data:/inotify_data --restart=always xpnas/inotify:latest
  ```
  * 开发版V2.0.0.5 
  ```
  docker run --name=inotify -d -p 8000:80 -v inotify_data:/inotify_data --restart=always xpnas/inotify:master
  ```
  2. 配置Nginx代理
  ```
  server
  {
    location / { proxy_pass http://127.0.0.1:8000; }
  }
  ```

  3. 进入`Github/Settings/Developer settings/OAuth Apps`创建应用
      * 记录`Client ID`，创建`Client secrets`
      *  `Authorization callback URL`回调地址填写https://{您的域名}/api/oauth/githubLogin
  4. 使用`默认用户名admin，密码123456`登陆后台/全局参数,修改Github登陆的`应用ID`、`应用密钥`并启动登陆
  5. 建议将`管理权限`的用户名设置成自己的github用户名，再使用Github登陆后，在用户管理页面`删除默认账号admin`

## BARK设置
  1. 本项目依据Bark-Server接口规范实现了内置BARK服务端
  2. 复制或扫码`消息验证\BARK授权`中的地址，填入BARK应用的服务器地址中,如`https://inotify.cf?act=6D474C0DB1474F19BD8F7342D570C0FC`
  3. BARK的APP会自动在本系统注册数据，记录将直接出现在`消息通道`

## 系统截图
  
![](../master/public/A.png)

![](../master/public/B.png)

![](../master/public/C.png)

![](../master/public/D.png)

