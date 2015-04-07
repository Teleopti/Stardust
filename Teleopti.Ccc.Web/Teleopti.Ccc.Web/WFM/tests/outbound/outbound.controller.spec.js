﻿'use strict';

describe('OutboundListCtrl', function () {
	var $q,
	    $rootScope,
	    $httpBackend;

	beforeEach(module('wfm'));
	beforeEach(module('outboundServiceMock'));

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
	}));

	it('creates a campaign in the list', inject(function ($controller, outboundServiceMock) {
		var scope = $rootScope.$new();

		$controller('OutboundListCtrl', { $scope: scope, OutboundService: outboundServiceMock });

		scope.create();

		expect(scope.campaigns.length).toEqual(8);
	}));
});
