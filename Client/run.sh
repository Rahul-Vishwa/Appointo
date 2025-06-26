#!/bin/bash

ng serve --host 0.0.0.0 --disable-host-check &
APP_PID=$!

trap "kill $APP_PID $CADDY_PID; exit" INT

sudo caddy reverse-proxy --from https://dev.taskzen.local --to http://localhost:4200 > /dev/null 2>&1 &
CADDY_PID=$!

wait $APP_PID

kill $APP_PID