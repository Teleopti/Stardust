describe('SupportEmailService', function() {
	var $httpBackend;

	beforeEach(function() {
		module('supportEmailService');
	});

	beforeEach(inject(function(_$httpBackend_) {
		$httpBackend = _$httpBackend_;
	}));

	it('should init the support email', function(done) {
		inject(function(SupportEmailService) {
			$httpBackend.expectGET('../api/Settings/SupportEmail').respond(200, 'servicedesk@teleopti.com');

			SupportEmailService.init().then(function() {
				expect(SupportEmailService.supportEmailSetting).toBe('servicedesk@teleopti.com');
				done();
			});
			$httpBackend.flush();
		});
	});

	it('should init with the default support email if nothing is provided by the server', function(done) {
		inject(function(SupportEmailService) {
			$httpBackend.expectGET('../api/Settings/SupportEmail').respond(200, '');

			SupportEmailService.init().then(function() {
				expect(SupportEmailService.supportEmailSetting).toBe('ServiceDesk@teleopti.com');
				done();
			});
			$httpBackend.flush();
		});
	});
});
