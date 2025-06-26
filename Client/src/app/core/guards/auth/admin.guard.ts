import { inject } from '@angular/core';
import { CanActivateFn, Router, RouterLinkWithHref } from '@angular/router';
import { UserService } from '../../services/user/user.service';
import { catchError, map, of, tap } from 'rxjs';

export const adminGuard: CanActivateFn = (route, state) => {
  const user = inject(UserService);
  const router = inject(Router);

  return user.getRole().pipe(
      map(role => {
        localStorage.setItem('role', role.role);
        const isAdmin = role.role === 'Admin';
        if (!isAdmin) {
          router.navigate(['/unauthorized']);
          return false;
        }
        return isAdmin;
      }),
      catchError(err => {
        console.log(err);
        router.navigate(['/unauthorized']);
        return of(false);
      })
    );
};
