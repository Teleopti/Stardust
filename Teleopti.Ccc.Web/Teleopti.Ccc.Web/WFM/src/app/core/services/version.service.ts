import { HttpBackend, HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';

type Version = string;

@Injectable()
export class VersionService {
	private version$ = new ReplaySubject<Version>(1);
	private http: HttpClient;

	constructor(handler: HttpBackend) {
		this.http = new HttpClient(handler);
		this.version$.next(''); // Default value
		this.http.get('../api/Global/Version').subscribe((version: Version) => {
			this.version$.next(version);
		});
	}

	getVersion(): Observable<Version> {
		return this.version$;
	}
}
