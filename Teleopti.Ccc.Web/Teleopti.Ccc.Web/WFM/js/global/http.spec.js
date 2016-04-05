'use strict';
describe('HttpTest', function() {
	var $q,
		$rootScope,
		$httpBackend,
		$http,
		httpInterceptor,
		NoticeService;

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
	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_, _$http_, _httpInterceptor_, _NoticeService_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$http = _$http_;
		httpInterceptor = _httpInterceptor_;
		NoticeService = _NoticeService_;
	}));

	it('Should react to http error 500', function(done) {
		var scope = $rootScope.$new();
		spyOn(NoticeService, 'error');
		var rejection = {
			status: 500
		};
		var response = httpInterceptor.responseError(rejection);
		var successCallback = function() {};
		var errorCallback = function(resolved) {
			expect(NoticeService.error).toHaveBeenCalled();
			done();
		};

		response.then(successCallback, errorCallback);
		scope.$digest();
	});

	it('Should react to all 4.XX http errors', function(done) {
		var scope = $rootScope.$new();
		spyOn(NoticeService, 'error');
		var rejection = {
			status: 418
		};
		var response = httpInterceptor.responseError(rejection);
		var successCallback = function() {};
		var errorCallback = function(resolved) {
			expect(NoticeService.error).toHaveBeenCalled();
			done();
		};

		response.then(successCallback, errorCallback);
		scope.$digest();
	});

	it('Should react to http error 0', function(done) {
		var scope = $rootScope.$new();
		spyOn(NoticeService, 'error');
		var rejection = {
			status: 0
		};
		var response = httpInterceptor.responseError(rejection);
		var successCallback = function() {};
		var errorCallback = function(resolved) {
			expect(NoticeService.error).toHaveBeenCalled();
			done();
		};

		response.then(successCallback, errorCallback);
		scope.$digest();
	});


});
