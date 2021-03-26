# inotify

[![docker](https://github.com/xpnas/inotify/actions/workflows/docker.yml/badge.svg)](https://github.com/xpnas/inotify/actions/workflows/docker.yml)

一个简易的消息通知系统，支持企业微信、电报机器人、邮件推送

类似Server酱、容易扩展

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
- [ ] 钉钉群机器人
- [ ] 飞书群机器人
- [ ] 自定义

## 使用方法
  1. Docker安装
   ```
   docker run --name=inotify -d -p 8000:80 -v inotify_data:/inotify_data --restart=always xpnas/inotify:latest
docker pull xpnas/inotify:latest
   ```

  2. 配置Nginx代理
   ```
   server
   {
    location / { proxy_pass http://127.0.0.1:8000; }
   }
   ```
 
  3. 默认用户名admin，密码123456
  
## 系统截图
  
![](../master/public/A.png)

![](../master/public/B.png)

![](../master/public/C.png)

![](../master/public/D.png)

