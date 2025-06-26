import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PatientService {

  constructor(
    private http: HttpClient
  ) { }

  getPatientCode(): Observable<{patientCode: string}> {
    return this.http.get<{patientCode: string}>('Patient/PatientCode');
  }

}
