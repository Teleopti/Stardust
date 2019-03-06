import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import {
	BankHolidayCalendar,
	SiteBankHolidayCalendars,
	SiteBankHolidayCalendarsFormData,
	BankHolidayCalendarItem
} from '../interface';

@Injectable()
export class BankCalendarDataService {
	public yearFormat = 'YYYY';
	public dateFormat = 'YYYY-MM-DD';

	public bankHolidayCalendarsList$: BehaviorSubject<BankHolidayCalendarItem[]> = new BehaviorSubject<
		BankHolidayCalendarItem[]
	>([]);

	constructor(private http: HttpClient) {
		this.getBankHolidayCalendars().subscribe(cals => {
			const calendars = cals as BankHolidayCalendarItem[];
			calendars.forEach(calendar => {
				this.resetActiveYearIndexAndYearFormatOfCalendar(calendar);
			});

			this.bankHolidayCalendarsList$.next(
				calendars.sort((c, n) => {
					return c.Name.localeCompare(n.Name);
				})
			);
		});
	}

	resetActiveYearIndexAndYearFormatOfCalendar(calendar: BankHolidayCalendarItem) {
		const curYear = moment().year();
		calendar.ActiveYearIndex = 0;
		calendar.Years.forEach((y, i) => {
			y.Year = y.Year.toString();
			y.Dates.forEach(d => {
				d.Date = moment(d.Date).format(this.dateFormat);
			});

			if (moment(y.Year, this.yearFormat).year() === curYear) {
				calendar.ActiveYearIndex = i;
			}
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
