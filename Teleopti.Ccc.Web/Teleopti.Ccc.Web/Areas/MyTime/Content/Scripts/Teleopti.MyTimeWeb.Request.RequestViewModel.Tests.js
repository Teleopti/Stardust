$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Request.RequestViewModel");

	test("should read personal account permissions", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readPersonalAccountPermission(true);

		equal(vm.PersonalAccountPermission(), true);
	});
	
	test("should read account values in days", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readAbsenceAccount({
			PeriodStart: moment("2015-01-01"),
			PeriodEnd: moment("2015-12-31"),
			TrackerType: "Days",
			Remaining: "15",
			Used: "2"
		});

		equal(vm.AbsenceAccountExists(), true);
		equal(vm.AbsenceAccountPeriodStart().isSame(moment("2015-01-01")), true),
		equal(vm.AbsenceAccountPeriodEnd().isSame(moment("2015-12-31")), true),
		equal(vm.AbsenceTrackedAsDay(), true);
		equal(vm.AbsenceTrackedAsHour(), false);
		equal(vm.AbsenceUsed(), '2');
		equal(vm.AbsenceRemaining(), '15');
	});

	test("should read account values in hours", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readAbsenceAccount({
			PeriodStart: moment("2015-01-01"),
			PeriodEnd: moment("2015-12-31"),
			TrackerType: "Hours",
			Remaining: "18:00",
			Used: "2:00"
		});

		equal(vm.AbsenceAccountExists(), true);
		equal(vm.AbsenceAccountPeriodStart().isSame(moment("2015-01-01")), true),
		equal(vm.AbsenceAccountPeriodEnd().isSame(moment("2015-12-31")), true),
		equal(vm.AbsenceTrackedAsDay(), false);
		equal(vm.AbsenceTrackedAsHour(), true);
		equal(vm.AbsenceUsed(), '2:00');
		equal(vm.AbsenceRemaining(), '18:00');
	});

	test("should read with null account", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readAbsenceAccount();

		var today = moment().startOf('day');
		equal(vm.AbsenceAccountExists(), false);
		equal(vm.AbsenceAccountPeriodStart().isSame(today), true),
		equal(vm.AbsenceAccountPeriodEnd().isSame(today), true),
		equal(vm.AbsenceTrackedAsDay(), false);
		equal(vm.AbsenceTrackedAsHour(), false);
		equal(vm.AbsenceUsed(), '0');
		equal(vm.AbsenceRemaining(), '0');
	});

	test("could tolerate invalid date hash in url", function() {
		var old = Teleopti.MyTimeWeb.Portal.ParseHash;
		Teleopti.MyTimeWeb.Portal.ParseHash = function() {
			return {
				dateHash: "20170101"
			}
		}
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		Teleopti.MyTimeWeb.Portal.ParseHash = old;
		equal(vm.DateTo().isValid(), true);
	});

	test("should validate date from input for new request", function () {
		var html = '<div class="testRequestsDateTimeInputHtml">' +
						'<input data-bind="value: DateFrom, event: {blur: validateRequestTime}">' +
						'<input data-bind="value: DateTo">' +
						'<input data-bind="value: TimeFrom">' +
						'<input data-bind="value: TimeTo">' +
					'</div>';
		$('body').append(html);

		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		ko.applyBindings(vm, $('.testRequestsDateTimeInputHtml')[0]);

		vm.DateTo('2017-11-21');
		vm.TimeFrom('08:00');
		vm.TimeTo('08:01');
		vm.DateFrom('2017-11-22');

		$('.testRequestsDateTimeInputHtml input')[0].focus();
		$('.testRequestsDateTimeInputHtml input')[0].blur();

		equal(vm.ShowError(), true);
		equal(vm.ErrorMessage(), 'End time must be greater than start time');

		$('.testRequestsDateTimeInputHtml').remove();
	});

	test("should validate date to input for new request", function () {
		var html = '<div class="testRequestsDateTimeInputHtml">' +
						'<input data-bind="value: DateFrom">' +
						'<input data-bind="value: DateTo, event: {blur: validateRequestTime}">' +
						'<input data-bind="value: TimeFrom">' +
						'<input data-bind="value: TimeTo">' +
					'</div>';
		$('body').append(html);

		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		ko.applyBindings(vm, $('.testRequestsDateTimeInputHtml')[0]);

		vm.DateFrom('2017-11-22');
		vm.TimeFrom('08:00');
		vm.TimeTo('08:01');
		vm.DateTo('2017-11-21');

		$('.testRequestsDateTimeInputHtml input')[1].focus();
		$('.testRequestsDateTimeInputHtml input')[1].blur();

		equal(vm.ShowError(), true);
		equal(vm.ErrorMessage(), 'End time must be greater than start time');

		$('.testRequestsDateTimeInputHtml').remove();
	});

	test("should validate illegal time from input for new request on same day", function () {
		var html = '<div class="testRequestsDateTimeInputHtml">' +
						'<input data-bind="value: DateFrom">' +
						'<input data-bind="value: DateTo">' +
						'<input data-bind="value: TimeFrom, event: {blur: validateRequestTime}">' +
						'<input data-bind="value: TimeTo">' +
					'</div>';
		$('body').append(html);

		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		ko.applyBindings(vm, $('.testRequestsDateTimeInputHtml')[0]);

		vm.DateFrom('2017-11-22');
		vm.DateTo('2017-11-22');
		vm.TimeTo('08:00');
		vm.TimeFrom('08:00');

		$('.testRequestsDateTimeInputHtml input')[2].focus();
		$('.testRequestsDateTimeInputHtml input')[2].blur();

		equal(vm.ShowError(), true);
		equal(vm.ErrorMessage(), 'End time must be greater than start time');

		$('.testRequestsDateTimeInputHtml').remove();
	});

	test("should validate legal time from input for new request on same day", function () {
		var html = '<div class="testRequestsDateTimeInputHtml">' +
						'<input data-bind="value: DateFrom">' +
						'<input data-bind="value: DateTo">' +
						'<input data-bind="value: TimeFrom, event: {blur: validateRequestTime}">' +
						'<input data-bind="value: TimeTo">' +
					'</div>';
		$('body').append(html);

		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		ko.applyBindings(vm, $('.testRequestsDateTimeInputHtml')[0]);

		vm.DateFrom('2017-11-22');
		vm.DateTo('2017-11-22');
		vm.TimeTo('08:01');
		vm.TimeFrom('08:00');

		$('.testRequestsDateTimeInputHtml input')[2].focus();
		$('.testRequestsDateTimeInputHtml input')[2].blur();

		equal(vm.ShowError(), false);
		equal(vm.ErrorMessage(), '');

		$('.testRequestsDateTimeInputHtml').remove();
	});

	test("should validate illegal time to input for new request on same day", function () {
		var html = '<div class="testRequestsDateTimeInputHtml">' +
						'<input data-bind="value: DateFrom">' +
						'<input data-bind="value: DateTo">' +
						'<input data-bind="value: TimeFrom">' +
						'<input data-bind="value: TimeTo, event: {blur: validateRequestTime}">' +
					'</div>';
		$('body').append(html);

		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		ko.applyBindings(vm, $('.testRequestsDateTimeInputHtml')[0]);

		vm.DateFrom('2017-11-22');
		vm.DateTo('2017-11-22');
		vm.TimeFrom('08:00');
		vm.TimeTo('08:00');

		$('.testRequestsDateTimeInputHtml input')[3].focus();
		$('.testRequestsDateTimeInputHtml input')[3].blur();

		equal(vm.ShowError(), true);
		equal(vm.ErrorMessage(), 'End time must be greater than start time');

		$('.testRequestsDateTimeInputHtml').remove();
	});

	test("should validate legal time to input for new request on same day", function () {
		var html = '<div class="testRequestsDateTimeInputHtml">' +
						'<input data-bind="value: DateFrom">' +
						'<input data-bind="value: DateTo">' +
						'<input data-bind="value: TimeFrom">' +
						'<input data-bind="value: TimeTo, event: {blur: validateRequestTime}">' +
					'</div>';
		$('body').append(html);

		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		ko.applyBindings(vm, $('.testRequestsDateTimeInputHtml')[0]);

		vm.DateFrom('2017-11-22');
		vm.DateTo('2017-11-22');
		vm.TimeFrom('08:00');
		vm.TimeTo('08:01');

		$('.testRequestsDateTimeInputHtml input')[3].focus();
		$('.testRequestsDateTimeInputHtml input')[3].blur();

		equal(vm.ShowError(), false);
		equal(vm.ErrorMessage(), '');

		$('.testRequestsDateTimeInputHtml').remove();
	});
});