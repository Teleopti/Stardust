'use strict';
describe('HttpTest', function() {
	var $q, $rootScope, $httpBackend, $http, httpInterceptor, NoticeService, SupportEmailService;

	beforeEach(function() {
		module('wfm.http');
		module(function($provide) {
			$provide.service('NoticeService', function() {
				return {
					error: function() {}
				};
			});
		});
	});

	beforeEach(inject(function(
		_$httpBackend_,
		_$q_,
		_$rootScope_,
		_$http_,
		_httpInterceptor_,
		_NoticeService_,
		_SupportEmailService_
	) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$http = _$http_;
		httpInterceptor = _httpInterceptor_;
		NoticeService = _NoticeService_;
		SupportEmailService = _SupportEmailService_;
	}));

	function reactHttp(statusCode, done) {
		var scope = $rootScope.$new();
		$httpBackend.expectGET('../api/Settings/SupportEmail').respond(200, 'mock');
		spyOn(NoticeService, 'error');
		var rejection = {
			status: statusCode
		};
		SupportEmailService.init();
		var response = httpInterceptor.responseError(rejection);
		var successCallback = function() {};
		var errorCallback = function() {
			expect(NoticeService.error).toHaveBeenCalled();
			done();
		};

		response.then(successCallback, errorCallback);
		scope.$digest();

		$httpBackend.flush();
	}

	it('Should react to http error 400', function(done) {
		reactHttp(400, done);
	});

	it('Should react to http error 500', function(done) {
		reactHttp(500, done);
	});

	it('Should react to all 404-600 http errors', function(done) {
		reactHttp(419, done);
	});

	it('Should react to http error 409', function(done) {
		reactHttp(409, done);
	});

	xit('Should reload on 418', function(done) {});

	xit('Should ignore on 422', function(done) {});

	it('Should react to http error -1', function(done) {
		reactHttp(-1, done);
	});
});
