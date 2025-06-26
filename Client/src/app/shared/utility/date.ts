import { parse } from "date-fns";

export function convert12HrToDate(timeStr: string, date: string): Date {
    const [time, meridian] = timeStr.trim().split(' ');
    const [hoursStr, minutesStr] = time.split(':');
  
    let hours = parseInt(hoursStr, 10);
    const minutes = parseInt(minutesStr, 10);
  
    if (meridian === 'PM' && hours < 12) hours += 12;
    if (meridian === 'AM' && hours === 12) hours = 0;
  
    const parsedDate = new Date(date);
    parsedDate.setHours(hours, minutes, 0, 0);
    return parsedDate;
}

export function compare12HrTime(time1:string, time2: string, operator: 'greater' | 'greater or equal' = 'greater'):boolean{
    const to24Hour = (time: string): number => {
        const [h, mPart] = time.trim().split(':');
        const [m, meridian] = mPart.split(' ');
        let hour = parseInt(h);
        const minute = parseInt(m);

        if (meridian === 'PM' && hour !== 12) hour += 12;
        if (meridian === 'AM' && hour === 12) hour = 0;

        return hour * 60 + minute;
    };

    if (operator == 'greater'){
        return to24Hour(time1) > to24Hour(time2);
    }
    else {
        return to24Hour(time1) >= to24Hour(time2);
    }
}