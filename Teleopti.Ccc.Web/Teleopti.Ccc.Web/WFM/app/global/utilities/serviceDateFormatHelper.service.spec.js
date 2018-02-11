describe('<serviceDateFormatHelper>', function () {
	var serviceDateFormatHelper;
	beforeEach(module('wfm.utilities'));
	beforeEach(inject(function ($injector) {
		serviceDateFormatHelper = $injector.get('serviceDateFormatHelper');
	}));


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
});

describe('#serviceDateFormatHelper for ar-AE#', function () {
	var serviceDateFormatHelper;
	beforeEach(module('wfm.utilities'));
	beforeEach(inject(function ($injector) {
		serviceDateFormatHelper = $injector.get('serviceDateFormatHelper');
	}));

	beforeEach(function () {
		moment.locale('ar-AE');
	});

	afterEach(function () {
		moment.locale('en');
	});

	it('should get correct service date string', function () {
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
});

describe('#serviceDateFormatHelper for Persian#', function () {
	var serviceDateFormatHelper;
	beforeEach(module('wfm.utilities'));
	beforeEach(inject(function ($injector) {
		serviceDateFormatHelper = $injector.get('serviceDateFormatHelper');
	}));

	beforeEach(function () {
		moment.locale('fa-IR');
	});

	afterEach(function () {
		moment.locale('en');
	});

	it('should get correct service date string', function () {
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
});