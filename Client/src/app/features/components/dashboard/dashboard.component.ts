import { ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { CardComponent } from "../../../shared/components/card/card.component";
import { DashboardService } from '../../services/dashboard/dashboard.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { ChartConfiguration } from 'chart.js';
import { ChartDirective } from '../../../shared/directives/chart.directive';
import { GetAppointmentCount } from '../../models/dashboard';
import { forkJoin, Subscription } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CardComponent, ChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class DashboardComponent implements OnInit, OnDestroy {
  private subscriptions = new Subscription();
  private dashboard = inject(DashboardService);
  analyticsToday = toSignal(this.dashboard.getAnalyticsToday(), {initialValue: null});
  percentageAnalytics = toSignal(this.dashboard.getPercentageAnalytics(), {initialValue: null});

  private chartConfig: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Appointments',
        backgroundColor: '#6366f1',
        borderColor: '#6366f1',
        borderWidth: 1,
        hoverBackgroundColor: '#5456da',
        hoverBorderColor: '#5456da'
      }
    ]
  };

  public past7Days: ChartConfiguration<'bar'>['data'] = this.chartConfig;
  public past7DaysCancelled: ChartConfiguration<'bar'>['data'] = this.chartConfig;
  public thisMonthCancelled: ChartConfiguration<'bar'>['data'] = this.chartConfig;
  public thisMonth: ChartConfiguration<'bar'>['data'] = this.chartConfig;

  public chartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    scales: {
      x: {
        title: {
          display: true,
          text: 'Date'
        }
      },
      y: {
        title: {
          display: true,
          text: 'Appointments'
        },
        beginAtZero: true
      }
    }
  };

  constructor(){}

  ngOnInit(): void {
    this.getAppointmentCountPast7Days();
    this.getChartData();
  }

  private getChartData(){
    const apis = {
      past7Days: this.getAppointmentCountPast7Days(),
      thisMonth: this.getAppointmentCountThisMonth(),
      past7DaysCancelled: this.getcancelledAppointmentCountPast7Days(),
      thisMonthCancelled: this.getCancelledAppointmentCountThisMonth(),
    };

    this.subscriptions.add(
      forkJoin(apis)
        .subscribe(data=>{
          this.past7Days = this.updateChartData(data.past7Days, 'past7Days');
          this.thisMonth = this.updateChartData(data.thisMonth, 'thisMonth');
          this.past7DaysCancelled = this.updateChartData(data.past7DaysCancelled, 'past7DaysCancelled');
          this.thisMonthCancelled = this.updateChartData(data.thisMonthCancelled, 'thisMonthCancelled');
        })
    );
  }

  private getAppointmentCountPast7Days(){
    return this.dashboard.getAppointmentCountPast7Days();
  }
  
  private getAppointmentCountThisMonth(){
    return this.dashboard.getAppointmentCountThisMonth();
  }
  
  private getcancelledAppointmentCountPast7Days(){
    return this.dashboard.getcancelledAppointmentCountPast7Days();
  }

  private getCancelledAppointmentCountThisMonth(){
    return this.dashboard.getCancelledAppointmentCountThisMonth();
  }

  private updateChartData(array: Array<GetAppointmentCount>, type: 'past7Days' | 'past7DaysCancelled' | 'thisMonth' | 'thisMonthCancelled') {
    const labels = array.map((item) => item.date) || [];
    const data = array.map((item) => item.count) || [];

    return {
      labels: labels,
      datasets: [
        {
          data: data,
          label: 'Appointments',
          backgroundColor: '#6366f1',
          borderColor: '#6366f1',
          borderWidth: 1,
          hoverBackgroundColor: '#5456da',
          hoverBorderColor: '#5456da'
        }
      ]
    };
  }

  getPercentageClass(type: 'today' | 'month'){
    let percentage;
    if (type === 'today'){
      percentage = this.percentageAnalytics()?.today?.split(' ');
    }
    else{
      percentage = this.percentageAnalytics()?.month?.split(' ');
    }
    if (percentage){
      return percentage[1] === 'Increase' ? 'text-green-500' : 'text-red-500';
    }
    return '';
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
