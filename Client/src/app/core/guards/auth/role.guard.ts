import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { UserService } from '../../services/user/user.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  return inject(UserService).getRole()
    .pipe(
        map(role => {
          localStorage.setItem('role', role.role);
          if (role.role !== 'Admin' && role.role !=='Customer'){
            router.navigate(['/unauthorized']);
          }
          return role.role === 'Admin' || 'Customer'
        }),
        catchError(err=>{
            console.log(err);
            router.navigate(['/unauthorized']);
            return of(err);
        })
    );
};
