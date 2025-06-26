import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GetAnalyticsToday, GetAppointmentCount, GetPercentageAnalytics } from '../../models/dashboard';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  constructor(
    private http: HttpClient
  ) { }

  getAnalyticsToday(): Observable<GetAnalyticsToday>{
    return this.http.get<GetAnalyticsToday>('Dashboard/GetAnalyticsToday');
  }

  getAppointmentCountPast7Days(): Observable<Array<GetAppointmentCount>>{
    return this.http.get<Array<GetAppointmentCount>>('Dashboard/GetAppointmentCountPast7Days');
  }

  getAppointmentCountThisMonth(): Observable<Array<GetAppointmentCount>>{
    return this.http.get<Array<GetAppointmentCount>>('Dashboard/GetAppointmentCountThisMonth');
  }
  
  getcancelledAppointmentCountPast7Days(): Observable<Array<GetAppointmentCount>>{
    return this.http.get<Array<GetAppointmentCount>>('Dashboard/GetCancelledAppointmentCountPast7Days');
  }
  getCancelledAppointmentCountThisMonth(): Observable<Array<GetAppointmentCount>>{
    return this.http.get<Array<GetAppointmentCount>>('Dashboard/GetCancelledAppointmentCountThisMonth');
  }
  
  getPercentageAnalytics(): Observable<GetPercentageAnalytics>{
    return this.http.get<GetPercentageAnalytics>('Dashboard/GetPercentageAnalytics');
  }
}
