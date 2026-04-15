import http from 'k6/http';
import { sleep } from 'k6';

export let options =
{
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
    { duration: '1m', target: 100 },
    { duration: '2m', target: 100 },
    { duration: '1m', target: 0 },
    ],
    thresholds: {
    http_req_duration: ['p(99)<1500'], // 99% of requests must complete below 1.5s
    },
};

export default () => {

    let response = http.get("http://5.189.139.171:8080/Movie/GetAllMovies");

    sleep(1);
};