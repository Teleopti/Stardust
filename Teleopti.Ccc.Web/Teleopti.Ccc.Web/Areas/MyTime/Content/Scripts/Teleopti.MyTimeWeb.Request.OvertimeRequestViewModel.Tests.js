var requestsMessagesUserTexts = {
	MISSING_SUBJECT: "Missing subject",
	MISSING_STARTTIME: "Missing start time",
	MISSING_DURATION: "Missing duration",
	OVERTIME_REQUEST_DATE_EXCEEDS_14DAYS: "Request date exceeds 14 days",
	OVERTIME_REQUEST_DATE_IS_PAST: "Can not add overtime request on past date"
};
$(document).ready(function() {
	var vm,
		ajax,
		sentData,
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
		requestDate = moment().format(Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly);

	module('Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel', {
		setup: function() {
			setup();
		}
	});

	function setup() {
		setupAjax();
		requestFormClosed = false;
		addedOvertimeRequest = undefined;

		vm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(data) {
			addedOvertimeRequest = data;
		}, fakeRequestDetailViewModel);
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');
	}

	function setupAjax() {
		ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/PersistOvertimeRequest') {
					sentData = options.data;
					options.success(fakeOvertimeRequestResponse);
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
		vm.StartTime('19:00');
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
		vm.StartTime('19:00');
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(JSON.stringify(addedOvertimeRequest), JSON.stringify(fakeOvertimeRequestResponse));
	});

	test('should post correct EndDate and EndTime with cross day', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime('19:00');
		vm.RequestDuration('06:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(sentData.Period.StartDate, requestDate);
		equal(sentData.Period.StartTime, '19:00');
		equal(sentData.Period.EndDate, moment(requestDate).add(1, 'days').format(Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly));
		equal(sentData.Period.EndTime, '01:00');
	});

	test('should post correct EndDate and EndTime with intraday', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime('19:30');
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
		vm.StartTime('09:30 AM');
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
		vm.StartTime('19:00');
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
		vm.StartTime('19:00');
		vm.RequestDuration('');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(addedOvertimeRequest, undefined);
		equal(vm.ErrorMessage(), 'Missing duration');
	});

	test('should not pass validation when request date exceeds 14 days', function() {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(moment().add(14, 'days').format(Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly));
		vm.StartTime('19:00');
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(addedOvertimeRequest, undefined);
		equal(vm.ErrorMessage(), 'Request date exceeds 14 days');
	});

	test('should not pass validation when request date is past date', function () {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(moment().add(-1, 'days').format(Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly));
		vm.StartTime('19:00');
		vm.RequestDuration('01:00');
		vm.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(addedOvertimeRequest, undefined);
		equal(vm.ErrorMessage(), 'Can not add overtime request on past date');
	});

	test('should close overtime request form panel after posting data', function() {
		vm.IsPostingData(false);
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom(requestDate);
		vm.StartTime('19:00');
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

	test('should set overtime request duration to one hour by default', function() {
		equal(vm.RequestDuration(), '01:00');
	});

	test('should display default start date and time by agents timezone', function() {
		fakeRequestDetailViewModel.baseUtcOffsetInMinutes = -600;
		var requestVm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, function(){}, fakeRequestDetailViewModel, null, false);
		equal(requestVm.StartTime(), moment().add(20, 'minutes').zone(-fakeRequestDetailViewModel.baseUtcOffsetInMinutes).format("HH:mm"));
	});
});