(function () {
	'use strict';

	describe('currentUserInfoService', function () {
		var $httpBackend, $rootScope;

		beforeEach(function () {
			module('currentUserInfoService');
		});

		beforeEach(inject(function (_$httpBackend_, _$rootScope_) {
			$httpBackend = _$httpBackend_;
			$rootScope = _$rootScope_;
			$rootScope.setTheme = function () { };
		}));

		it('should set the current user info', inject(function (CurrentUserInfo) {
			var data = {
				UserName: 'Ashley', DefaultTimeZone: '',
				Language: '', DateFormatLocale: '', NumberFormat: ''
			};

			CurrentUserInfo.SetCurrentUserInfo(data);

			var result = CurrentUserInfo.CurrentUserInfo();
			expect(result).not.toBe(null);
			expect(result.UserName).toBe('Ashley');
		}));

		it('should get the current user from the server', function (done) {
			inject(function (CurrentUserInfo) {
				$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
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
				$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
				$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(401);

				var request = CurrentUserInfo.getCurrentUserFromServer();

				$httpBackend.flush();
				expect(request.$$state.status).toBe(2);
				done();
			});
		});

		it('should init the user context with toggles', function (done) {
			inject(function (CurrentUserInfo) {
				$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, { 'WfmGlobalLayout_personalOptions_37114': true });
				$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, { Language: 'en', DateFormat: 'en', UserName: 'Ashley' });
				$httpBackend.expectGET("../api/BusinessUnit").respond(200, ['mock']);
				$httpBackend.expectGET("../api/Settings/SupportEmail").respond(200, '');
				$httpBackend.expectGET("../api/Theme").respond(200, { Name: 'light' });

				CurrentUserInfo.initContext().then(function () {
					var result = CurrentUserInfo.isConnected();
					expect(result).toBe(true);
					done();
				});
				$httpBackend.flush();
			});
		});

		it('should init the user context without theme toggle', function (done) {
			inject(function (CurrentUserInfo) {
				$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, { 'WfmGlobalLayout_personalOptions_37114': false });
				$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, { Language: 'en', DateFormat: 'en', UserName: 'Ashley' });
				$httpBackend.expectGET("../api/BusinessUnit").respond(200, ['mock']);
				$httpBackend.expectGET("../api/Settings/SupportEmail").respond(200, '');
				$httpBackend.expectGET("../api/Theme").respond(200, '');

				CurrentUserInfo.initContext().then(function () {
					var result = CurrentUserInfo.isConnected();
					expect(result).toBe(true);
					done();
				});
				$httpBackend.flush();
			});
		});

		it('should init the support email', function (done) {
			inject(function (Settings) {
				$httpBackend.expectGET("../api/Settings/SupportEmail").respond(200, 'servicedesk@teleopti.com');

				Settings.init().then(function () {
					expect(Settings.supportEmailSetting).toBe('servicedesk@teleopti.com');
					done();
				});
				$httpBackend.flush();
			});
		});

		it('should init with the default support email if nothing is provided by the server', function (done) {
			inject(function (Settings) {
				$httpBackend.expectGET("../api/Settings/SupportEmail").respond(200, '');

				Settings.init().then(function () {
					expect(Settings.supportEmailSetting).toBe('ServiceDesk@teleopti.com');
					done();
				});
				$httpBackend.flush();
			});
		});

	});
})();
