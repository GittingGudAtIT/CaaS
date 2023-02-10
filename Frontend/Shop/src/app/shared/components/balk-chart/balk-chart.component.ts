import { Component, Input } from '@angular/core';
import { WeekdayDistribution } from '../../dtos/weekday-distribution';

@Component({
  selector: 'caas-balk-chart',
  templateUrl: './balk-chart.component.html',
  styleUrls: [
    './balk-chart.component.css'
  ],
  styles: [
  ]
})
export class BalkChartComponent {
  @Input() weekdayDist = new WeekdayDistribution<number>();

  private max() : number{
    return Math.max(
      this.weekdayDist.sunday,
      this.weekdayDist.monday,
      this.weekdayDist.tuesday,
      this.weekdayDist.wednesday,
      this.weekdayDist.thursday,
      this.weekdayDist.friday,
      this.weekdayDist.saturday
    );
  }

  percentageValueOf(dayValue: number) : number {
    const max = this.lineValue() * 5;
    if(max === 0)
      return 0;
    return dayValue / max;
  }

  fixed(value: number, digits: number = 2) : string{
    return value.toFixed(digits);
  }

  lineValue(): number{
    var i = Math.round(this.max());
    var s = i.toFixed(0);
    var start = '';
    var end = '';
    for(var x = 0; x < s.length - 2; ++x)
      end += '0';

    var cnt = 0;
    const max = 2;
    while(s.length > 2 && cnt < max){

      const c0 = s.charAt(0);
      const c1 = s.charAt(1);

      cnt += 1;
      if(c1 === '9' || cnt === max)
        return Number(`${start}${Number(c0) + 1}0${end}`) / 5;
      else if(c1 >= '7')
        return Number(`${start}${c0}${Number(c1) + 1}${end}`) / 5;

      start = `${c0}${start}`;
      end = end.slice(0, end.length - 1);
      s = s.slice(1, s.length);
    }
    if(i <= 2)
      return 0.5;

    if(i <= 3)
      return 0.6;

    return i / 5;
  }
}
