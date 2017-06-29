$(document).ready(function () {
	var vm,
		ajax,
		sentData,
		addedOvertimeRequest,
		fakeActivities = [
			{
				Id: '90EA529A-EEA0-4E22-80AB-9B5E015AB3C6',
				Name: 'Phone'
			},
			{
				Id: 'CAD1BFBA-3A35-46BD-8AC2-9B5E015AB3C6',
				Name: 'Email'
			}
		],
		fakeDefinitionSets = [
			{
				Id: '29F7ECE8-D340-408F-BE40-9BB900B8A4CB',
				Name: 'Overtime paid'
			},
			{
				Id: '9019D62F-0086-44B1-A977-9BB900B8C361',
				Name: 'Overtime time'
			}],
		fakeOvertimeRequestResponse = {
			Id: '7155082E-108B-4F72-A36A-C1430C37CADA'
		},
		fakeRequestListViewModel = {
			AddItemAtTop: function (item) {
				addedOvertimeRequest = item;
			}
		};

	module('Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel',
		{
			setup: function () {
				setup();
			}
		});

	function setup() {
		setupAjax();
		vm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax, fakeRequestListViewModel);
	}

	function setupAjax() {
		ajax = {
			Ajax: function (options) {
				if (options.url === '../api/MultiplicatorDefinitionSet/Mine') {
					options.success(fakeDefinitionSets);
				}
				if (options.url === '../api/Request/AddOvertime') {
					sentData = options.data;
					options.success(fakeOvertimeRequestResponse);
				}
			}
		};
	}

	test('should have template', function () {
		equal(vm.Template,'add-overtime-request-template');
	});

	test('should display overtime types', function () {
		equal(vm.MultiplicatorDefinitionSets.length, fakeDefinitionSets.length);

		fakeDefinitionSets.forEach(function (set, index) {
			equal(vm.MultiplicatorDefinitionSets[index].Id, set.Id);
			equal(vm.MultiplicatorDefinitionSets[index].Name, set.Name);
		});
	});

	test('should submit overtime request', function () {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom('2017-06-27');
		vm.DateTo('2017-06-27');
		vm.TimeFrom('19:00');
		vm.TimeTo('22:00');
		vm.MultiplicatorDefinitionSet('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(sentData.Subject, 'overtime request');
		equal(sentData.Message, 'I want to work overtime');
		equal(sentData.MultiplicatorDefinitionSet, '29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		var period = sentData.Period;
		equal(period.StartDate, '2017-06-27');
		equal(period.EndDate, '2017-06-27');
		equal(period.StartTime, '19:00');
		equal(period.EndTime, '22:00');
	});

	test('should save overtime request', function () {
		vm.Subject('overtime request');
		vm.Message('I want to work overtime');
		vm.DateFrom('2017-06-27');
		vm.DateTo('2017-06-27');
		vm.TimeFrom('19:00');
		vm.TimeTo('22:00');
		vm.MultiplicatorDefinitionSet('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');

		vm.AddRequest();

		equal(JSON.stringify(addedOvertimeRequest), JSON.stringify(fakeOvertimeRequestResponse));
	});

	test('should enable save button after posting data', function () {
		vm.IsPostingData(true);
		vm.AddRequest();

		equal(vm.IsPostingData(), false);
	});

	test('should limit lenght of message to 2000 chars', function () {
		var html = '<textarea id="MessageBox" data-bind="value: Message, event:{change:checkMessageLength}" />';

		$('body').append(html);
		ko.applyBindings(vm, $('#MessageBox')[0]);

		$('#MessageBox').val(new Array(2001).join('a'));
		$('#MessageBox').change();

		equal(vm.Message().length, 2000);

		$('#MessageBox').remove();
	});

	test('should not exceed 23 hours for request duration', function () {
		var html = '<input id="duration" data-bind="value: RequestDuration, event:{change:validateDuration}"/>';

		$('body').append(html);
		ko.applyBindings(vm, $('#duration')[0]);

		$('#duration').val('99:00');
		$('#duration').change();

		equal(vm.RequestDuration(), '00:00');

		$('#duration').remove();
	});

	test('should not exceed 59 minutes for request duration', function () {
		var html = '<input id="duration" data-bind="value: RequestDuration, event:{change:validateDuration}"/>';

		$('body').append(html);
		ko.applyBindings(vm, $('#duration')[0]);

		$('#duration').val('01:60');
		$('#duration').change();

		equal(vm.RequestDuration(), '01:59');

		$('#duration').remove();
	});

	test('should contains only one : in request duration', function () {
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
});