export interface BankHolidayCalendar {
	Id?: string;
	Name: string;
	Years: BankHolidayCalendarYear[];
}

export interface BankHolidayCalendarItem extends BankHolidayCalendar {
	ActiveYearIndex: number;
}

export interface BankHolidayCalendarYear {
	Year: string;
	Dates: BankHolidayCalendarDateItem[];
	Active?: boolean;
}

export interface BankHolidayCalendarDate {
	Id?: string;
	Date: string;
	Description: string;
	IsDeleted?: boolean;
}

export interface BankHolidayCalendarDateItem extends BankHolidayCalendarDate {
	IsLastAdded?: boolean;
}

export interface SiteBankHolidayCalendars {
	Site: string;
	Calendars: string[];
}

export interface SiteBankHolidayCalendarsFormData {
	Settings: SiteBankHolidayCalendars[];
}
