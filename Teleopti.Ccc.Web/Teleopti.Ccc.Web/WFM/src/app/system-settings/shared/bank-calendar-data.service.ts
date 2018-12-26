import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BankHolidayCalendarListItem, BankHolidayCalendar } from '../interface';

@Injectable()
export class BankCalendarDataService {
	constructor(private http: HttpClient) {}

	getBankHolidayCalendars(): Observable<BankHolidayCalendarListItem[]> {
		return this.http.get('../api/BankHolidayCalendars') as Observable<BankHolidayCalendarListItem[]>;
	}

	saveNewBankHolidayCalendar(calendar: BankHolidayCalendar): Observable<BankHolidayCalendarListItem> {
		return this.http.post('../api/BankHolidayCalendars/Save', calendar) as Observable<BankHolidayCalendarListItem>;
	}

	saveExistingHolidayCalendar(calendar: BankHolidayCalendar): Observable<BankHolidayCalendarListItem> {
		return this.http.post('../api/BankHolidayCalendars/Save', calendar) as Observable<BankHolidayCalendarListItem>;
	}

	deleteBankHolidayCalendar(id: string): any {
		return this.http.delete('../api/BankHolidayCalendars/' + id) as Observable<BankHolidayCalendarListItem>;
	}
}
