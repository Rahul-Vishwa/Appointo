import { Directive, ElementRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

Chart.register(...registerables);

@Directive({
  selector: '[appChart]',
  standalone: true
})
export class ChartDirective implements OnChanges {
  @Input() data: any;
  @Input() type: string = 'bar';
  @Input() options: any = {};

  private chart: Chart | null = null;

  constructor(private el: ElementRef) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['data'] || changes['type'] || changes['options']) {
      this.updateChart();
    }
  }

  private updateChart() {
    if (this.chart) {
      this.chart.destroy();
    }

    const config: ChartConfiguration = {
      type: this.type as any,
      data: this.data,
      options: {
        responsive: true,
        ...this.options
      }
    };

    this.chart = new Chart(this.el.nativeElement, config);
  }
} 