import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'time12hr'
})
export class Time12hrPipe implements PipeTransform {

  transform(value: string): string {
    if (!value) return '';

    const [hourStr, minute] = value.split(':');
    let hour = parseInt(hourStr, 10);
    const ampm = hour >= 12 ? 'PM' : 'AM';

    hour = hour % 12;
    hour = hour === 0 ? 12 : hour;

    return `${hour}:${minute} ${ampm}`;
  }

}
