import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../../services/user/user.service';
import { inject } from '@angular/core';
import { map, catchError, of, tap } from 'rxjs';

export const customerGuard: CanActivateFn = (route, state) => {
  const user = inject(UserService);
  const router = inject(Router);

  return user
    .getRole()
    .pipe(
      map(role => {
        localStorage.setItem('role', role.role);
        const isCustomer = role.role === 'Customer';
        if (!isCustomer) {
          router.navigate(['/unauthorized']);
          return false;
        }
        return isCustomer;
      }),
      catchError(err => {
        console.log(err);
        router.navigate(['/unauthorized']);
        return of(false);
      })
    );
};
