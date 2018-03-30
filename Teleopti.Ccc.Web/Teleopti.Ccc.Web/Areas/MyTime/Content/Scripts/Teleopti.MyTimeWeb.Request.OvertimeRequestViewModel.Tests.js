$(document).ready(function() {
	var vm,
		ajax,
		sentData,
		actualRequestData,
		addedOvertimeRequest,
		requestFormClosed = false,
		fakeOvertimeRequestResponse = {
			Id: '7155082E-108B-4F72-A36A-C1430C37CADA'
		},
		fakeRequestDetailViewModel = {
			CancelAddingNewRequest: function() {
				requestFormClosed = true;
			}
		},
		fakeAvailableDays = 13,
		fakeDefaultStartTimeFromBackend = {
			"IsShiftStartTime": false,
			"IsShiftEndTime": false,
			"DefaultStartTimeString":"2018-01-25 10:00"
		},
		hasFetchDefaultStartTime = false,
		dateOnlyFormat = Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly,
		requestDate = moment().format(dateOnlyFormat),
		toggleFnTemp, tempFn1, tempFn2, enabledTogglesList = [];

	module('Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel', {
		setup: function() {
			setup();
		},
		teardown: function () {
			restoreFn();
		}
	});

	function setup() {
		actualRequestData = undefined;
		setupAjax();
		requestFormClosed = false;
		addedOvertimeRequest = undefined;
		hasFetchDefaultStartTime = false;

		tempFn1 = Date.prototype.getTeleoptiTime;
		Date.prototype.getTeleoptiTime = function() {
			return new Date("2018-03-05T02:30:00Z").getTime();
		};

		tempFn2 = Date.prototype.getTeleoptiTimeInUserTimezone;
		Date.prototype.getTeleoptiTimeInUserTimezone = function() {
			return "2018-03-04";
		};		

		toggleFnTemp = Teleopti.MyTimeWeb.Common.IsToggleEnabled;
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(toggle) {
			if (enabledTogglesList.indexOf(toggle) > -1)
				return true;
		};

		vm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(data) {
			addedOvertimeRequest = data;
		}, fakeRequestDetailViewModel, null, false);
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');
	}

	function restoreFn() {
		Date.prototype.getTeleoptiTime = tempFn1;
		Date.prototype.getTeleoptiTimeInUserTimezone = tempFn2;

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = toggleFnTemp;
	}

	function setupAjax() {
		ajax = {
			Ajax: function(options) {
				if (options.url === 'OvertimeRequests/Save') {
					sentData = actualRequestData || options.data;
					if (sentData.Subject === '') {
						options.error({ responseJSON: { Errors: [requestsMessagesUserTexts.MISSING_SUBJECT] } });
					}
					else if (sentData.MultiplicatorDefinitionSet === '') {
						options.error({ responseJSON: { Errors: [requestsMessagesUserTexts.MISSING_OVERTIME_TYPE] } });
					} else {
						options.success(fakeOvertimeRequestResponse);
					}
				}

				if( options.url === 'OvertimeRequests/GetAvailableDays') {
					options.success(fakeAvailableDays);
				}

				if (options.url === 'OvertimeRequests/GetDefaultStartTime') {
					hasFetchDefaultStartTime = true;
					options.success(fakeDefaultStartTimeFromBackend);
				}
			}
		};
	}

	test('should have template', function() {
		equal(vm.Template, 'add-overtime-request-template');
	});

	test('should submit overtime request', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime("19:00");
		vm.RequestDuration('03:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(sentData.Subject, 'overtime request');
		equal(sentData.Message, 'I want to work overtime');
		equal(sentData.MultiplicatorDefinitionSet, '29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		var period = sentData.Period;
		equal(period.StartDate, requestDate);
		equal(period.EndDate, requestDate);
		equal(period.StartTime, '19:00');
		equal(period.EndTime, '22:00');
	});

	test('should input subject', function() {
		vm.Subject('');

		vm.AddRequest();

		equal(vm.ErrorMessage(), 'Missing subject');
	});

	test('should save overtime request', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime("19:00");
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(JSON.stringify(addedOvertimeRequest), JSON.stringify(fakeOvertimeRequestResponse));
	});

	test('should post correct EndDate and EndTime with cross day', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime("19:00");
		vm.RequestDuration('06:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(sentData.Period.StartDate, requestDate);
		equal(sentData.Period.StartTime, '19:00');
		equal(sentData.Period.EndDate, moment(requestDate).add(1, 'days').format(dateOnlyFormat));
		equal(sentData.Period.EndTime, '01:00');
	});

	test('should post correct EndDate and EndTime with intraday', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime("19:30");
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(sentData.Period.StartDate, requestDate);
		equal(sentData.Period.StartTime, '19:30');
		equal(sentData.Period.EndDate, requestDate);
		equal(sentData.Period.EndTime, '20:30');
	});

	test('should calculate correct EndDate and EndTime with meridian', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime("9:30 AM");
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(sentData.Period.StartDate, requestDate);
		equal(sentData.Period.StartTime, '09:30');
		equal(sentData.Period.EndDate, requestDate);
		equal(sentData.Period.EndTime, '10:30');
	});

	test('should not pass validation when post data has no subject', function() {
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime("19:00");
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(addedOvertimeRequest, undefined);
		equal(vm.ErrorMessage(), 'Missing subject');
	});

	test('should not pass validation when post data has no start time', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.StartTime('');
		vm.DateFrom(requestDate);
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();
		equal(addedOvertimeRequest, undefined);
		equal(vm.ErrorMessage(), 'Missing start time');
	});

	test('should not pass validation when post data has no duration', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime("19:00");
		vm.RequestDuration('');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(addedOvertimeRequest, undefined);
		equal(vm.ErrorMessage(), 'Missing duration');
	});

	test('should not pass validation when duration is 00:00', function () {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime('19:00');
		vm.RequestDuration('00:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(addedOvertimeRequest, undefined);
		equal(vm.ErrorMessage(), 'Missing duration');
	});

	test('should set PeriodEndDate according to available days when toggle OvertimeRequestPeriodSetting_46417 is on', function() {
		enabledTogglesList = ['OvertimeRequestPeriodSetting_46417'];

		var vm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(data) {
			addedOvertimeRequest = data;
		}, fakeRequestDetailViewModel);

		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(moment().format(dateOnlyFormat));
		vm.StartTime("19:00");
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		equal(vm.PeriodEndDate().format(dateOnlyFormat), moment().add(fakeAvailableDays, 'days').format(dateOnlyFormat));
	});

	test('should not pass validation when request date is past date', function () {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(moment().add(-1, 'days').format(dateOnlyFormat));
		vm.StartTime("19:00");
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(addedOvertimeRequest, undefined);
		equal(vm.ErrorMessage(), 'Can not add overtime request on past date');
	});

	test('should not pass validation when post data has no overtime type', function () {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(moment().add(1, 'days').format(dateOnlyFormat));
		vm.StartTime("19:00");
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('');

		vm.AddRequest();

		equal(addedOvertimeRequest, undefined);
		equal(vm.ErrorMessage(), 'Missing overtime type');
	});

	test('should close overtime request form panel after posting data', function() {
		vm.IsPostingData(false);
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime("19:00");
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		equal(requestFormClosed, false);
		vm.AddRequest();
		equal(requestFormClosed, true);
	});

	test('should limit lenght of message to 2000 chars', function() {
		var html = '<textarea id="MessageBox" data-bind="value: Message, event:{change:checkMessageLength}" />';

		$('body').append(html);
		ko.applyBindings(vm, $('#MessageBox')[0]);

		$('#MessageBox').val(new Array(2001).join('a'));
		$('#MessageBox').change();

		equal(vm.Message().length, 2000);

		$('#MessageBox').remove();
	});

	test('should not exceed 23 hours for request duration', function() {
		var html = '<input id="duration" data-bind="value: RequestDuration, event:{change:validateDuration}"/>';

		$('body').append(html);
		ko.applyBindings(vm, $('#duration')[0]);

		$('#duration').val('99:00');
		$('#duration').change();

		equal(vm.RequestDuration(), '00:00');

		$('#duration').remove();
	});

	test('should not exceed 59 minutes for request duration', function() {
		var html = '<input id="duration" data-bind="value: RequestDuration, event:{change:validateDuration}"/>';

		$('body').append(html);
		ko.applyBindings(vm, $('#duration')[0]);

		$('#duration').val('01:60');
		$('#duration').change();

		equal(vm.RequestDuration(), '01:59');

		$('#duration').remove();
	});

	test('should allow empty request duration', function() {
		var html = '<input id="duration" data-bind="value: RequestDuration, event:{change:validateDuration}"/>';

		$('body').append(html);
		ko.applyBindings(vm, $('#duration')[0]);

		$('#duration').val('');
		$('#duration').change();

		equal(vm.RequestDuration(), '');

		$('#duration').remove();
	});

	test('should close request duration time list when clicking outside', function() {
		var html = 	'<div id="test-duration">' +
						'<div id="outside-div"></div>' +
						'<span id="duration-container" data-bind="outsideClickCallback: CloseDropdownTimeList">' +
							'<input id="duration" data-bind="value: RequestDuration, event:{change:validateDuration}"/>' +
						'</span>' +
					'</div>';

		$('body').append(html);
		ko.applyBindings(vm, $('#test-duration')[0]);
		vm.IsTimeListOpened(true);

		$('#outside-div').click();

		equal(vm.IsTimeListOpened(), false);

		$('#test-duration').remove();
	});

	test('should contains only one : in request duration', function() {
		var html = '<input id="duration" data-bind="value: RequestDuration, event:{change:validateDuration}"/>';

		$('body').append(html);
		ko.applyBindings(vm, $('#duration')[0]);

		$('#duration').val('01:60:00');
		$('#duration').change();

		equal(vm.RequestDuration(), '01:59');

		$('#duration').val('016000');
		$('#duration').change();

		equal(vm.RequestDuration(), '00:59');

		$('#duration').remove();
	});

	test('should initialize with data when viewing request detail', function() {
		var data = {
			Subject: "subject",
			Text: "text",
			DateTimeFrom: "2017-06-30T03:45:00.0000000",
			DateTimeTo: "2017-06-30T06:45:00.0000000",
			MultiplicatorDefinitionSet: "9019D62F-0086-44B1-A977-9BB900B8C361"
		};
		var isViewingDetail = true;

		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(data) {
			addedOvertimeRequest = data;
		}, fakeRequestDetailViewModel, null, isViewingDetail);
		requestVm.Initialize(data);

		equal(requestVm.Subject(), "subject");
		equal(requestVm.Message(), "text");
		equal(requestVm.DateFrom().format("YYYY-MM-DD"), "2017-06-30");
		equal(requestVm.StartTime(), "03:45");
		equal(requestVm.RequestDuration(), "03:00");
		equal(requestVm.MultiplicatorDefinitionSetId(), "9019D62F-0086-44B1-A977-9BB900B8C361");
	});

	test('should set default start time in AM/PM format when showing Meridian', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = "hh:mm A";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);
		equal(requestVm.StartTime(), moment().add(20, 'minutes').format('hh:mm A'));
	});

	test('should set default start time in 24 hours format when not showing Meridian', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);

		equal(requestVm.StartTime(), moment().add(20, 'minutes').format('HH:mm'));
	});

	test('should use default start time by default', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);

		equal(requestVm.UseDefaultStartTime(), true);
	});

	test('should use default start time from backend as default start date when UseDefaultStartTime toggle is toggled on', function() {
		enabledTogglesList = ['OvertimeRequestPeriodSetting_46417', 'MyTimeWeb_OvertimeRequestDefaultStartTime_47513'];

		var tomorrow = moment().add(1, 'days').hours(17).minutes(0);

		fakeDefaultStartTimeFromBackend.DefaultStartTimeString = tomorrow.format('YYYY-MM-DD HH:mm');

		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);

		requestVm.DateFrom(moment());
		requestVm.UseDefaultStartTime(false);
		requestVm.UseDefaultStartTime(true);

		equal(requestVm.UseDefaultStartTime(), true);
		equal(requestVm.DateFrom().format('YYYY-MM-DD'), tomorrow.format('YYYY-MM-DD'));
	});

	test('should not fetch default start time when viewing overtime request detail', function () {
		enabledTogglesList = ['OvertimeRequestPeriodSetting_46417', 'MyTimeWeb_OvertimeRequestDefaultStartTime_47513'];

		var tomorrow = moment().add(1, 'days').hours(17).minutes(0);
		fakeDefaultStartTimeFromBackend.DefaultStartTimeString = tomorrow.format('YYYY-MM-DD HH:mm');

		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function () { }, fakeRequestDetailViewModel, null, true, true);
		requestVm.DateFrom(moment());
		requestVm.UseDefaultStartTime(false);
		requestVm.UseDefaultStartTime(true);

		var fakeData = {
			DateFromDayOfMonth: 31,
			DateFromMonth: 1,
			DateFromYear: 2018,
			DateTimeFrom: "2018-01-31T17:00:00.0000000",
			DateTimeTo: "2018-01-31T20:00:00.0000000",
			DateToDayOfMonth: 31,
			DateToMonth: 1,
			DateToYear: 2018,
			DenyReason: "January contains too much overtime (04:00). Max is 03:00.",
			ExchangeOffer: null,
			From: "",
			Id: "2f4a7e4d-7b34-4005-a1d0-a87900125281",
			IsApproved: false,
			IsCreatedByUser: false,
			IsDenied: true,
			IsFullDay: false,
			IsNew: false,
			IsNextDay: false,
			IsPending: false,
			IsReferred: false,
			Link: {
				rel: "self",
				href: "/TeleoptiWFM/Web/MyTime/Requests/RequestDetail/2f4a7e4d-7b34-4005-a1d0-a87900125281", Methods: "GET",
			},
			MultiplicatorDefinitionSet: "29f7ece8-d340-408f-be40-9bb900b8a4cb",
			Payload: "Overtime paid",
			PayloadId: null,
			Status: "Denied",
			Subject: "31",
			Text: "31",
			To: "",
			Type: "Overtime",
			TypeEnum: 4,
			UpdatedOnDateTime: "2018-01-31T02:06:42.6270000"
		};
		requestVm.Initialize(fakeData);

		equal(hasFetchDefaultStartTime, false);
		equal(requestVm.DateFrom().format('YYYY-MM-DD'), moment(fakeData.DateTimeFrom).format('YYYY-MM-DD'));
		equal(requestVm.StartTime().format('HH:mm'), moment(fakeData.DateTimeFrom).format('HH:mm'));
	});

	test('should use selected day as default start time when UseDefaultStartTime toggle is toggled off', function() {
		enabledTogglesList = ['OvertimeRequestPeriodSetting_46417', 'MyTimeWeb_OvertimeRequestDefaultStartTime_47513'];

		var tomorrow = moment().add(1, 'days').hours(17).minutes(0);
		fakeDefaultStartTimeFromBackend.DefaultStartTimeString = tomorrow.format('YYYY-MM-DD HH:mm');


		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);

		requestVm.DateFrom(moment());
		requestVm.UseDefaultStartTime(false);

		equal(requestVm.UseDefaultStartTime(), false);
		equal(requestVm.DateFrom().format('YYYY-MM-DD'), moment().format('YYYY-MM-DD'));
	});

	test('should use shift start time subtracting duration for default start time', function() {
		enabledTogglesList = ['OvertimeRequestPeriodSetting_46417', 'MyTimeWeb_OvertimeRequestDefaultStartTime_47513'];

		var tomorrow = moment().add(1, 'days').hours(17).minutes(0);
		fakeDefaultStartTimeFromBackend.DefaultStartTimeString = tomorrow.format('YYYY-MM-DD HH:mm');
		fakeDefaultStartTimeFromBackend.IsShiftStartTime = true;
		fakeDefaultStartTimeFromBackend.IsShiftEndTime = false;

		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);

		requestVm.DateFrom(moment());
		requestVm.UseDefaultStartTime(false);
		requestVm.UseDefaultStartTime(true);
		requestVm.RequestDuration('01:00');

		equal(requestVm.UseDefaultStartTime(), true);
		equal(requestVm.DateFrom().format('YYYY-MM-DD'), tomorrow.format('YYYY-MM-DD'));

		var expectedStartTime = tomorrow.add(-requestVm.RequestDuration().split(':')[0], 'hours')
								.add(-requestVm.RequestDuration().split(':')[1], 'minutes').format('HH:mm');

		equal(requestVm.StartTime(), expectedStartTime);
	});

	test('should not subtract duration from shift end time when setting default start time', function() {
		enabledTogglesList = ['OvertimeRequestPeriodSetting_46417', 'MyTimeWeb_OvertimeRequestDefaultStartTime_47513'];

		var tomorrow = moment().add(1, 'days').hours(7).minutes(0);
		fakeDefaultStartTimeFromBackend.DefaultStartTimeString = tomorrow.format('YYYY-MM-DD HH:mm');
		fakeDefaultStartTimeFromBackend.IsShiftStartTime = false;
		fakeDefaultStartTimeFromBackend.IsShiftEndTime = true;

		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);

		requestVm.DateFrom(moment());
		requestVm.UseDefaultStartTime(false);
		requestVm.UseDefaultStartTime(true);
		requestVm.RequestDuration('01:00');

		equal(requestVm.UseDefaultStartTime(), true);
		equal(requestVm.DateFrom().format('YYYY-MM-DD'), tomorrow.format('YYYY-MM-DD'));
		equal(requestVm.StartTime(), tomorrow.format('HH:mm'));
	});

	test('should re-calculate start time when changing duration', function() {
		enabledTogglesList = ['OvertimeRequestPeriodSetting_46417', 'MyTimeWeb_OvertimeRequestDefaultStartTime_47513'];

		var tomorrow = moment().add(1, 'days').hours(7).minutes(0);
		fakeDefaultStartTimeFromBackend.DefaultStartTimeString = tomorrow.format('YYYY-MM-DD HH:mm');
		fakeDefaultStartTimeFromBackend.IsShiftStartTime = true;
		fakeDefaultStartTimeFromBackend.IsShiftEndTime = false;

		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);

		requestVm.DateFrom(moment());
		requestVm.UseDefaultStartTime(false);
		requestVm.UseDefaultStartTime(true);
		requestVm.RequestDuration('03:00');

		equal(requestVm.DateFrom().format('YYYY-MM-DD'), tomorrow.format('YYYY-MM-DD'));

		var expectedStartTime = tomorrow.add(-requestVm.RequestDuration().split(':')[0], 'hours')
								.add(-requestVm.RequestDuration().split(':')[1], 'minutes').format('HH:mm');

		equal(requestVm.StartTime(), expectedStartTime);
	});

	test('should not show UseDefaultStartTime toggle when viewing an overtime request', function () {
		enabledTogglesList = ['OvertimeRequestPeriodSetting_46417', 'MyTimeWeb_OvertimeRequestDefaultStartTime_47513'];

		var tomorrow = moment('2018-01-31').add(1, 'days').hours(7).minutes(0);
		fakeDefaultStartTimeFromBackend.DefaultStartTimeString = tomorrow.format('YYYY-MM-DD HH:mm');
		fakeDefaultStartTimeFromBackend.IsShiftStartTime = true;
		fakeDefaultStartTimeFromBackend.IsShiftEndTime = false;

		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function () { }, fakeRequestDetailViewModel, null, true);

		requestVm.DateFrom(moment('2018-01-31'));
		requestVm.UseDefaultStartTime(false);
		requestVm.UseDefaultStartTime(true);
		requestVm.RequestDuration('03:00');

		var fakeData = {
			DateFromDayOfMonth: 31,
			DateFromMonth: 1,
			DateFromYear: 2018,
			DateTimeFrom: "2018-01-31T17:00:00.0000000",
			DateTimeTo: "2018-01-31T20:00:00.0000000",
			DateToDayOfMonth: 31,
			DateToMonth: 1,
			DateToYear: 2018,
			DenyReason: "January contains too much overtime (04:00). Max is 03:00.",
			ExchangeOffer: null,
			From: "",
			Id: "2f4a7e4d-7b34-4005-a1d0-a87900125281",
			IsApproved: false,
			IsCreatedByUser: false,
			IsDenied: true,
			IsFullDay: false,
			IsNew: false,
			IsNextDay: false,
			IsPending: false,
			IsReferred: false,
			Link: {
				rel: "self",
				href: "/TeleoptiWFM/Web/MyTime/Requests/RequestDetail/2f4a7e4d-7b34-4005-a1d0-a87900125281", Methods: "GET",
			},
			MultiplicatorDefinitionSet: "29f7ece8-d340-408f-be40-9bb900b8a4cb",
			Payload: "Overtime paid",
			PayloadId: null,
			Status: "Denied",
			Subject: "31",
			Text: "31",
			To: "",
			Type: "Overtime",
			TypeEnum: 4,
			UpdatedOnDateTime: "2018-01-31T02:06:42.6270000",
		};
		requestVm.IsEditable(false);
		requestVm.Initialize(fakeData);

		equal(requestVm.showUseDefaultStartTimeToggle(), false);
	});

	test('should set overtime request duration to one hour by default', function() {
		equal(vm.RequestDuration(), '01:00');
	});

	test('should display default start date and time by agents timezone', function() {
		fakeRequestDetailViewModel.baseUtcOffsetInMinutes = -600;
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);
		equal(requestVm.StartTime(), moment().add(20, 'minutes').zone(-fakeRequestDetailViewModel.baseUtcOffsetInMinutes).format("HH:mm"));
	});

	test('should display error message from server response when missing overtime time', function() {
		actualRequestData = { Subject: 'overtime request', MultiplicatorDefinitionSet: '' };

		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime("19:00");
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(vm.ErrorMessage(), requestsMessagesUserTexts.MISSING_OVERTIME_TYPE);
	});
});