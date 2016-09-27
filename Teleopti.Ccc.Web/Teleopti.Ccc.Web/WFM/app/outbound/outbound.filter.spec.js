'use strict';

describe('OutboundFilters', function() {

	beforeEach(module('wfm.outbound'));

	it('timespan filter should return correct timespan string', inject(function($filter) {

		var filter = $filter('showTimespan');
		var input1 = 36.5;
		var input2 = 0.1;

		var output1 = filter(input1);
		var output2 = filter(input2);

		expect(output1).toEqual('36 : 30');
		expect(output2).toEqual('0 : 06');
	}));


	it('timespan filter should display null timespan as dash', inject(function($filter) {
		var filter = $filter('showTimespan');
		expect(filter(null)).toEqual(' -- : -- ');
	}));

	it('weekday filter should return correct weekday string', inject(function($filter) {

		var filter = $filter('showWeekdays');

		var input1 = { WeekDay: 0 };
		var input2 = { WeekDay: 6 };

		var output1 = filter(input1);
		var output2 = filter(input2);

		expect(output1).toEqual('Sun');
		expect(output2).toEqual('Sat');


	}));

});