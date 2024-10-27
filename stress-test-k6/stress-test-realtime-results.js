import { sleep } from 'k6';
import { Httpx } from 'https://jslib.k6.io/httpx/0.1.0/index.js';
import { uuidv7 } from "https://unpkg.com/uuidv7@^1";
import { Counter } from 'k6/metrics';

export const options = {
    discardResponseBodies: true,
    scenarios: {
        banquo: {
            executor: 'ramping-vus',
            startVUs: 1,
            gracefulRampDown: '0s',
            stages: [
                { target: 10, duration: '10s' },   // warmup
                { target: 500, duration: '170s' }, // teste
            ],
        },
    },
};

const http = new Httpx({
    baseURL: 'http://localhost:9999',
    headers: {
        'Content-Type': 'application/json',
    },
    timeout: 10000,
});

const okCounter = new Counter('ok');
const nokCounter = new Counter('nok');

const reportMetricNok = () => {
    nokCounter.add(1);
    okCounter.add(0);
};

const reportMetricOk = () => {
    okCounter.add(1);
    nokCounter.add(0);
};

export default function () {

    const payload = JSON.stringify({
        valor: (Math.random() * 10000.0).toFixed(2),
        clienteIdDe: uuidv7(),
        clienteIPara: uuidv7(),
    });

    const response = http.post('/transferencias', payload);

    if ([200, 201, 202, 204].includes(response.status)) {
        reportMetricOk();
    } else {
        reportMetricNok();
    }

    const httpDurationSecs = response.timings.duration / 1000.0;
    const waitSecs = Math.max(0.0, 1.0 - httpDurationSecs);

    sleep(waitSecs);
}
