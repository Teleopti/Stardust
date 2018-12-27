import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface ChangePasswordRequest {
	OldPassword: string;
	NewPassword: string;
}

export interface ChangePasswordResultInfo {
	IsSuccessful: boolean;
	IsAuthenticationSuccessful: boolean;
	ErrorCode: string;
}

@Injectable()
export class PasswordService {
	constructor(private http: HttpClient) {}

	public setPassword(passwordRequest: ChangePasswordRequest): Observable<ChangePasswordResultInfo> {
		return this.http.post('../Settings/ChangePassword', passwordRequest).pipe(
			map(response => {
				return response;
			})
		) as Observable<ChangePasswordResultInfo>;
	}
}
