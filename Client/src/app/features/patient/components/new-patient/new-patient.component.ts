import { Component, CUSTOM_ELEMENTS_SCHEMA, OnInit } from '@angular/core';
import { CardComponent } from "../../../../shared/components/card/card.component";
import { InputComponent } from "../../../../shared/components/input/input.component";
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NewPatient } from '../../models/newpatient';
import { DropdownComponent } from '../../../../shared/components/dropdown/dropdown.component';
import { TextareaComponent } from "../../../../shared/components/textarea/textarea.component";
import { DatepickerComponent } from "../../../../shared/components/datepicker/datepicker.component";
import { PatientService } from '../../services/patient.service';

@Component({
  selector: 'app-new-patient',
  imports: [CardComponent, InputComponent, ReactiveFormsModule, DropdownComponent, TextareaComponent, DatepickerComponent],
  templateUrl: './new-patient.component.html',
  styleUrl: './new-patient.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class NewPatientComponent implements OnInit {
  form!: FormGroup<NewPatient>;
  
  genderOptions = [
    { value: 'male', label: 'Male' },
    { value: 'female', label: 'Female' },
    { value: 'other', label: 'Other' }
  ];

  bloodGroupOptions = [
    { value: 'A+', label: 'A+' },
    { value: 'A-', label: 'A-' },
    { value: 'B+', label: 'B+' },
    { value: 'B-', label: 'B-' },
    { value: 'AB+', label: 'AB+' },
    { value: 'AB-', label: 'AB-' },
    { value: 'O+', label: 'O+' },
    { value: 'O-', label: 'O-' }
  ];

  doctorOptions = [
    { value: 'dr1', label: 'Dr. John Smith' },
    { value: 'dr2', label: 'Dr. Sarah Johnson' },
    { value: 'dr3', label: 'Dr. Michael Brown' }
  ];
  maxDate = new Date();

  constructor(
    private formBuilder: FormBuilder,
    private patientService: PatientService
  ) {}

  ngOnInit() {
    this.initializeForm();
    this.getPatientCode();
  }

  initializeForm() {
    this.form = this.formBuilder.group({
      // Patient Registration Fields
      fullName: new FormControl<string|null>(null, [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100),
        Validators.pattern(/^[a-zA-Z\s]*$/)
      ]),
      dateOfBirth: new FormControl<Date|null>(null, [
        Validators.required,
      ]),
      gender: new FormControl<string|null>(null, [Validators.required]),
      phoneNumber: new FormControl<string|null>(null, [
        Validators.required
      ]),
      emailAddress: new FormControl<string|null>(null, [
        Validators.email,
        Validators.maxLength(100)
      ]),
      address: new FormControl<string|null>(null, [
        Validators.maxLength(250)
      ]),

      // Medical Information
      bloodGroup: new FormControl<string|null>(null),
      allergies: new FormControl<string|null>(null, [
        Validators.maxLength(100)
      ]),
      chronicConditions: new FormControl<string|null>(null, [
        Validators.maxLength(300)
      ]),
      medicalHistory: new FormControl<string|null>(null, [
        Validators.maxLength(500)
      ]),
      currentMedications: new FormControl<string|null>(null, [
        Validators.maxLength(300)
      ]),

      // Administrative Info
      patientCode: new FormControl<string|null>(null),
      registrationDate: new FormControl<Date|null>(new Date()),
      assignedDoctor: new FormControl<string|null>(null),
      insuranceDetails: new FormControl<string|null>(null, [
        Validators.maxLength(300)
      ]),

      // Attachments
      documents: new FormControl<File|null>(null)
    });
  }

  private getPatientCode() {
    this.patientService.getPatientCode().subscribe((data) => {
      this.form.get('patientCode')?.setValue(data.patientCode);
    });
  }
}
