import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BankHolidayCalendar } from '../interface';

@Injectable()
export class BankCalendarDataService {
	constructor(private http: HttpClient) {}

	getBankHolidayCalendars(): Observable<BankHolidayCalendar[]> {
		return this.http.get('../api/BankHolidayCalendars') as Observable<BankHolidayCalendar[]>;
	}

	saveNewBankHolidayCalendar(calendar: BankHolidayCalendar): Observable<BankHolidayCalendar> {
		return this.http.post('../api/BankHolidayCalendars/Save', calendar) as Observable<BankHolidayCalendar>;
	}

	saveExistingHolidayCalendar(calendar: BankHolidayCalendar): Observable<BankHolidayCalendar> {
		return this.http.post('../api/BankHolidayCalendars/Save', calendar) as Observable<BankHolidayCalendar>;
	}

	deleteBankHolidayCalendar(id: string): any {
		return this.http.delete('../api/BankHolidayCalendars/' + id) as Observable<BankHolidayCalendar>;
	}
}
