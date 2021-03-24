# inotify
[![后端docker构建](https://github.com/xpnas/Inotify/actions/workflows/dockerservice.yml/badge.svg)](https://github.com/xpnas/Inotify/actions/workflows/dockerservice.yml)
[![前端docker构建](https://github.com/xpnas/Inotify/actions/workflows/dockervue.yml/badge.svg)](https://github.com/xpnas/Inotify/actions/workflows/dockervue.yml)

a message notify center for weChat and telegram and email

一个简易的消息通知系统，支持企业微信、电报机器人、邮件推送
类似Server酱

## 使用方法
  1. 下载docker-compose.yaml文件
  2. 执行docker-compose up -d
    ```
    docker-compose up -d
    ```
  4. 配置Nginx代理
   ```
   server
   {
    location / {
        proxy_pass http://127.0.0.1:8000;
        }
    location /api {
        proxy_pass http://127.0.0.1:8001;
        }
    }
   ```
   4. 默认用户名admin，密码123456

