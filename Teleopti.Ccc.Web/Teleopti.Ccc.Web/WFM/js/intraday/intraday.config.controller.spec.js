'use strict';
describe('IntradayConfigCtrl', function () {
	var $httpBackend,
		$controller,
		$translate,
		scope;

	var skills = [];
	var appInsights;

	beforeEach(module('wfm.intraday'));

	beforeEach(function () {
	    appInsights = {
	               trackPageView:function() {
	        	            return;
	        	        }
	       }
		skills = [
		{
			Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
			Name: "my skill"
		}];
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$rootScope_, _$translate_) {
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		scope = _$rootScope_.$new();
		$translate = _$translate_;
	    appInsights: appInsights

		$httpBackend.whenGET("../api/intraday/skills")
			.respond(200, skills);
	}));

	var createController = function () {
		$controller('IntradayConfigCtrl', {
			$scope: scope,
			$translate: $translate
		});
		scope.$digest();
		$httpBackend.flush();
	}

	it('should display list of skills', function () {
		createController();

		expect(scope.skills[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(scope.skills[0].Name).toEqual("my skill");
	});
});
