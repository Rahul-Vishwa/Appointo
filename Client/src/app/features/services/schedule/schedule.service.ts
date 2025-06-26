import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GetSchedule, Schedule } from '../../models/schedule';

@Injectable({
  providedIn: 'root'
})
export class ScheduleService {

  constructor(
    private http:HttpClient,
  ) { }

  saveSchedule(data:any):Observable<any>{
    return this.http.post<any>('Schedule', data);
  }

  getSchedules(page: number, pageSize: number):Observable<GetSchedule>{
    return this.http.get<GetSchedule>('Schedule', {
      params:{
        page,
        pageSize
      }
    });
  }

  deleteSchedule(id: number):Observable<void>{
    return this.http.delete<void>(`Schedule/${id}`);
  }
}
