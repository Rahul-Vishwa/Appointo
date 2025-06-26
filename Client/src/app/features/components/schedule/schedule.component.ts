import { Component, CUSTOM_ELEMENTS_SCHEMA, OnInit, signal, WritableSignal } from '@angular/core';
import { CardComponent } from "../../../shared/components/card/card.component";
import { Schedule, ScheduleForm } from '../../models/schedule';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DatePipe, NgClass } from '@angular/common';
import { TimepickerComponent } from "../../../shared/components/timepicker/timepicker.component";
import { DropdownComponent } from "../../../shared/components/dropdown/dropdown.component";
import { DatepickerComponent } from "../../../shared/components/datepicker/datepicker.component";
import { ScheduleService } from '../../services/schedule/schedule.service';
import { ToastService } from '../../../shared/services/toast.service';
import { Time12hrPipe } from '../../../shared/pipes/time12hr.pipe';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-schedule',
  imports: [
    CardComponent,
    NgClass,
    TimepickerComponent,
    DropdownComponent,
    DatepickerComponent,
    ReactiveFormsModule,
    Time12hrPipe,
    DatePipe
],
  templateUrl: './schedule.component.html',
  styleUrl: './schedule.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class ScheduleComponent implements OnInit {
  private subscriptions = new Subscription();
  form!: FormGroup<ScheduleForm>;
  availableOn:string[] = [
    'Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'
  ];

  slotDuration = [
    {label:'5 mins', value: 5},
    {label:'10 mins', value: 10},
    {label:'15 mins', value: 15},
    {label:'20 mins', value: 20},
    {label:'30 mins', value: 30},
  ];

  minEffectiveFromDate = new Date();

  schedules: WritableSignal<Array<Schedule>> = signal([]); 
  pagination: { page: number, pageSize: number, totalCount: number, totalPages: number } = {
    page: 1,
    pageSize: 5,
    totalCount: 0,
    totalPages: 0
  };


  constructor(
    private formBuilder:FormBuilder,
    private schedule:ScheduleService,
    private toast:ToastService
  ){}

  ngOnInit(): void {
    this.minEffectiveFromDate.setDate(new Date().getDate() + 1);

    this.initializeForm();
    this.getSchedules();
  }

  private initializeForm(){
    this.form = this.formBuilder.group({
      daysAvailable: new FormControl<string[]>([], [Validators.required]),
      startTime: new FormControl<string | null>(null, [Validators.required]),
      endTime: new FormControl<string | null>(null, [Validators.required]),
      breakStartTime: new FormControl<string | null>(null, [Validators.required]),
      breakEndTime: new FormControl<string | null>(null, [Validators.required]),
      slotDuration: new FormControl<number | null>(null, [Validators.required]),
      effectiveFrom: new FormControl<Date | null>(null, [Validators.required]),
    });
  }

  private getSchedules(){
    this.subscriptions.add(
      this.schedule.getSchedules(
        this.pagination.page,
        this.pagination.pageSize
      )
      .subscribe(schedule=>{
        this.schedules.set(schedule.schedule);
        this.pagination.totalCount = schedule.totalCount;
        this.pagination.totalPages = Math.ceil(schedule.totalCount / this.pagination.pageSize);
      })
    );
  }

  decreasePage(){
    if (this.pagination.page > 1){
      this.pagination.page -= 1;
      this.getSchedules();
    }
  }

  increasePage(){
    if (this.pagination.page < this.pagination.totalPages){
      this.pagination.page += 1;
      this.getSchedules();
    }
  }

  selectDay(day:string){
    const selectedDays = this.form.controls['daysAvailable'].value || [];

    if (selectedDays.includes(day)){
      selectedDays.splice(
        selectedDays.indexOf(day),
        1
      );
    }
    else{
      selectedDays.push(day);
    }
    this.form.controls['daysAvailable'].setValue(selectedDays);
  }
  
  private cleanUp(control1:FormControl, control2:FormControl, str1:string, str2: string){
    control1.reset();
    control2.reset();
    this.toast.info(`${str1} cannot be greater than the ${str2}.`);
  }

  private wrongTime(control1:FormControl, control2:FormControl, str1:string, str2: string){
    if (control1.value && control2.value){
      const [h1, m1] = control1.value.split(':').map(Number);
      const [h2, m2] = control2.value.split(':').map(Number);
      
      if (h1 < h2) return;
      if (h1 > h2) {
        this.cleanUp(control1, control2, str1, str2);
        return;
      }
      if (m1 > m2){
        this.cleanUp(control1, control2, str1, str2);
        return;
      }
    }
  }
  
  onTimeChange(){
    this.wrongTime(
      this.form.controls['startTime'],
      this.form.controls['endTime'],
      'Start Time',
      'End Time',
    );
    this.compareBreakWithStartAndEndTime();
  }

  private compareBreakWithStartAndEndTime(){
    this.wrongTime(
      this.form.controls['startTime'],
      this.form.controls['breakStartTime'],
      'Start Time',
      'Break Start Time',
    );
    this.wrongTime(
      this.form.controls['startTime'],
      this.form.controls['breakEndTime'],
      'Start Time',
      'Break End Time',
    );
    this.wrongTime(
      this.form.controls['breakEndTime'],
      this.form.controls['endTime'],
      'Break End Time',
      'End Time',
    );
    this.wrongTime(
      this.form.controls['breakStartTime'],
      this.form.controls['endTime'],
      'Break Start Time',
      'End Time',
    );
  }
  
  onBreakTimeChange(){
    this.wrongTime(
      this.form.controls['breakStartTime'],
      this.form.controls['breakEndTime'],
      'Break Start Time',
      'Break End Time',
    );
    this.compareBreakWithStartAndEndTime();
  }

  save(){
    this.subscriptions.add(
      this.schedule.saveSchedule(this.form.value)
      .subscribe(()=>{
        this.toast.info('Schedule Updated.');
        this.form.reset();
        this.getSchedules();
      })
    );
  }

  delete(id: number){
    this.subscriptions.add(
      this.schedule.deleteSchedule(id)
        .subscribe(()=>{
          this.toast.info('Schedule Deleted.');
          this.getSchedules();
        })
    );
  }
}
