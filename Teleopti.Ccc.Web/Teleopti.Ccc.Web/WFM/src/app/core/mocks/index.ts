import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

/**
 * Calling `dummy()` will make a GET request to `/dummy`
 */
@Injectable()
export class DummyHttpService {
	constructor(private http: HttpClient) {}

	/**
	 * Makes a request to /dummy and returns entire response
	 */
	public dummy(): Observable<any> {
		return this.http.get('/dummy', { observe: 'response' });
	}
}
