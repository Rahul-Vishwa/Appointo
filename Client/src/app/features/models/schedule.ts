import { FormControl } from "@angular/forms";

export interface ScheduleForm {
    daysAvailable: FormControl<string[] | null>;
    startTime: FormControl<string | null>;
    endTime: FormControl<string | null>;
    breakStartTime: FormControl<string | null>;
    breakEndTime: FormControl<string | null>;
    slotDuration: FormControl<number | null>;
    effectiveFrom: FormControl<Date | null>;
}

export interface GetSchedule {
    schedule: Array<Schedule>;
    totalCount: number;
}

export interface Schedule {
    id: number;
    daysAvailable: string[];
    startTime: string;
    endTime: string;
    breakStartTime: string;
    breakEndTime: string;
    slotDuration: number;
    effectiveFrom: string;
    isActive: boolean;
    createdAt: string;
    createdBy: string;
}