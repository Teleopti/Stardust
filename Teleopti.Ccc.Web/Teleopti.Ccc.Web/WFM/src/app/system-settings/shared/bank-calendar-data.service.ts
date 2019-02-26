import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { BankHolidayCalendar, SiteBankHolidayCalendars, SiteBankHolidayCalendarsFormData } from '../interface';

@Injectable()
export class BankCalendarDataService {
	public yearFormat = 'YYYY';
	public dateFormat = 'YYYY-MM-DD';

	public bankHolidayCalendarsList$: BehaviorSubject<BankHolidayCalendar[]> = new BehaviorSubject<
		BankHolidayCalendar[]
	>([]);

	constructor(private http: HttpClient) {
		this.getBankHolidayCalendars().subscribe(calendars => {
			const cals = calendars as BankHolidayCalendar[];
			const curYear = moment().year();
			cals.forEach(c => {
				// c.CurrentYearIndex = 0;

				c.Years.forEach((y, i) => {
					y.Dates.forEach(d => {
						d.Date = moment(d.Date).format(this.dateFormat);
					});

					if (moment(y.Year.toString(), 'YYYY').year() === curYear) {
						// c.CurrentYearIndex = i;
					}
				});
			});

			this.bankHolidayCalendarsList$.next(
				cals.sort((c, n) => {
					return c.Name.localeCompare(n.Name);
				})
			);
		});
	}

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

	getSitesByCalendar(calendarId: string): Observable<string[]> {
		return this.http.get(`../api/SitesByCalendar/${calendarId}`) as Observable<string[]>;
	}

	getSiteBankHolidayCalendars(): Observable<SiteBankHolidayCalendars[]> {
		return this.http.get('../api/SiteBankHolidayCalendars') as Observable<SiteBankHolidayCalendars[]>;
	}

	updateCalendarForSite(data: SiteBankHolidayCalendarsFormData): Observable<boolean> {
		return this.http.post('../api/SiteBankHolidayCalendars/Update', data) as Observable<boolean>;
	}
}
