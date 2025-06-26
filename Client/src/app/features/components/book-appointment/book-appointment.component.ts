import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnDestroy, OnInit, signal, WritableSignal } from '@angular/core';
import { CalendarComponent } from "../../../shared/components/calendar/calendar.component";
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { BookAppointment, GetLeave } from '../../models/Appointment';
import { CardComponent } from '../../../shared/components/card/card.component';
import { AppointmentService } from '../../services/appointment/appointment.service';
import { DatePipe, NgClass } from '@angular/common';
import { Time12hrPipe } from '../../../shared/pipes/time12hr.pipe';
import { ToastService } from '../../../shared/services/toast.service';
import { compare12HrTime, convert12HrToDate } from '../../../shared/utility/date';
import { forkJoin, map, Subscription } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-book-appointment',
  imports: [
    CalendarComponent,
    ReactiveFormsModule,
    CardComponent,
    NgClass,
    Time12hrPipe,
],
  templateUrl: './book-appointment.component.html',
  styleUrl: './book-appointment.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class BookAppointmentComponent implements OnInit, OnDestroy {
  private subscriptions = new Subscription();
  form!: FormGroup<BookAppointment>;
  date = new Date();
  minDate = this.date;
  maxDate = new Date(this.date.getFullYear(), this.date.getMonth(), this.date.getDate() + 30);

  private appointment: AppointmentService = inject(AppointmentService);
  slots: WritableSignal<Array<{ time:string, class:string, tooltip:string | null }>> = signal([]);
  bookedSlots: WritableSignal<Array<string>> = signal([]); 
  userBookedSlot: WritableSignal<{ id: number, time: string } | null> = signal(null); 
  editAppointmentId: WritableSignal<number | null> = signal(null);
  leave: WritableSignal<GetLeave | null> = signal(null);
  currentSlot: WritableSignal<string | null> = signal(null);

  constructor(
    private formBuilder: FormBuilder,
    private toast: ToastService,
    private activatedRoute: ActivatedRoute
  ){}

  ngOnInit(){
    this.initializeForm();
  }

  private initializeForm(){
    this.form = this.formBuilder.group({
      date: new FormControl<string | null>(new DatePipe('en-IN').transform(new Date(), 'yyyy-MM-dd'), [Validators.required]),
      time: new FormControl<string | null>(null, [Validators.required])
    });


    this.activatedRoute.queryParams.subscribe(params=>{
      if (params['id'] && params['date']){
        this.editAppointmentId.set(params['id']);
        this.form.controls['date'].setValue(params['date']);
      }
    });

    this.getSchedule();
  }

  getSlots(){
    return this.appointment.getSlots(
      this.form.controls['date'].value!
    )
    .pipe(
      map(slots=>
        slots.map((s:string) => {
          return { time: s, class: 'available', tooltip: 'Available' }
        })
      ),
    )
  }

  getBookedSlots(){
    return this.appointment.getBookedSlots(
      this.form.controls['date'].value!
    )
  }
  
  getUserBookedSlots(){
    return this.appointment.getUserBookedSlots(
      this.form.controls['date'].value!
    );
  }

  private setUnavailableSlots(){
    this.slots().forEach(slot => {
      if (this.leave() && this.leave()?.fromTime && this.leave()?.toTime){
        if (
          compare12HrTime(slot.time, this.leave()?.fromTime!, 'greater or equal') &&
          compare12HrTime(this.leave()?.toTime!, slot.time, 'greater or equal')
        ){
          slot.class = 'leave';
          slot.tooltip = 'On Leave';
          return;
        }
      }

      if (this.userBookedSlot()?.time === slot.time){
        slot.class = 'user-booked';
        slot.tooltip = 'Your Appointment';
        return;
      }

      const slotTime = convert12HrToDate(slot.time, this.form.controls['date'].value!);
      if (slotTime < new Date()){
        slot.class = 'unavailable';
        slot.tooltip = 'Unavailable';
        return;
      }      
      
      if (this.bookedSlots().includes(slot.time)){
        slot.class = 'booked';
        slot.tooltip = 'Booked';
      }
    });
  }

  private getLeave(){
    return this.appointment.getLeave(
      this.form.controls['date'].value!
    );
  }

  getSchedule(){
    this.form.controls['time'].reset();

    const apis = {
      slots: this.getSlots(),
      bookedSlots: this.getBookedSlots(),
      userBookedSlot: this.getUserBookedSlots(),
      leave: this.getLeave()
    };

    this.subscriptions.add(
      forkJoin(apis).subscribe(result=>{
        this.slots.set(result.slots);
        this.bookedSlots.set(result.bookedSlots);
        this.userBookedSlot.set(result.userBookedSlot);
        this.leave.set(result.leave);

        if (this.leave()?.leaveType === 'full'){
          return;
        }
        this.setUnavailableSlots();
      })
    );
  }

  onMouseEnter(slot: string){
    this.currentSlot.set(slot);
  }

  onMouseLeave(){
    this.currentSlot.set(null);
  }

  selectSlot(slot:string){
    if (
      this.editAppointmentId() && 
      (
        this.userBookedSlot()?.id === this.editAppointmentId() ||
        !this.userBookedSlot()
      )
    ){
      this.form.controls['time'].setValue(slot);
    }
    else if (!this.editAppointmentId() && !this.userBookedSlot()){
      this.form.controls['time'].setValue(slot);
    }
  }

  isPastAppointment(){
    const parsedDate = convert12HrToDate(
      this.userBookedSlot()?.time!, 
      this.form.controls['date']?.value!, 
    );
    return parsedDate <= new Date();
  }

  book(){
    if (this.editAppointmentId()){
      this.subscriptions.add(
        this.appointment.editSlot({
          id: this.editAppointmentId(),
          ...this.form.value
        })
        .subscribe(()=>{
          this.toast.info('Appointment Updated.');
          this.editAppointmentId.set(null);
          this.getSchedule();
        })
      );
    }
    else{
      this.subscriptions.add(
        this.appointment.book(this.form.value)
          .subscribe({
            next:()=>{
              this.toast.info('Appointment Booked.');
              this.getSchedule();
            }
          })
      );
    }
  }

  edit(){
    this.editAppointmentId.set(this.userBookedSlot()!.id);
  }

  cancel(){
    this.subscriptions.add(
      this.appointment.cancel(
        this.userBookedSlot()?.id!
      )
      .subscribe(() => {
        this.toast.info('Appointment Cancelled.');
        this.getSchedule();
      })
    );
  }

  goBack(){
    this.form.controls['time'].reset();
    this.editAppointmentId.set(null);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
