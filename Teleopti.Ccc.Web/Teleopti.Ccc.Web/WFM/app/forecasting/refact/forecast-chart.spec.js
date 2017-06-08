'use strict';

describe('component: forecastChart', function() {
	var $componentController,
	ctrl;

	beforeEach(function() {
		module('wfm.forecasting');
	});

	beforeEach(inject(function( _$componentController_) {
		$componentController = _$componentController_;

		// var days = [
		// 	{
		// 		date:"2017-07-01T00:00:00",
		// 		vacw: 10,
		// 		vc: 10,
		// 		vtacw: 10,
		// 		vtc: 10,
		// 		vtt: 10,
		// 		vttt: 10
		// 	},
		// 	{
		// 		date:"2017-07-02T00:00:00",
		// 		vacw: 20,
		// 		vc: 20,
		// 		vtacw: 20,
		// 		vtc: 20,
		// 		vtt: 20,
		// 		vttt: 20
		// 	}
		// ];
		//
		// var chartId = "chartb8a74a6c-3125-4c13-a19a-9f0800e35a1f";

	}));

	// it('should', function() {
	// 	ctrl = $componentController('forecastChart', null,{});
	// 	expect(true).toEqual(false);
	// });

});
