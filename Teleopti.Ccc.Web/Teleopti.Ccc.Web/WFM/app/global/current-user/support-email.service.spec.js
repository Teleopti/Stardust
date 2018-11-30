describe('SupportEmailService', function() {
	var $httpBackend;

	beforeEach(function() {
		module('currentUserInfoService');
	});

	beforeEach(inject(function(_$httpBackend_) {
		$httpBackend = _$httpBackend_;
	}));

	it('should init the support email', function(done) {
		inject(function(Settings) {
			$httpBackend.expectGET('../api/Settings/SupportEmail').respond(200, 'servicedesk@teleopti.com');

			Settings.init().then(function() {
				expect(Settings.supportEmailSetting).toBe('servicedesk@teleopti.com');
				done();
			});
			$httpBackend.flush();
		});
	});

	it('should init with the default support email if nothing is provided by the server', function(done) {
		inject(function(Settings) {
			$httpBackend.expectGET('../api/Settings/SupportEmail').respond(200, '');

			Settings.init().then(function() {
				expect(Settings.supportEmailSetting).toBe('ServiceDesk@teleopti.com');
				done();
			});
			$httpBackend.flush();
		});
	});
});
