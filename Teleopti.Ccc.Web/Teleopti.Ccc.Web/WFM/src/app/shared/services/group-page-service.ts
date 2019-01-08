import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GroupPage } from '../interface/group-page.interface';

@Injectable()
export class GroupPageService {
	constructor(private http: HttpClient) {}

	getAvailableGroupPagesForDate(date: string): Observable<GroupPage> {
		return this.http.get(`../api/GroupPage/AvailableGroupPages?date=${date}`) as Observable<GroupPage>;
	}
}
