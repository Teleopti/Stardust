$(document).ready(function() {
	var hash = '',
		completeLoadedCount = 0,
		fakeTeamScheduleData,
		fakeDefaultTeamData,
		fakeAvailableTeamsData,
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
		equal($('.mobile-teamschedule-view .mobile-datepicker input').val(), today);
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
		equal($('.mobile-teamschedule-view .mobile-datepicker input').val(), expectDateStr);
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
		equal($('.mobile-teamschedule-view .mobile-datepicker input').val(), expectDateStr);
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

		equal($('.mobile-teamschedule-view .mobile-datepicker input').val(), expectDateStr);
	});

	test('should render sites and teams', function() {
		var today = moment();

		var html = [
			'	<div class="teamschedule-filter-component">',
			'		<select data-bind="foreach: availableTeams, select2: { value: selectedTeam } ">',
			'			<optgroup data-bind="attr: { label: text }, foreach: children">',
			'				<option data-bind="text: text, value: id"></option>',
			'			</optgroup>',
			'		</select>',
			'	</div>',
		].join('');

		$('.mobile-teamschedule-view').append(html);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);
		equal($('.mobile-teamschedule-view .teamschedule-filter-component select')[0].length, 11);

		equal($($('.mobile-teamschedule-view .teamschedule-filter-component select')[0][0]).text(), 'All Teams');
		equal($($('.mobile-teamschedule-view .teamschedule-filter-component select')[0][1]).text(), 'London/Students');
		equal($($('.mobile-teamschedule-view .teamschedule-filter-component select')[0][2]).text(), 'London/Team Flexible');
		equal($($('.mobile-teamschedule-view .teamschedule-filter-component select')[0][3]).text(), 'London/Team Outbound');
		equal($($('.mobile-teamschedule-view .teamschedule-filter-component select')[0][4]).text(), 'London/Team Preferences');
		equal($($('.mobile-teamschedule-view .teamschedule-filter-component select')[0][5]).text(), 'London/Team Rotations');
		equal($($('.mobile-teamschedule-view .teamschedule-filter-component select')[0][10]).text(), 'Contract Group/Team Rotations');
	});

	test('should select default team', function() {
		var today = moment();

		var html = [
			'	<div class="teamschedule-filter-component">',
			'		<select data-bind="foreach: availableTeams, select2: { value: selectedTeam } ">',
			'			<optgroup data-bind="attr: { label: text }, foreach: children">',
			'				<option data-bind="text: text, value: id"></option>',
			'			</optgroup>',
			'		</select>',
			'	</div>',
		].join('');

		$('.mobile-teamschedule-view').append(html);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		equal(completeLoadedCount, 1);
		equal($('.mobile-teamschedule-view .teamschedule-filter-component select').val(), fakeDefaultTeamData.DefaultTeam);
	});

	function setUpFakeData() {
		fakeAvailableTeamsData = {
			allTeam: { id: 'allTeams', text: 'All Teams' },
			teams: [
				{
					children: [
						{ id: 'e5f968d7-6f6d-407c-81d5-9b5e015ab495', text: 'London/Students' },
						{ id: 'd7a9c243-8cd8-406e-9889-9b5e015ab495', text: 'London/Team Flexible' },
						{ id: 'a74e1f94-7662-4a7f-9746-a56e00a66f17', text: 'London/Team Outbound' },
						{ id: '34590a63-6331-4921-bc9f-9b5e015ab495', text: 'London/Team Preferences' },
						{ id: 'e7ce8892-4db3-49c8-bdf6-9b5e015ab495', text: 'London/Team Rotations' }
					],
					PageId: '6ce00b41-0722-4b36-91dd-0a3b63c545cf',
					text: 'Business Hierarchy'
				},
				{
					children: [
						{ id: 'e5f968d7-6f6d-407c-81d5-9b5e015ab491', text: 'Contract Group/Students' },
						{ id: 'd7a9c243-8cd8-406e-9889-9b5e015ab492', text: 'Contract Group/Team Flexible' },
						{ id: 'a74e1f94-7662-4a7f-9746-a56e00a66f13', text: 'Contract Group/Team Outbound' },
						{ id: '34590a63-6331-4921-bc9f-9b5e015ab494', text: 'Contract Group/Team Preferences' },
						{ id: 'e7ce8892-4db3-49c8-bdf6-9b5e015ab496', text: 'Contract Group/Team Rotations' }
					],
					PageId: '6ce00b41-0722-4b36-91dd-0a3b63c545cf',
					text: 'Contract Group'
				}
			]
		};
		fakeDefaultTeamData = { DefaultTeam: fakeAvailableTeamsData.teams[0].children[0].id };
	}

	function setupAjax() {
		ajax = {
			Ajax: function(option) {
				if (option.url === 'Team/TeamsAndGroupsWithAllTeam') {
					option.success(fakeAvailableTeamsData);
				}

				if (option.url === 'TeamSchedule/DefaultTeam') {
					option.success(fakeDefaultTeamData);
				}
			}
		};
	}

	function setup() {
		completeLoadedCount = 0;
		setUpFakeData();
		setupAjax();

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

	function fakeCompletelyLoadedCallback() {
		completeLoadedCount++;
	}
});
