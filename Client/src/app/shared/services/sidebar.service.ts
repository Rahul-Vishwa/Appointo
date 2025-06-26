import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SidebarService {
    private _isOpen = new BehaviorSubject<boolean>(false);
    open$ = this._isOpen.asObservable(); 

    expand(){
        this._isOpen.next(true);
    }

    shrink(){
        this._isOpen.next(false);
    }
}