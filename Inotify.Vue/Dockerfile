FROM nginx
RUN mkdir /usr/share/nginx/dist
RUN rm -rf /etc/nginx/nginx.conf
COPY ./nginx.conf /etc/nginx/nginx.conf
COPY ../inotify/dist /usr/share/nginx/dist
EXPOSE 9099