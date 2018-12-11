import { of } from 'rxjs';
import { GenericResponse, ResetPasswordService, ResetRequest } from './reset-password.service';

describe('ResetPasswordService', () => {
	let httpClientSpy: { post: jasmine.Spy };
	let resetPasswordService: ResetPasswordService;

	beforeEach(() => {
		httpClientSpy = jasmine.createSpyObj('HttpClient', ['post']);
		resetPasswordService = new ResetPasswordService(<any>httpClientSpy);
	});

	it('should post token', () => {
		let response: Partial<GenericResponse> = { success: true };
		httpClientSpy.post.and.returnValue(of(response));
		resetPasswordService.isValidToken('thisisatokeniguess').subscribe(response => {
			expect(response).toBe(true);
		});
		expect(httpClientSpy.post.calls.count()).toEqual(1);
	});

	it('should post a reset', () => {
		let response: Partial<GenericResponse> = { success: true };
		httpClientSpy.post.and.returnValue(of(response));

		const resetRequest: ResetRequest = {
			NewPassword: 'password',
			ResetToken: 'token'
		};

		resetPasswordService.reset(resetRequest).subscribe(response => {
			expect(response).toBe(true);
		});

		expect(httpClientSpy.post.calls.count()).toEqual(1);
	});
});
