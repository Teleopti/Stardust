import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BankHolidayCalendar, SiteBankHolidayCalendars, UpdateSiteBankHolidayCalendarsFormData } from '../interface';

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

	getSiteBankHolidayCalendars(): Observable<SiteBankHolidayCalendars[]> {
		return this.http.get('../api/SiteBankHolidayCalendars') as Observable<SiteBankHolidayCalendars[]>;
	}

	updateCalendarForSite(data: UpdateSiteBankHolidayCalendarsFormData): Observable<boolean> {
		return this.http.post('../api/SiteBankHolidayCalendars/Update', data) as Observable<boolean>;
	}
}
