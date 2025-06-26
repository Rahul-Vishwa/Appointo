import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideAuth0 } from '@auth0/auth0-angular';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { httpInterceptor } from './core/interceptors/http.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    provideAuth0({
      domain: 'codeaux.us.auth0.com',
      clientId: 'kQZL1x5ZEuy5bMZIS7u900UVbfgUrnoK',
      authorizationParams: {
        redirect_uri: 'https://dev.taskzen.local/callback/',
        audience: 'https://api.taskzen.local/'
      },
      httpInterceptor: {
        allowedList: [
          {
            uri: 'http://localhost:5093/*',
          }
        ]
      }
    }),
    provideHttpClient(
      withInterceptors([httpInterceptor, errorInterceptor])
    ),
  ],
};
