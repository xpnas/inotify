FROM nginx
RUN mkdir /usr/share/nginx/dist
RUN rm -rf /etc/nginx/nginx.conf
COPY ./nginx.conf /etc/nginx/nginx.conf
COPY ./dist /usr/share/nginx/dist
EXPOSE 9099