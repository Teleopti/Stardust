﻿$(document).ready(function() {
	var agentSchedulesHtml = '',
		completeLoadedCount = 0,
		fakeTeamScheduleData,
		fakeOriginalAgentSchedulesData,
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
			Teleopti.MyTimeWeb.Common.DateFormat = null;
		}
	});

	test('should select today as selectedDate(displayDate)', function () {
		initVm();

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

	test('should change selected date after clicking on "previous day" icon', function () {
		initVm();
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

	test('should change selected date after clicking on "next day" icon', function () {
		initVm();

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

	test('should change display date after changing selected date', function () {
		initVm();

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

	test('should render sites and teams', function () {
		initVm();

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

		var selector = '.mobile-teamschedule-view .teamschedule-filter-component select';
		equal($(selector)[0].length, 11);

		equal($($(selector)[0][0]).text(), 'All Teams');
		equal($($(selector)[0][1]).text(), 'London/Students');
		equal($($(selector)[0][2]).text(), 'London/Team Flexible');
		equal($($(selector)[0][3]).text(), 'London/Team Outbound');
		equal($($(selector)[0][4]).text(), 'London/Team Preferences');
		equal($($(selector)[0][5]).text(), 'London/Team Rotations');
		equal($($(selector)[0][10]).text(), 'Contract Group/Team Rotations');
	});

	test('should select default team', function () {
		initVm();

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

	test('should render timeline', function () {
		initVm();

		var html = ['<div class="mobile-timeline floatleft" data-bind="style: {height: scheduleContainerHeight + \'px\'}">',
			'	<!-- ko foreach: timeLines -->',
			'	<div class="mobile-timeline-label absolute" data-bind="style: {top: topPosition}, text: timeText, visible: isHour">',
			'	</div>',
			'	<!-- /ko -->',
			'</div>'].join("");

		$('.mobile-teamschedule-view').append(html);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		var labelSelector = '.mobile-teamschedule-view .mobile-timeline .mobile-timeline-label';
		equal($(labelSelector).length, 9);
		equal($(labelSelector)[0].innerText, '05:00');
		equal($(labelSelector)[1].innerText, '06:00');
		equal($(labelSelector)[2].innerText, '07:00');
		equal($(labelSelector)[3].innerText, '08:00');
	});

	test('should render overnight timeline', function () {
		initVm();

		var html = ['<div class="mobile-timeline floatleft" data-bind="style: {height: scheduleContainerHeight + \'px\'}">',
			'	<!-- ko foreach: timeLines -->',
			'	<div class="mobile-timeline-label absolute" data-bind="style: {top: topPosition}, text: timeText, visible: isHour">',
			'	</div>',
			'	<!-- /ko -->',
			'</div>'].join("");

		$('.mobile-teamschedule-view').append(html);

		vm = Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.Vm();

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		var labelSelector = '.mobile-teamschedule-view .mobile-timeline .mobile-timeline-label:visible';

		equal($(labelSelector)[4].innerText, '23:00');
		equal($(labelSelector)[5].innerText, '00:00');
		equal($(labelSelector)[6].innerText, '01:00');
		equal($(labelSelector)[7].innerText, '02:00');
		
	});

	test('should render my schedule', function () {
		initVm();

		$('.mobile-teamschedule-view').append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		equal($('.mobile-teamschedule-view .mobile-teamschedule-agent-name.my-name span').text(), '@Resources.MySchedule');
		equal($('.mobile-teamschedule-view .my-schedule-column .mobile-schedule-layer').length, 1);
		equal($('.mobile-teamschedule-view .my-schedule-column .mobile-schedule-layer strong').text(), 'Phone');
	});

	test('should render teammates\'s schedules', function () {
		initVm();

		$('.mobile-teamschedule-view').append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		equal($('.teammates-agent-name-row .mobile-teamschedule-agent-name:nth-child(1) .text-name').text(), 'Jon Kleinsmith1');
		equal($('.mobile-teamschedule-view .teammates-schedules-column .mobile-schedule-layer').length, 10);
		equal($('.mobile-teamschedule-view .teammates-schedules-column .mobile-schedule-layer strong:visible').length, 0);
	});

	test('should reload schedule after clicking on "previous day" icon', function () {
		initVm();

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

		equal(fetchTeamScheduleDataRequestCount, 2);
	});

	test('should reload schedule after clicking on "next day" icon', function () {
		initVm();

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

		equal(fetchTeamScheduleDataRequestCount, 2);
	});

	test('should reset skip count after changing selected date', function () {
		initVm();

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
		vm.paging.skip = 10;

		vm.selectedDate(today.add(9, 'days'));

		equal(vm.paging.skip, 0);
	});

	test('should reset skip count after clicking on "previous day" icon', function () {
		initVm();
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
		vm.paging.skip = 10;
		$('.previous-day').click();

		equal(vm.paging.skip, 0);
	});

	test('should reset skip count after clicking on "next day" icon', function () {
		initVm();

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
		vm.paging.skip = 10;
		$('.next-day').click();

		equal(vm.paging.skip, 0);
	});

	test('should filter agent schedules', function () {
		initVm();

		var html = [
			'<div class="teamschedule-filter-component">',
			'	<p>',
			'		<b>@Resources.Team: </b>',
			'	</p>',
			'	<select data-bind="foreach: availableTeams, select2: { value: selectedTeam } ">',
			'		<optgroup data-bind="attr: { label: text }, foreach: children">',
			'			<option data-bind="text: text, value: id"></option>',
			'		</optgroup>',
			'	</select>',
			'</div>',
			'<div class="teamschedule-search-component">',
			'	<p>',
			'		<b>@Resources.AgentName: </b>',
			'	</p>',
			'	<form data-bind="submit: submitSearchForm">',
			'		<input type="search" class="form-control" placeholder=\'@Resources.SearchHintForName\' data-bind="value: searchNameText" />',
			'		<input type="submit" style="display: none"/>',
			'	</form>',
			'</div>',
			'<div class="mobile-teamschedule-submit-buttons">',
			'	<button class="btn btn-default pull-right" data-bind="click: toggleFilterPanel">@Resources.Cancel</button>',
			'	<button class="btn btn-primary pull-left" data-bind="click: submitSearchForm">@Resources.Search</button>',
			'</div>'].join("");

		$('.mobile-teamschedule-view').append(html).append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		$('.mobile-teamschedule-view input.form-control').val('Kleinsmith10');
		$('.mobile-teamschedule-view input.form-control').change();
		$('.mobile-teamschedule-submit-buttons button.btn-primary').click();

		equal(vm.currentPageNum(), 1);
		equal($('.teammates-agent-name-row .mobile-teamschedule-agent-name:nth-child(1) .text-name').text(), 'Jon Kleinsmith10');
	});

	test('should update agent names after loaded schedules', function () {
		initVm();

		$('.mobile-teamschedule-view').append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		equal($('.teammates-agent-name-row .mobile-teamschedule-agent-name:nth-child(1) .text-name').text(), 'Jon Kleinsmith1');
		equal($('.teammates-agent-name-row .mobile-teamschedule-agent-name:nth-child(10) .text-name').text(), 'Jon Kleinsmith10');
	});

	test('should update agent names after loaded more schedules', function () {
		initVm();

		$('.mobile-teamschedule-view').append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		vm.readMoreTeamScheduleData({
			AgentSchedules: fakeOriginalAgentSchedulesData.slice(10)
		});

		equal($('.teammates-agent-name-row .mobile-teamschedule-agent-name:nth-child(1) .text-name').text(), 'Jon Kleinsmith1');
		equal($('.teammates-agent-name-row .mobile-teamschedule-agent-name:nth-child(10) .text-name').text(), 'Jon Kleinsmith10');
		equal($('.teammates-agent-name-row .mobile-teamschedule-agent-name:nth-child(11) .text-name').text(), 'Jon Kleinsmith11');
		equal($('.teammates-agent-name-row .mobile-teamschedule-agent-name:nth-child(20) .text-name').text(), 'Jon Kleinsmith20');
	});

	test('should reset search name to empty after click cancel button in panel', function () {
		initVm();

		$('.mobile-teamschedule-view').append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		vm.toggleFilterPanel();

		vm.searchNameText('test search name text');

		$('.mobile-teamschedule-submit-buttons button:first-child').click();

		equal(vm.searchNameText(), '');
	});

	test('should reset search name text to last submitted value after click cancel button in panel', function () {
		initVm();

		$('.mobile-teamschedule-view').append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);
		vm.searchNameText('10');
		vm.submitSearchForm();

		vm.toggleFilterPanel();

		vm.searchNameText('test search name text');

		$('.mobile-teamschedule-submit-buttons button:first-child').click();

		equal(vm.searchNameText(), '10');
	});

	test('should reset to selected team default team after click cancel button in panel', function () {
		initVm();

		$('.mobile-teamschedule-view').append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		vm.toggleFilterPanel();

		vm.selectedTeam('d7a9c243-8cd8-406e-9889-9b5e015ab495');

		$('.mobile-teamschedule-submit-buttons button:first-child').click();

		equal(vm.selectedTeam(), fakeDefaultTeamData.DefaultTeam);
	});

	test('should reset selected team to last submitted team after click cancel button in panel', function () {
		initVm();

		$('.mobile-teamschedule-view').append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		vm.selectedTeam('allTeams');
		vm.submitSearchForm();

		vm.toggleFilterPanel();

		vm.selectedTeam('a74e1f94-7662-4a7f-9746-a56e00a66f17');

		$('.mobile-teamschedule-submit-buttons button:first-child').click();

		equal(vm.selectedTeam(), 'allTeams');
	});

	test('should map dayOff of MySchedule', function () {
		initVm();
		var html = ['<!--ko if:mySchedule().isDayOff-->',
		'<div class="shift-trade-layer-container floatleft shift-trade-dayoff">',
		'<div class="dayoff" data-bind="text:mySchedule().dayOffName"></div>',
		'</div>',
		'<!--/ko-->'].join("");

		$('.mobile-teamschedule-view').append(html);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		equal(vm.mySchedule().isDayOff, true);
		equal(vm.mySchedule().dayOffName, 'Day off');
		equal($('.dayoff').text(), 'Day off');
	});

	
	test('should map dayOff of agents\' schedule', function () {
		initVm();

		$('.mobile-teamschedule-view').append(agentSchedulesHtml);

		ko.applyBindings(vm, $('.mobile-teamschedule-view')[0]);

		vm.paging.take = 100;
		vm.toggleFilterPanel();
		vm.searchNameText('dayoffAgent');
		$('.mobile-teamschedule-submit-buttons button.btn-primary').click();

		equal($('.teammates-schedules-container .dayoff-text:first').text(), 'Day off');
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

		fakeTeamScheduleData = {
			"AgentSchedules": [],
			"TimeLine": [
				{ "Time": "05:00:00", "TimeLineDisplay": "05:00", "PositionPercentage": 0.0714, "TimeFixedFormat": null },
				{ "Time": "06:00:00", "TimeLineDisplay": "06:00", "PositionPercentage": 0.3571, "TimeFixedFormat": null },
				{ "Time": "07:00:00", "TimeLineDisplay": "07:00", "PositionPercentage": 0.6429, "TimeFixedFormat": null },
				{ "Time": "08:00:00", "TimeLineDisplay": "08:00", "PositionPercentage": 0.9286, "TimeFixedFormat": null },
				{ "Time": "23:00:00", "TimeLineDisplay": "23:00", "PositionPercentage": 0.0714, "TimeFixedFormat": null },
				{ "Time": "1.00:00:00", "TimeLineDisplay": "00:00", "PositionPercentage": 0.3571, "TimeFixedFormat": null },
				{ "Time": "1.01:00:00", "TimeLineDisplay": "01:00", "PositionPercentage": 0.6429, "TimeFixedFormat": null },
				{ "Time": "1.02:00:00", "TimeLineDisplay": "02:00", "PositionPercentage": 0.6429, "TimeFixedFormat": null },
				{ "Time": "1.03:00:00", "TimeLineDisplay": "03:00", "PositionPercentage": 0.6429, "TimeFixedFormat": null },
			],
			"TimeLineLengthInMinutes": 210,
			"PageCount": 4,
			"MySchedule": {
				"Name": "Ashley Andeen",
				"StartTimeUtc": "2018-05-24T05:00:00",
				"PersonId": "b46a2588-8861-42e3-ab03-9b5e015b257c",
				"MinStart": null,
				"Total": 1,
				"DayOffName": 'Day off',
				"ContractTimeInMinute": 480.0,
				"Date": "",
				"FixedDate": "",
				"Header": "",
				"HasMainShift": "",
				"HasOvertime": "",
				"IsFullDayAbsence": false,
				"IsDayOff": true,
				"Summary": "",
				"Periods": [
					{
						"Title": "Phone",
						"TimeSpan": "05:00 - 06:45",
						"StartTime": "2018-05-24T05:00:00",
						"EndTime": "2018-05-24T06:45:00",
						"Summary": "",
						"StyleClassName": "",
						"Meeting": "",
						"StartPositionPercentage": "",
						"EndPositionPercentage": "",
						"Color": "#80FF80",
						"IsOvertime": false,
						"IsAbsenceConfidential": false,
						"TitleTime": "05:00 - 06:45"
					}
				],
				"DayOfWeekNumber": "",
				"HasNotScheduled": ""
			}
		};
		
		if (fakeOriginalAgentSchedulesData == undefined || fakeOriginalAgentSchedulesData == null) {
			fakeOriginalAgentSchedulesData = [];
			for (var i = 0; i < 20; i++) {
				var agentSchedule = {
					"Name": "Jon Kleinsmith" + (i + 1),
					"StartTimeUtc": "2018-05-24T05:00:00",
					"PersonId": "a74e1f94-6331-4a7f-9746-9b5e015b257c",
					"MinStart": null,
					"Total": 1,
					"DayOffName": null,
					"ContractTimeInMinute": 480.0,
					"Date": "",
					"FixedDate": "",
					"Header": "",
					"HasMainShift": "",
					"HasOvertime": "",
					"IsFullDayAbsence": false,
					"IsDayOff": false,
					"Summary": "",
					"Periods": [
						{
							"Title": "Email",
							"TimeSpan": "05:00 - 06:45",
							"StartTime": "2018-05-24T05:00:00",
							"EndTime": "2018-05-24T06:45:00",
							"Summary": "",
							"StyleClassName": "",
							"Meeting": "",
							"StartPositionPercentage": "",
							"EndPositionPercentage": "",
							"Color": "#80FF80",
							"IsOvertime": false,
							"IsAbsenceConfidential": false,
							"TitleTime": "05:00 - 06:45"
						}
					],
					"DayOfWeekNumber": "",
					"HasNotScheduled": ""
				};
				
				fakeOriginalAgentSchedulesData.push(agentSchedule);
			}

			fakeOriginalAgentSchedulesData.push({
				"Name":"dayoffAgent",
				"IsDayOff": true,
				"DayOffName": "Day off",
				"Periods": []
			});
		}

	}

	function setupAjax() {
		ajax = {
			Ajax: function(option) {
				if (option.url === 'Team/TeamsAndGroupsWithAllTeam') {
					option.success(fakeAvailableTeamsData);
				}

				if (option.url === '../api/TeamSchedule/DefaultTeam') {
					option.success(fakeDefaultTeamData);
				}

				if (option.url === '../api/TeamSchedule/TeamSchedule') {
					fetchTeamScheduleDataRequestCount++;

					var data = JSON.parse(option.data);
					var skip = data.Paging.Skip;
					var take = data.Paging.Take;
					var pagedAgentSchedules = [];
					for (var i = skip; i < skip + take; i++) {
						if (fakeOriginalAgentSchedulesData[i]) {
							if (data.ScheduleFilter.searchNameText){
								if (fakeOriginalAgentSchedulesData[i].Name.indexOf(data.ScheduleFilter.searchNameText) > -1){
									pagedAgentSchedules.push(fakeOriginalAgentSchedulesData[i]);
								}
							}
							else{
								pagedAgentSchedules.push(fakeOriginalAgentSchedulesData[i]);
							}
						}
					}
					fakeTeamScheduleData.AgentSchedules = [];
					fakeTeamScheduleData.AgentSchedules = pagedAgentSchedules;
					option.success(fakeTeamScheduleData);
				}
			}
		};
	}

	function setUpHtml() {
		agentSchedulesHtml = [
			'<div class="mobile-teamschedule-view-body container">',
			'	<div class="mobile-timeline floatleft" data-bind="style: {height: scheduleContainerHeight() + \'px\'}">',
			'		<!-- ko foreach: timeLines -->',
			'		<div class="mobile-timeline-label absolute" data-bind="style: {top: topPosition}, text: timeText, visible: isHour">',
			'		</div>',
			'		<!-- /ko -->',
			'	</div>',
			'	<!-- ko if: mySchedule -->',
			'	<div class="mobile-teamschedule-agent-name my-name" data-bind="tooltip: { title: mySchedule().name, trigger: \'click\', placement: \'bottom\'}, hideTooltipAfterMouseLeave: true">',
			'		<span class="text-name">@Resources.MySchedule</span>',
			'		<span class="team-schedule-arrow-up"><i class="glyphicon glyphicon-chevron-up"></i></span>',
			'		<span class="team-schedule-arrow-down"><i class="glyphicon glyphicon-chevron-down"></i></span>',
			'	</div>',
			'	<!-- /ko -->',
			'	<div class="teammates-agent-name-row">',
			'		<!-- ko foreach: agentNames -->',
			'		<div class="mobile-teamschedule-agent-name" data-bind="tooltip: { title: $data, trigger: \'click\', placement: \'bottom\'}, hideTooltipAfterMouseLeave: true, adjustTooltipPositionOnMobileTeamSchedule: true">',
			'			<span class="text-name" data-bind="text: $data"></span>',
			'			<span class="team-schedule-arrow-up"><i class="glyphicon glyphicon-chevron-up"></i></span>',
			'			<span class="team-schedule-arrow-down"><i class="glyphicon glyphicon-chevron-down"></i></span>',
			'		</div>',
			'		<!-- /ko -->',
			'	</div>',
			'	<div class="my-schedule-column relative" data-bind="style: {height: scheduleContainerHeight() + \'px\'}">',
			'		<!-- ko if: mySchedule -->',
			'		<div class="mobile-schedule-container relative">',
			'			<!-- ko foreach: mySchedule().layers -->',
			'			<div class="mobile-schedule-layer absolute" data-bind="tooltip: { title: tooltipText, html: true, trigger: \'click\' }, style: styleJson, css:{\'overtime-background-image-light\': isOvertime, overTimeLighterBackgroundStyle, \'overtime-background-image-dark\': isOvertime, overTimeDarkerBackgroundStyle, \'last-layer\': isLastLayer}, hideTooltipAfterMouseLeave: true">',
			'				<div data-bind="visible: showTitle() && !isOvertimeAvailability()">',
			'					<strong data-bind="text: title()"></strong>',
			'					<!-- ko if: hasMeeting -->',
			'					<div class="meeting floatright">',
			'						<i class="meeting-icon mr10">',
			'							<i class="glyphicon glyphicon-user ml10"></i>',
			'						</i>',
			'					</div>',
			'					<!-- /ko -->',
			'					<span class="fullwidth displayblock" data-bind="visible: false && showDetail, text: timeSpan"></span>',
			'				</div>',
			'				<div data-bind="visible: false && showTitle() && isOvertimeAvailability()">',
			'					<i class="glyphicon glyphicon-time"></i>',
			'				</div>',
			'			</div>',
			'			<!-- /ko -->',
			'		</div>',
			'		<!-- /ko -->',
			'	</div>',
			'	<!-- ko if: teamSchedules -->',
			'	<div class="teammates-schedules-container" data-bind="style: {height: scheduleContainerHeight() + \'px\'}">',
			'		<!-- ko foreach: teamSchedules -->',
			'		<div class="teammates-schedules-column relative">',
			'			<div class="mobile-schedule-container relative">',
			'				<!-- ko foreach: layers -->',
			'				<div class="mobile-schedule-layer absolute" data-bind="tooltip: { title: tooltipText, html: true, trigger: \'click\' }, style: styleJson, css:{\'overtime-background-image-light\': isOvertime, overTimeLighterBackgroundStyle, \'overtime-background-image-dark\': isOvertime, overTimeDarkerBackgroundStyle, \'last-layer\': isLastLayer}, hideTooltipAfterMouseLeave: true, adjustTooltipPositionOnMobileTeamSchedule: true">',
			'					<div data-bind="visible: showTitle() && !isOvertimeAvailability()">',
			'						<strong data-bind="visible: false, text: title()"></strong>',
			'						<!-- ko if: hasMeeting -->',
			'						<div class="meeting floatright">',
			'							<i class="meeting-icon mr10">',
			'								<i class="glyphicon glyphicon-user ml10"></i>',
			'							</i>',
			'						</div>',
			'						<!-- /ko -->',
			'						<span class="fullwidth displayblock" data-bind="visible: false && showDetail, text: timeSpan"></span>',
			'					</div>',
			'					<div data-bind="visible: false && showTitle() && isOvertimeAvailability()">',
			'						<i class="glyphicon glyphicon-time"></i>',
			'					</div>',
			'				</div>',
			'				<!-- /ko -->',
			'			</div>',
			'				<!--ko if:isDayOff-->',
			'				<div class=\"dayoff\">',
			'					<strong class="dayoff-text" data-bind="text: dayOffName"></strong>',
			'				</div>',
			'				<!-- /ko -->',
			'		</div>',
			'		<!-- /ko -->',
			'	</div>',
			'	<!-- /ko -->',
			'	<!-- ko if: isPanelVisible -->',
			'	<div class="mobile-teamschedule-panel">',
			'		<div class="teamschedule-filter-component">',
			'			<p>',
			'				<b>@Resources.Team: </b>',
			'			</p>',
			'			<select data-bind="foreach: availableTeams, select2: { value: selectedTeam, minimumResultsForSearch: \'Infinity\'}" id="team-selection">',
			'				<optgroup data-bind="attr: { label: text }, foreach: children">',
			'					<option data-bind="text: text, value: id"></option>',
			'				</optgroup>',
			'			</select>',
			'		</div>',
			'		<div class="teamschedule-search-component">',
			'			<p>',
			'				<b>@Resources.AgentName: </b>',
			'			</p>',
			'			<form data-bind="submit: submitSearchForm">',
			'				<input type="search" class="form-control" placeholder=\'@Resources.SearchHintForName\' data-bind="value: searchNameText" />',
			'				<input type="submit" style="display: none"/>',
			'			</form>',
			'		</div>',
			'		<div class="empty-search-result">',
			'			<!-- ko if: emptySearchResult -->',
			'			<p><b>@Resources.NoResultForCurrentFilter</b></p>',
			'			<!--/ko-->',
			'		</div>',
			'		<div class="mobile-teamschedule-submit-buttons">',
			'			<button class="btn btn-default" data-bind="click: cancelClick">@Resources.Cancel</button>',
			'			<button class="btn btn-primary" data-bind="click: submitSearchForm">@Resources.Search</button>',
			'		</div>',
			'	</div>',
			'	<!--/ko-->',
			'</div>'].join("");
	}

	function setup() {
		fetchTeamScheduleDataRequestCount = 0;
		completeLoadedCount = 0;
		setUpHtml();
		setUpFakeData();
		setupAjax();

		Teleopti.MyTimeWeb.Common.DateFormat = dateFormat;

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
	}

	function initVm() {
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
