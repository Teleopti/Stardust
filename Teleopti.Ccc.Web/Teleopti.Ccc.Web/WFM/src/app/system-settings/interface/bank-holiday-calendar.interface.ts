export interface BankHolidayCalendar {
	Id?: string;
	Name: string;
	Years: BankHolidayCalendarYear[];
}

export interface BankHolidayCalendarYear {
	Year: string;
	Dates: BankHolidayCalendarDateItem[];
}

export interface BankHolidayCalendarYearItem extends BankHolidayCalendarYear {
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
