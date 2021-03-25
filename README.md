# inotify
[![后端docker构建](https://github.com/xpnas/Inotify/actions/workflows/dockerservice.yml/badge.svg)](https://github.com/xpnas/Inotify/actions/workflows/dockerservice.yml)
[![前端docker构建](https://github.com/xpnas/Inotify/actions/workflows/dockervue.yml/badge.svg)](https://github.com/xpnas/Inotify/actions/workflows/dockervue.yml)

a message notify center for weChat and telegram and email

一个简易的消息通知系统，支持企业微信、电报机器人、邮件推送

支持快速自定义扩展

类似Server酱

## 一、使用方法
  1. Docker安装
   ```
   docker run --name=inotify -d -p 8000:80 -v im_data:/im_data --restart=always xpnas/inotify:master
   ```

  2. 配置Nginx代理
   ```
   server
   {
    location / { proxy_pass http://127.0.0.1:8000; }
   }
   ```
 
  3. 默认用户名admin，密码123456
  
## 二、系统截图
  
![](../master/public/A.png)

![](../master/public/B.png)

![](../master/public/C.png)

![](../master/public/D.png)

