$(document).ready(function() {
	var hash = '',
		teamScheduleData,
		fakeWindow,
		ajax,
		fetchTeamScheduleDataRequestCount,
		constants = Teleopti.MyTimeWeb.Common.Constants,
		dateFormat = constants.serviceDateTimeFormat.dateOnly,
		vm;

	module('Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule', {
		setup: function() {
			setup();
		},
		teardown: function() {
			$('.mobile-teamschedule-view').remove();
		}
	});

	test('should select today as selectedDate(displayDate)', function() {
		var html = [
			'<li class="mobile-datepicker">',
			'					<div class="input-group">',
			'						<span class="input-group-btn">',
			'							<button class="btn btn-default">',
			'								<i class="glyphicon glyphicon-chevron-left"></i>',
			'							</button>',
			'						</span>',
			'						<input class="form-control text-center date-input-style" data-bind="value: displayDate" type="text" readonly="readonly"/>',
			'						<span class="input-group-btn">',
			'							<span class="btn btn-default moment-datepicker" type="button">',
			'								<i class="glyphicon glyphicon-calendar"></i>',
			'							</span>',
			'							<button class="btn btn-default">',
			'								<i class="glyphicon glyphicon-chevron-right"></i>',
			'							</button>',
			'						</span>',
			'					</div>',
			'				</li>'
		].join('');
		$('.mobile-teamschedule-view').append(html);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		var today = moment().format(dateFormat);

		equal(vm.selectedDate().format(dateFormat), today);
		equal($('.mobile-datepicker input').val(),today);
	});

	test('should change selected date after clicking on "previous day" icon', function() {
		var today = moment();

		var html = [
			'<li class="mobile-datepicker">',
			'					<div class="input-group">',
			'						<span class="input-group-btn">',
			'							<button class="btn btn-default previous-day" data-bind="click: previousDay">',
			'								<i class="glyphicon glyphicon-chevron-left"></i>',
			'							</button>',
			'						</span>',
			'						<input class="form-control text-center date-input-style" data-bind="value: displayDate" type="text" readonly="readonly"/>',
			'						<span class="input-group-btn">',
			'							<span class="btn btn-default moment-datepicker" type="button">',
			'								<i class="glyphicon glyphicon-calendar"></i>',
			'							</span>',
			'							<button class="btn btn-default">',
			'								<i class="glyphicon glyphicon-chevron-right"></i>',
			'							</button>',
			'						</span>',
			'					</div>',
			'				</li>'
		].join('');
		$('.mobile-teamschedule-view').append(html);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);
		$('.previous-day').click();

		var expectDateStr = today.add(-1, 'days').format(dateFormat);

		equal(vm.selectedDate().format(dateFormat), expectDateStr);
		equal($('.mobile-datepicker input').val(), expectDateStr);
	});

	test('should change selected date after clicking on "next day" icon', function() {
		var today = moment();

		var html = [
			'<li class="mobile-datepicker">',
			'					<div class="input-group">',
			'						<span class="input-group-btn">',
			'							<button class="btn btn-default">',
			'								<i class="glyphicon glyphicon-chevron-left"></i>',
			'							</button>',
			'						</span>',
			'						<input class="form-control text-center date-input-style" data-bind="value: displayDate" type="text" readonly="readonly"/>',
			'						<span class="input-group-btn">',
			'							<span class="btn btn-default moment-datepicker" type="button">',
			'								<i class="glyphicon glyphicon-calendar"></i>',
			'							</span>',
			'							<button class="btn btn-default next-day" data-bind="click: nextDay">',
			'								<i class="glyphicon glyphicon-chevron-right"></i>',
			'							</button>',
			'						</span>',
			'					</div>',
			'				</li>'
		].join('');

		$('.mobile-teamschedule-view').append(html);
		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);
		$('.next-day').click();

		var expectDateStr = today.add(1, 'days').format(dateFormat);

		equal(vm.selectedDate().format(dateFormat), expectDateStr);
		equal($('.mobile-datepicker input').val(), expectDateStr);
	});

	test('should change display date after changing selected date', function() {
		var today = moment();

		var html = [
			'<li class="mobile-datepicker">',
			'					<div class="input-group">',
			'						<span class="input-group-btn">',
			'							<button class="btn btn-default">',
			'								<i class="glyphicon glyphicon-chevron-left"></i>',
			'							</button>',
			'						</span>',
			'						<input class="form-control text-center date-input-style" data-bind="value: displayDate" type="text" readonly="readonly"/>',
			'						<span class="input-group-btn">',
			'							<span class="btn btn-default moment-datepicker" type="button">',
			'								<i class="glyphicon glyphicon-calendar"></i>',
			'							</span>',
			'							<button class="btn btn-default next-day" data-bind="click: nextDay">',
			'								<i class="glyphicon glyphicon-chevron-right"></i>',
			'							</button>',
			'						</span>',
			'					</div>',
			'				</li>'
		].join('');

		$('.mobile-teamschedule-view').append(html);
		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		vm.selectedDate(today.add(9, 'days'));

		var expectDateStr = vm.selectedDate().format(dateFormat);

		equal($('.mobile-datepicker input').val(), expectDateStr);
	});

	function setup() {
		$('body').append('<div class="mobile-teamschedule-view"></div>');
		Teleopti.MyTimeWeb.UserInfo = {
			WhenLoaded: function(whenLoadedCallBack) {
				var data = { WeekStart: '' };
				whenLoadedCallBack(data);
			}
		};

		fakeWindow = {
			navigator: {
				appVersion:
					'5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1',
				userAgent:
					'Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1'
			}
		};
		Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax,
			fakeWindow
		);

		vm = Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.Vm();
	}

	function fakeReadyForInteractionCallback() {}

	function fakeCompletelyLoadedCallback() {}
});
