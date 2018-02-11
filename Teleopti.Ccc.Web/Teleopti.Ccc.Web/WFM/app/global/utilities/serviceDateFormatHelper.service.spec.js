describe('#serviceDateFormatHelper#', function () {
	var serviceDateFormatHelper;
	beforeEach(module('wfm.utilities'));
	beforeEach(inject(function ($injector) {
		serviceDateFormatHelper = $injector.get('serviceDateFormatHelper');
	}));
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
		beforeAll(function () {
			moment.locale('fa-IR');
		});

		afterAll(function () {
			moment.locale('en');
		});
		commonTestsInDifferentLocale();
	});

	function commonTestsInDifferentLocale() {
		it('should get correct service date only', function () {
			var dateString = serviceDateFormatHelper.getDateOnly(new Date(2018, 1, 11));
			expect(dateString).toEqual('2018-02-11');
		});

		it('should get correct service time only', function () {
			var timeString = serviceDateFormatHelper.getTimeOnly(new Date(2018, 1, 11, 10, 30));
			expect(timeString).toEqual('10:30');
		});

		it('should get correct service date time', function () {
			var dateTimeString = serviceDateFormatHelper.getDateTime(new Date(2018, 1, 11, 10, 30));
			expect(dateTimeString).toEqual('2018-02-11 10:30');
		});

		it('should get corrent service date time with input format', function () {
			var dateTimeString = serviceDateFormatHelper.getDateByFormat(new Date(2018, 1, 11, 10, 30, 20), 'YYYY-MM-DDTHH:mm:00');
			expect(dateTimeString).toEqual('2018-02-11T10:30:00');
		});
	}
});