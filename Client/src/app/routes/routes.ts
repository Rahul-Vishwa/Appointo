import { Router, Routes } from '@angular/router';
import { adminGuard } from '../core/guards/auth/admin.guard';
import { customerGuard } from '../core/guards/auth/customer.guard';
import { inject } from '@angular/core';
import { UserService } from '../core/services/user/user.service';
import { catchError, map, of } from 'rxjs';
import { roleGuard } from '../core/guards/auth/role.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('../features/components/home/home.component').then(m => m.HomeComponent),
        canActivate: [roleGuard],
        children: [
            {
                path: 'dashboard',
                loadComponent: () => import('../features/components/dashboard/dashboard.component').then(m => m.DashboardComponent),
                canActivate: [adminGuard]
            },
            {
                path: 'schedule',
                loadComponent: () => import('../features/components/schedule/schedule.component').then(m => m.ScheduleComponent),
                canActivate: [adminGuard]
            },
            {
                path: 'appointment-actions',
                loadComponent: () => import('../features/components/appointment-actions/appointment-actions.component').then(m => m.AppointmentActionsComponent),
                canActivate: [adminGuard]
            },
            {
                path: 'book-appointment',
                loadComponent: () => import('../features/components/book-appointment/book-appointment.component').then(m => m.BookAppointmentComponent),
                canActivate: [customerGuard]
            },
            {
                path: 'appointments',
                loadComponent: () => import('../features/components/my-appointments/my-appointments.component').then(m => m.MyAppointmentsComponent),
                canActivate: [roleGuard]
            },
        ]
    }
]; 