'use strict';

// ignore until 33699 is finished

xdescribe("OutboundService", function () {
	var
		 $httpBackend;

	beforeEach(module('wfm'));

	beforeEach(inject(function (_$httpBackend_) {
		$httpBackend = _$httpBackend_;

		$httpBackend.whenGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.whenGET("../api/Global/User/CurrentUser").respond(200, 'mock');

		$httpBackend.whenGET("html/forecasting/forecasting.html").respond(200, 'mock');
	}));

	it("should be able to list campaigns", inject(function (OutboundService) {
		$httpBackend.whenGET('../api/Outbound/Campaign').respond([{ id: 1, name: "camp1" }, { id: 2, name: "camp2" }]);

		var campaigns = OutboundService.listCampaign();
		$httpBackend.flush();

		expect(campaigns.length).toEqual(2);
		expect(campaigns[0].name).toEqual("camp1");
	}));
});