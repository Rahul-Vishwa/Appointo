import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { User } from '@auth0/auth0-angular';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http:HttpClient) { }

  saveUser(user:User): Observable<any>{
    return this.http.post('User', user);
  }

  assignRole(role: string): Observable<void>{
    return this.http.post<void>('User/AssignRole', { role });
  }

  getRole(): Observable<{ role: string }>{
    return this.http.get<{ role: string }>('User/GetRole');
  }
}
