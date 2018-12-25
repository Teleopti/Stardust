import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BankHolidayCalendar } from '../interface';
import { Observable, of } from 'rxjs';

@Injectable()
export class BankCalendarDataService {
	fakeResult: BankHolidayCalendar[] = [];

	constructor(private http: HttpClient) {}

	getBankHolidayCalendars(): Observable<BankHolidayCalendar[]> {
		return of(this.fakeResult);
	}

	saveNewHolidayCalendar(calendar: BankHolidayCalendar): Observable<BankHolidayCalendar> {
		//return this.http.post('../api/BankHolidayCalendar/Create', calendar) as Observable<BankHolidayCalendar>;
		calendar.Id == 'e6c0ac98-93ee-4cb5-bf32-e890c084135b';
		this.fakeResult.push(calendar);

		return of(calendar);
	}

	saveExistingHolidayCalendar(calendar: BankHolidayCalendar): Observable<BankHolidayCalendar> {
		//return this.http.post('../api/BankHolidayCalendar/Create', calendar) as Observable<BankHolidayCalendar>;
		this.fakeResult.forEach(f => {
			if (f.Id == calendar.Id) {
				f = calendar;
			}
		});

		return of(calendar);
	}
}
