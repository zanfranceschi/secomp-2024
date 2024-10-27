import { sleep } from 'k6';
import http from 'k6/http';
import { uuidv7 } from "https://unpkg.com/uuidv7@^1";
import { Counter } from 'k6/metrics';

export const options = {
    discardResponseBodies: true,
    scenarios: {
        banquo: {
            executor: 'ramping-vus',
            startVUs: 1,
            gracefulRampDown: "0s",
            stages: [
                { target: 600, duration: '3m' },
            ],
        },
    },
};

const okCounter = new Counter('ok');
const nokCounter = new Counter('nok');

export default function () {

    const start = Date.now();
    
    let url = "http://localhost:9999/transferencias";

    const payload = JSON.stringify({
        "valor": (Math.random() * 10000.0).toFixed(2),
        "clienteIdDe": uuidv7(),
        "clienteIPara": uuidv7(),
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const res = http.post(url, payload, params);

    if (res.status == 201 || res.status == 202) {
        okCounter.add(1);
        nokCounter.add(0);
    } else {
        nokCounter.add(1);
        okCounter.add(0);
    }

    const end = Date.now();
    const delta = end - start;
    const waitSecs = Math.max(0.0, 1.0 - (delta / 1000.0));

    sleep(waitSecs);
}
