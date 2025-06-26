import { Routes } from '@angular/router';
import { LandingPageComponent } from './features/components/landing-page/landing-page.component';
import { AuthService } from '@auth0/auth0-angular';
import { AuthCallbackComponent } from './core/components/auth-callback/auth-callback.component';
import { inject } from '@angular/core';
import { authGuard } from './core/guards/auth/auth.guard';

export const routes: Routes = [
    {
        path: '', 
        loadComponent: () => LandingPageComponent
    },
    {
        path: 'callback', 
        loadComponent: () => AuthCallbackComponent,
    },
    {
        path: 'home', 
        loadChildren: () => import('./routes/routes').then(m => m.routes),
        canActivate: [authGuard],
        canActivateChild: [authGuard]
    },
    {
        path: 'unauthorized',
        loadComponent: () => import('./core/components/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
    },
    {
        path: '**',
        loadComponent: () => import('./core/components/not-found/not-found.component').then(m => m.NotFoundComponent)
    }
];
