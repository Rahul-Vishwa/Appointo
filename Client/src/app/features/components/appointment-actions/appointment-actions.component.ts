import { NgClass, NgTemplateOutlet, DatePipe } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnDestroy, OnInit, signal, WritableSignal } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription, map, forkJoin } from 'rxjs';
import { CalendarComponent } from '../../../shared/components/calendar/calendar.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { Time12hrPipe } from '../../../shared/pipes/time12hr.pipe';
import { ToastService } from '../../../shared/services/toast.service';
import { compare12HrTime, convert12HrToDate } from '../../../shared/utility/date';
import { LeaveForm, BookedSlots, GetLeave } from '../../models/Appointment';
import { AppointmentService } from '../../services/appointment/appointment.service';

@Component({
  selector: 'app-appointment-actions',
  imports: [
    CalendarComponent,
    ReactiveFormsModule,
    CardComponent,
    NgClass,
    Time12hrPipe,
    NgTemplateOutlet,
  ],
  templateUrl: './appointment-actions.component.html',
  styleUrl: './appointment-actions.component.css',
  schemas:[CUSTOM_ELEMENTS_SCHEMA]
})
export class AppointmentActionsComponent implements OnInit, OnDestroy {
  private subscriptions = new Subscription();
  form!: FormGroup<LeaveForm>;
  date = new Date();
  minDate = this.date;
  maxDate = new Date(this.date.getFullYear(), this.date.getMonth(), this.date.getDate() + 30);
  private appointment: AppointmentService = inject(AppointmentService);
  slots:WritableSignal<Array<{ id: number | null, name: string | null, userId: number | null, time:string, class:string, tooltip:string | null }>> = signal([]);
  bookedSlots:WritableSignal<Array<BookedSlots>> = signal([]); 
  editAppointmentId:WritableSignal<number | null> = signal(null);
  originalEditDate:WritableSignal<string | null> = signal(null);
  originalEditTime:WritableSignal<string | null> = signal(null);
  leaveMode: WritableSignal<boolean> = signal(false);
  leaveOption: WritableSignal<'edit' | 'take'> = signal('edit');
  leaveType: WritableSignal<'full' | 'firstHalf' | 'secondHalf' | 'custom'> = signal('full');
  leaveSlots: WritableSignal<Array<string>> = signal([]);
  leave:WritableSignal<GetLeave | null> = signal(null);
  showOptions: WritableSignal<string | null> = signal(null);
  editTime: WritableSignal<string | null> = signal(null);
  tooltipTime: WritableSignal<string | null> = signal(null);

  constructor(
    private formBuilder: FormBuilder,
    private toast: ToastService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ){}

  ngOnInit(){
    this.initializeForm();
    this.getSchedule();
  }

  private initializeForm(){
    this.form = this.formBuilder.group({
      date: new FormControl<string | null>(new DatePipe('en-IN').transform(new Date(), 'yyyy-MM-dd'), [Validators.required]),
      fromTime: new FormControl<string | null>(null, [Validators.required]),
      toTime: new FormControl<string | null>(null, [Validators.required]),
    });
  }

  getSlots(){
    return this.appointment.getSlots(
      this.form.controls['date'].value!
    )
    .pipe(
      map(slots=>
        slots.map((s:string) => {
          return { id: null, name: null, userId: null, time: s, class: 'available', tooltip: 'Available' }
        })
      ),
    )
  }
  
  getBookedSlots(){
    return this.appointment.getBookedSlotsWithDetails(
      this.form.controls['date'].value!
    )
  }
  
  getUserBookedSlots(){
    return this.appointment.getUserBookedSlots(
      this.form.controls['date'].value!
    );
  }

  private getLeave(){
    return this.appointment.getLeave(
      this.form.controls['date'].value!
    );
  }

  private setUnavailableSlots(){
    this.slots().forEach(slot => {
      if (this.leaveMode()){
        this.editLeave();
      }
      else{
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
      }


      const slotTime = convert12HrToDate(slot.time, this.form.controls['date'].value!);
      if (slotTime < new Date()){
        slot.class = 'unavailable';
        slot.tooltip = 'Unavailable';
        return;
      }      
      
      const bookedSlotsTime = this.bookedSlots().find(a => a.time === slot.time);

      if (bookedSlotsTime){
        if (this.editAppointmentId() === bookedSlotsTime.id){
          this.editTime.set(slot.time);
          return; 
        }
        slot.id = bookedSlotsTime.id;
        slot.name = bookedSlotsTime.name;
        slot.userId = bookedSlotsTime.userId;
        slot.class = 'booked';
        slot.tooltip = 'Booked';
      }
    });
  }

  getSchedule(){
    this.leaveSlots.set([]);
    this.editTime.set(null);
    this.form.controls['fromTime'].reset();
    this.form.controls['toTime'].reset();
    this.leaveMode.set(false);

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
        this.leave.set(result.leave);

        this.setUnavailableSlots();
      })
    );
  }

  selectSlot(time: string, slotClass: string){
    if (
      (this.leaveOption() === 'edit' && slotClass === 'unavailable') ||
      (this.leaveOption() === 'take' && (slotClass === 'unavailable' || slotClass === 'leave'))
    ){
      return;
    }
    const fromTime = this.form.controls['fromTime'].value; 
    const toTime = this.form.controls['toTime'].value; 

    if (!fromTime){
      this.form.controls['fromTime'].setValue(time);
      this.leaveSlots.set([]);
      this.leaveSlots().push(time);
    }
    else if (!toTime){
      if (compare12HrTime(
        this.form.controls['fromTime'].value!,
        time
      )){
        this.toast.info('Leave End Time cannot be smaller than Leave Start Time.')
        return;
      }
      this.form.controls['toTime'].setValue(time);

      this.slots().forEach(slot => {
        if (
          compare12HrTime(slot.time, fromTime, 'greater or equal') &&
          compare12HrTime(this.form.controls['toTime'].value!, slot.time, 'greater or equal')
        ){
          this.leaveSlots().push(slot.time);
        }
      });
    }
    else if (fromTime && toTime){
      this.form.controls['fromTime'].setValue(time);
      this.form.controls['toTime'].reset();

      this.leaveSlots.set([time]);

    }
  }

  private editLeave(){
    if (this.leave()?.leaveType){
      this.leaveSlots.set([]);
      this.slots().forEach(slot=>{
        if (this.leave() && this.leave()?.fromTime && this.leave()?.toTime){
          if (
            compare12HrTime(slot.time, this.leave()?.fromTime!, 'greater or equal') &&
            compare12HrTime(this.leave()?.toTime!, slot.time, 'greater or equal')
          ){
            slot.class = this.leaveOption() === 'edit' ? 'available' : 'unavailable';
            slot.tooltip = this.leaveOption() === 'edit' ? 'available' : 'unavailable';
            this.leaveSlots().push(slot.time);
            return;
          }
        }
      });
    }
  }


  takeLeave(type:'edit' | 'take'){
    this.leaveMode.set(true);
    this.originalEditDate.set(this.form.controls['date'].value);
    this.leaveOption.set(type);
    this.editLeave();
  }

  goBack(){
    this.editAppointmentId.set(null);
    this.editTime.set(null);
    this.leaveMode.set(false);
    this.form.controls['fromTime'].reset();
    this.form.controls['toTime'].reset();
    this.leaveSlots.set([]);
    this.leaveType.set('full');

    this.getSchedule();
  }

  applyLeave(){
    if (this.leave()?.leaveType){
      this.subscriptions.add(
        this.appointment.editLeave({
          ...this.form.value,
          leaveType: this.leaveType(),
          id: this.leave()?.id
        })
        .subscribe((status)=>{
          this.toast.info('Leave Updated.');
          if (status.status){
            this.toast.info(status.status);
          }
          this.goBack();
        })
      );
    }
    else{
      this.subscriptions.add(
        this.appointment.applyLeave({
          ...this.form.value,
          leaveType: this.leaveType()
        })
        .subscribe((status)=>{
          this.toast.info('Leave Applied.');
          if (status.status){
            this.toast.info(status.status);
          }
          this.goBack();
        })
      );
    }
  }

  cancelLeave(){
    this.subscriptions.add(
      this.appointment.cancelLeave(
        this.leave()?.id!
      )
      .subscribe(()=>{
        this.toast.info('Leave Canceled.');
        this.goBack();
      })
    )
  }

  onMouseEnter(slot:string){
    this.showOptions.set(slot);
  }

  onMouseLeave(){
    this.showOptions.set(null);
  }

  showTooltip(slot: string){
    this.tooltipTime.set(slot);
  }
  
  hideTooltip(){
    this.tooltipTime.set(null);
  }

  viewAppointments(userId: number, userName: string){
    this.router.navigate(['../appointments'], {
      queryParams: {
        id: userId,
        name: userName
      },
      relativeTo: this.activatedRoute
    });
  }

  cancelAppointment(id: number){
    this.subscriptions.add(
      this.appointment.cancel(id)
        .subscribe(()=>{
          this.toast.info('Appointment Cancelled.');
          this.getSchedule();
        })
    );   
  }

  editAppointment(slot:any){
    slot.class = 'available';
    slot.tooltip = 'available';
    this.editAppointmentId.set(slot.id);
    this.originalEditDate.set(this.form.controls['date'].value);
    this.originalEditTime.set(slot.time);
    this.editTime.set(slot.time);
  }

  setAppointmentTime(time: string){
    this.editTime.set(time);
  }

  updateAppointment(){
    if (this.editAppointmentId()){
      this.subscriptions.add(
        this.appointment.editSlot({
          id: this.editAppointmentId(),
          date: this.form.controls['date'].value,
          time: this.editTime()
        })
        .subscribe(()=>{
          this.toast.info('Appointment Updated.');
          this.goBack();
        })
      );
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
