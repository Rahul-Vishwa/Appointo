export interface GetAnalyticsToday {
    appointments: number;
    upcomingAppointments: number;
    cancellations: number;
    openSlotsLeft: number;
}

export interface GetAppointmentCount {
    count: number;
    date: string;
}

export interface GetPercentageAnalytics {
    today: string;
    month: string;
}