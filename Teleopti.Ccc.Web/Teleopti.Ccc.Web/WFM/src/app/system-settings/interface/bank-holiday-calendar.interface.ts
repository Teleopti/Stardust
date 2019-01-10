export interface BankHolidayCalendar {
	Id?: string;
	Name: string;
	Years: BankHolidayCalendarYear[];
}

export interface BankHolidayCalendarItem extends BankHolidayCalendar {
	CurrentYearIndex: number;
}

export interface BankHolidayCalendarYear {
	Year: string;
	Dates: BankHolidayCalendarDateItem[];
}

export interface BankHolidayCalendarYearItem extends BankHolidayCalendarYear {
	ModifiedDates?: BankHolidayCalendarDateItem[];
	YearDate: Date;
	DisabledDate?: Function;
	SelectedDates: number[];
	Active: boolean;
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
	Calendars: SiteBankHolidayCalendar[];
}

export interface SiteBankHolidayCalendar {
	Id: string;
}

export interface SiteBankHolidayCalendarsFormData {
	Site: string;
	Calendars: string[];
}

export interface UpdateSiteBankHolidayCalendarsFormData {
	Settings: SiteBankHolidayCalendarsFormData[];
}
