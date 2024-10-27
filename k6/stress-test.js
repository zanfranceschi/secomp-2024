import { sleep } from 'k6';
import http from 'k6/http';
import { uuidv7 } from "https://unpkg.com/uuidv7@^1";

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

export default function () {

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

    const waitSecs = Math.max(0.0, 1.0 - (res.timings.duration / 1000.0));

    sleep(waitSecs);
}
