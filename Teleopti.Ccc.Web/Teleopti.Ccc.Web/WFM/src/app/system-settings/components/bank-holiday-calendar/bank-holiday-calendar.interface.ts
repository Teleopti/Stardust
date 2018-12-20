export interface BankHolidayCalendar {
	Id?: string;
	Name: string;
	Years: BankHolidayCalendarYear[];
}

export interface BankHolidayCalendarYear {
	Id?: string;
	Year: string;
	Dates: BankHolidayCalendarDate[];
}

export interface BankHolidayCalendarDate {
	Id?: string;
	Date: string;
	Description: string;
	IsDeleted?: boolean;
}
