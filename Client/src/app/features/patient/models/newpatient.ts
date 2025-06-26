import { FormControl } from "@angular/forms";

export interface NewPatient {
    // Patient Registration Fields
    fullName: FormControl<string|null>;
    dateOfBirth: FormControl<Date|null>;
    gender: FormControl<string|null>;
    phoneNumber: FormControl<string|null>;
    emailAddress: FormControl<string|null>;
    address: FormControl<string|null>;

    // Medical Information
    bloodGroup: FormControl<string|null>;
    allergies: FormControl<string|null>;
    chronicConditions: FormControl<string|null>;
    medicalHistory: FormControl<string|null>;
    currentMedications: FormControl<string|null>;

    // Administrative Info
    patientCode: FormControl<string|null>;
    registrationDate: FormControl<Date|null>;
    assignedDoctor: FormControl<string|null>;
    insuranceDetails: FormControl<string|null>;

    // Attachments
    documents: FormControl<File|null>;
} 