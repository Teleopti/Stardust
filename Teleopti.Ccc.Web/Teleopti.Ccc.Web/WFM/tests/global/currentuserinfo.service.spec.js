(function() {
	'use strict';

	describe('currentUserInfoService', function () {
		var $httpBackend;

		beforeEach(function () {
			module('currentUserInfoService');
		});

		beforeEach(inject(function (_$httpBackend_) {
			$httpBackend = _$httpBackend_;
		}));

		it('should set the current user info', inject(function(CurrentUserInfo) {
			var data = {
				UserName: 'Ashley', DefaultTimeZone: '',
				Language: '', DateFormatLocale: '', NumberFormat: ''
			};

			CurrentUserInfo.SetCurrentUserInfo(data);

			var result = CurrentUserInfo.CurrentUserInfo();
			expect(result).not.toBe(null);
			expect(result.UserName).toBe('Ashley');
		}));

		it('should get the current user from the server', function(done) {
			inject(function (CurrentUserInfo) {
				$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, { Language: 'en', DateFormat: 'en', UserName: 'Ashley' });
				var request = CurrentUserInfo.getCurrentUserFromServer();
				
				request.success(function (result) {
					expect(result).not.toBe(null);
					expect(result.UserName).toBe('Ashley');
					done();
				});
				$httpBackend.flush();
			});
		});

		it('should return an error if the user is not logged on', function (done) {
			inject(function (CurrentUserInfo) {
				$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(401);

				var request = CurrentUserInfo.getCurrentUserFromServer();
				
				$httpBackend.flush();
				expect(request.$$state.status).toBe(2);
				done();
			});
		});

		it('should init the user context', function (done) {
			inject(function (CurrentUserInfo) {
				$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, { Language: 'en', DateFormat: 'en', UserName: 'Ashley' });

				CurrentUserInfo.initContext().then(function() {
					var result = CurrentUserInfo.isConnected();
					expect(result).toBe(true);
					done();
				});
				$httpBackend.flush();

			});
		});
		
	});
})();