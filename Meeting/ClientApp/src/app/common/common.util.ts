import { DatePipe } from "@angular/common";

export class TimeUtil {
  public static dateFormat(time: number = Date.now(), format: string = 'yyyy-MM-dd HH:mm:ss'): string {
    const date: Date = new Date();
    date.setTime(time);

    const datePipe: DatePipe = new DatePipe('en-US');
    return datePipe.transform(date, format);
  }
}