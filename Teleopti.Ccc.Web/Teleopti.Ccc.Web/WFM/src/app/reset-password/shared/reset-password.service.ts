import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of, throwError } from 'rxjs';
import { flatMap } from 'rxjs/operators';

export interface ValidateTokenRequest {
	ResetToken: string;
}

export interface ResetRequest {
	ResetToken: string;
	NewPassword: string;
}

export interface GenericResponse {
	success: boolean;
	result: any[];
	errors: string[];
}

@Injectable({
	providedIn: 'root'
})
export class ResetPasswordService {
	constructor(private http: HttpClient) {}

	isValidToken(token: string) {
		const body: ValidateTokenRequest = {
			ResetToken: token
		};
		return this.http.post('../ChangePassword/ValidateToken', body).pipe(
			flatMap((res: GenericResponse) => {
				if (res.success === true) return of(true);
				else if (res.success === false) return of(false);
			})
		);
	}

	reset(body: ResetRequest) {
		return this.http.post('../ChangePassword/RequestReset', body).pipe(
			flatMap((res: GenericResponse) => {
				if (res.success === true) return of(true);
				else if (res.success === false) return throwError(false);
			})
		);
	}

	resetPassword(body: ResetRequest) {
		return this.http.post('../ChangePassword/Reset', body).pipe(
			flatMap((res: GenericResponse) => {
				if (res.success === true) return of(true);
				else if (res.success === false) return throwError(false);
			})
		);
	}
}
