﻿'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {

	xit('should notify invalid configuration', function (t) {
		t.backend.configurationValidation.with(
			{
				Resource: 'resource'
			}
		);
		t.createController();

		expect(t.something).toEqual('resource');
	});

});
