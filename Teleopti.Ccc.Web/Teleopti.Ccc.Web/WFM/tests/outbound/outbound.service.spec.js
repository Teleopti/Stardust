'use strict';

describe("OutboundService", function () {
	var
		 $httpBackend;

	beforeEach(module('wfm'));

	beforeEach(inject(function (_$httpBackend_) {


		$httpBackend = _$httpBackend_;

		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');

		$httpBackend.expectGET("html/forecasting/forecasting.html").respond(200, 'mock');
	}));

	it("should be able to list campaigns", inject(function (OutboundService) {
		$httpBackend.whenGET('../api/Outbound/Campaign').respond([{ id: 1, name: "camp1" }, { id: 2, name: "camp2" }]);

		var campaigns = OutboundService.listCampaign();
		$httpBackend.flush();

		expect(campaigns.length).toEqual(2);
		expect(campaigns[0].name).toEqual("camp1");
	}));
});