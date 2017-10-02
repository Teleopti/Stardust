'use strict';
describe('rtaFormatService', function () {
	var target;

	beforeEach(module('wfm.rta'));
	beforeEach(module('wfm.rtaTestShared'));
	
	beforeEach(inject(function (rtaFormatService) {
		target = rtaFormatService;
	}));

	afterEach(function () {
		jasmine.clock().uninstall();
	});

	it('should format timestamp for today', function () {
		var today = new Date('2015-04-17');
		jasmine.clock().mockDate(today);

		expect(target.formatDateTime("\/Date(1429254905000)\/")).toEqual("07:15");
	});

	it('should format timestamp for tomorrow', function () {
		var today = new Date('2015-05-19');
		jasmine.clock().mockDate(today);

		expect(target.formatDateTime("\/Date(1432109700000)\/")).toEqual("2015-05-20 08:15:00");
		expect(target.formatDateTime("\/Date(1432105910000)\/")).toEqual("2015-05-20 07:11:50");
	});

	it('should format duration', function () {
		expect(target.formatDuration(15473)).toEqual("4:17:53");
	});

	it('should format hex color', function () {
		expect(target.formatHexToRgb("#00FF00")).toEqual("rgba(0, 255, 0, 0.6)");
	});

	it('should not format an empty string', function () {
		expect(target.formatDateTime("")).toEqual("");
		expect(target.formatDateTime(null)).toEqual("");
		expect(target.formatDateTime(undefined)).toEqual("");
	});
});
