'use strict';

describe("Timezone filter tests", function () {

	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: "Asia/Hong_Kong" };
		}
	};

	var target;

	beforeEach(function () {
		module("wfm.teamSchedule");
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function ($filter) {
		target = $filter('timezone');
	}));

	it("Should return timezone adjusted time for input time string", function () {

		var input = '08:00';
		var result = target(input, 'Europe/Stockholm');
		var isDST = moment().tz('Europe/Stockholm').isDST();
		if (isDST || moment().format('YYYY-MM-DD') === '2017-10-29') {
			expect(result).toEqual('02:00');
		} else {
			expect(result).toEqual('01:00');
		}
	});

	it("Should return timezone adjusted time for input time string in 24 hours format", function () {

		var input = '05:00';
		var result = target(input, 'Europe/Stockholm');
		var isDST = moment().tz('Europe/Stockholm').isDST();
		if (isDST || moment().format('YYYY-MM-DD') === '2017-10-29') {
			expect(result).toEqual('23:00');
		} else {
			expect(result).toEqual('22:00');
		}
	});

	it("Should return timezone adjusted time for input date time string", function () {

		var input = '2016-10-07T08:00';
		var result = target(input, 'Europe/Stockholm');
		expect(result).toEqual('2016-10-07T02:00');
	});

	it("Should return timezone adjusted time for input date time string in 24 hours format", function () {

		var input = '2016-10-07T05:00';
		var result = target(input, 'Europe/Stockholm');
		expect(result).toEqual('2016-10-06T23:00');
	});

});

describe("#serviceTimezone filter#", function () {
	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: "Asia/Hong_Kong" };
		}
	};

	var target;

	beforeEach(function () {
		module("wfm.teamSchedule");
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function ($filter) {
		target = $filter('serviceTimezone');
	}));

	function commonTestsInDifferentLocale() {
		it("Should return timezone adjusted time for input time string", function () {

			var input = '08:00';
			var result = target(input, 'Europe/Stockholm');
			var isDST = moment().tz('Europe/Stockholm').isDST();
			if (isDST || moment().format('YYYY-MM-DD') === '2017-10-29') {
				expect(result).toEqual('02:00');
			} else {
				expect(result).toEqual('01:00');
			}
		});

		it("Should return timezone adjusted time for input time string in 24 hours format", function () {

			var input = '05:00';
			var result = target(input, 'Europe/Stockholm');
			var isDST = moment().tz('Europe/Stockholm').isDST();
			if (isDST || moment().format('YYYY-MM-DD') === '2017-10-29') {
				expect(result).toEqual('23:00');
			} else {
				expect(result).toEqual('22:00');
			}
		});

		it("Should return timezone adjusted time for input date time string", function () {

			var input = '2016-10-07T08:00';
			var result = target(input, 'Europe/Stockholm');
			expect(result).toEqual('2016-10-07T02:00');
		});

		it("Should return timezone adjusted time for input date time string in 24 hours format", function () {

			var input = '2016-10-07T05:00';
			var result = target(input, 'Europe/Stockholm');
			expect(result).toEqual('2016-10-06T23:00');
		});
	}

	commonTestsInDifferentLocale();

	describe('in locale ar-AE', function () {
		beforeAll(function () {
			moment.locale('ar-AE');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	describe('in locale fa-IR', function () {
		beforeEach(function () {
			moment.locale('fa-IR');
		});

		afterEach(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});
});