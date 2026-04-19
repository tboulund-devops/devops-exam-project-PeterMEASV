FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --ignore-scripts
COPY . .
RUN npm run build

FROM nginx:alpine
ARG SERVER_IP=5.189.151.230
COPY --from=build /app/dist /usr/share/nginx/html

RUN printf 'server {\n\
    listen 8080;\n\
    location /api/ {\n\
        proxy_pass http://%s:8080/api/;\n\
    }\n\
    location /Movie/ {\n\
        proxy_pass http://%s:8080/Movie/;\n\
    }\n\
    location /Genre/ {\n\
        proxy_pass http://%s:8080/Genre/;\n\
    }\n\
    location /Storage/ {\n\
        proxy_pass http://%s:8080/Storage/;\n\
    }\n\
    location / {\n\
        root /usr/share/nginx/html;\n\
        index index.html index.htm;\n\
        try_files $uri $uri/ /index.html;\n\
    }\n\
}\n' "$SERVER_IP" "$SERVER_IP" "$SERVER_IP" "$SERVER_IP" > /etc/nginx/conf.d/default.conf

EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]