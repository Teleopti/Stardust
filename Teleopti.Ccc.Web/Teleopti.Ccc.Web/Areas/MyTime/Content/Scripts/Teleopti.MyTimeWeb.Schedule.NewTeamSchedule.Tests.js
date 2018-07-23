$(document).ready(function() {
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
			Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(t) {
				if (t == 'MyTimeWeb_NewTeamScheduleView_75989' || t == 'MyTimeWeb_NewTeamScheduleViewDesktop_76313')
					return true;
			};
			setup();
		},
		teardown: function() {
			$('.new-teamschedule-view').remove();
			Teleopti.MyTimeWeb.Common.DateFormat = null;
		}
	});

	test('should select today as selectedDate(displayDate)', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		var today = moment().format(dateFormat);

		equal(vm.selectedDate().format(dateFormat), today);
		equal($('.new-teamschedule-view .mobile-datepicker a.formatted-date-text').text(), today);
	});

	test('should change selected date after clicking on "previous day" icon', function() {
		$('body').append(agentSchedulesHtml);
		initVm();
		var today = moment();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);
		$('.previous-day').click();

		var expectDateStr = today.add(-1, 'days').format(dateFormat);

		equal(vm.selectedDate().format(dateFormat), expectDateStr);
		equal($('.new-teamschedule-view .mobile-datepicker a.formatted-date-text').text(), expectDateStr);
	});

	test('should change selected date after clicking on "next day" icon', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		var today = moment();
		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);
		$('.next-day').click();

		var expectDateStr = today.add(1, 'days').format(dateFormat);

		equal(vm.selectedDate().format(dateFormat), expectDateStr);
		equal($('.new-teamschedule-view .mobile-datepicker a.formatted-date-text').text(), expectDateStr);
	});

	test('should change display date after changing selected date', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		var today = moment();
		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		vm.selectedDate(today.add(9, 'days'));

		var expectDateStr = vm.selectedDate().format(dateFormat);

		equal($('.new-teamschedule-view .mobile-datepicker a.formatted-date-text').text(), expectDateStr);
	});

	test('should render sites and teams', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		$('.new-teamschedule-filter').click();

		var selector = '.new-teamschedule-view .teamschedule-filter-component select';
		equal($(selector)[0].length, 11);

		equal($($(selector)[0][0]).text(), 'All Teams');
		equal($($(selector)[0][1]).text(), 'London/Students');
		equal($($(selector)[0][2]).text(), 'London/Team Flexible');
		equal($($(selector)[0][3]).text(), 'London/Team Outbound');
		equal($($(selector)[0][4]).text(), 'London/Team Preferences');
		equal($($(selector)[0][5]).text(), 'London/Team Rotations');
		equal($($(selector)[0][10]).text(), 'Contract Group/Team Rotations');
	});

	test('should select default team', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		$('.new-teamschedule-filter').click();

		equal(completeLoadedCount, 1);
		equal($('.new-teamschedule-view .teamschedule-filter-component select').val(), fakeDefaultTeamData.DefaultTeam);
	});

	test('should render timeline', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		var labelSelector = '.new-teamschedule-view .mobile-timeline .mobile-timeline-label';
		equal($(labelSelector).length, 32);
		equal($(labelSelector)[0].innerText, '23:00 -1');
		equal($(labelSelector)[1].innerText, '00:00');
		equal($(labelSelector)[2].innerText, '01:00');
		equal($(labelSelector)[3].innerText, '02:00');
		equal($(labelSelector)[30].innerText, '05:00 +1');
		equal($(labelSelector)[31].innerText, '06:00 +1');
	});

	test('should update timeline after load more schedules', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		vm.readMoreTeamScheduleData({
			AgentSchedules: [],
			TimeLine: [
				{
					Time: '2018-07-23T08:00:00',
					TimeLineDisplay: '23/07/2018T08:00',
					PositionPercentage: 0.0,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T09:00:00',
					TimeLineDisplay: '23/07/2018T09:00',
					PositionPercentage: 0.0412,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T10:00:00',
					TimeLineDisplay: '23/07/2018T10:00',
					PositionPercentage: 0.0825,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T11:00:00',
					TimeLineDisplay: '23/07/2018T11:00',
					PositionPercentage: 0.1237,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T12:00:00',
					TimeLineDisplay: '23/07/2018T12:00',
					PositionPercentage: 0.1649,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T13:00:00',
					TimeLineDisplay: '23/07/2018T13:00',
					PositionPercentage: 0.2062,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T14:00:00',
					TimeLineDisplay: '23/07/2018T14:00',
					PositionPercentage: 0.2474,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T15:00:00',
					TimeLineDisplay: '23/07/2018T15:00',
					PositionPercentage: 0.2887,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T16:00:00',
					TimeLineDisplay: '23/07/2018T16:00',
					PositionPercentage: 0.3299,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T17:00:00',
					TimeLineDisplay: '23/07/2018T17:00',
					PositionPercentage: 0.3711,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T18:00:00',
					TimeLineDisplay: '23/07/2018T18:00',
					PositionPercentage: 0.4124,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T19:00:00',
					TimeLineDisplay: '23/07/2018T19:00',
					PositionPercentage: 0.4536,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T20:00:00',
					TimeLineDisplay: '23/07/2018T20:00',
					PositionPercentage: 0.4948,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T21:00:00',
					TimeLineDisplay: '23/07/2018T21:00',
					PositionPercentage: 0.5361,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T22:00:00',
					TimeLineDisplay: '23/07/2018T22:00',
					PositionPercentage: 0.5773,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T23:00:00',
					TimeLineDisplay: '23/07/2018T23:00',
					PositionPercentage: 0.6186,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T00:00:00',
					TimeLineDisplay: '24/07/2018T00:00',
					PositionPercentage: 0.6598,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T01:00:00',
					TimeLineDisplay: '24/07/2018T01:00',
					PositionPercentage: 0.701,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T02:00:00',
					TimeLineDisplay: '24/07/2018T02:00',
					PositionPercentage: 0.7423,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T03:00:00',
					TimeLineDisplay: '24/07/2018T03:00',
					PositionPercentage: 0.7835,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T04:00:00',
					TimeLineDisplay: '24/07/2018T04:00',
					PositionPercentage: 0.8247,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T05:00:00',
					TimeLineDisplay: '24/07/2018T05:00',
					PositionPercentage: 0.866,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T06:00:00',
					TimeLineDisplay: '24/07/2018T06:00',
					PositionPercentage: 0.9072,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T07:00:00',
					TimeLineDisplay: '24/07/2018T07:00',
					PositionPercentage: 0.9485,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T08:00:00',
					TimeLineDisplay: '24/07/2018T08:00',
					PositionPercentage: 0.9897,
					TimeFixedFormat: null
				}
			]
		});

		var labelSelector = '.new-teamschedule-view .mobile-timeline .mobile-timeline-label';
		equal($(labelSelector).length, 34);
		equal($(labelSelector)[0].innerText, '23:00 -1');
		equal($(labelSelector)[1].innerText, '00:00');
		equal($(labelSelector)[2].innerText, '01:00');
		equal($(labelSelector)[32].innerText, '07:00 +1');
		equal($(labelSelector)[33].innerText, '08:00 +1');
	});

	test('should render overnight timeline', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		vm = Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.Vm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		var labelSelector = '.new-teamschedule-view .mobile-timeline .mobile-timeline-label:visible';
		equal($(labelSelector).length, 32);
		equal($(labelSelector)[0].innerText, '23:00 -1');
		equal($(labelSelector)[1].innerText, '00:00');
		equal($(labelSelector)[2].innerText, '01:00');
		equal($(labelSelector)[3].innerText, '02:00');
		equal($(labelSelector)[30].innerText, '05:00 +1');
		equal($(labelSelector)[31].innerText, '06:00 +1');
	});

	test('should render my schedule', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		equal(
			$('.new-teamschedule-view .new-teamschedule-agent-name.my-name .text-name').text(),
			'@Resources.MySchedule'
		);
		equal($('.new-teamschedule-view .my-schedule-column .new-teamschedule-layer').length, 1);
		equal($('.new-teamschedule-view .my-schedule-column .new-teamschedule-layer strong').text(), 'Phone');
	});

	test("should render teammates's schedules", function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		equal(
			$('.teammates-agent-name-row .new-teamschedule-agent-name:nth-child(1) .text-name').text(),
			'Jon Kleinsmith1'
		);
		equal($('.new-teamschedule-view .teammates-schedules-column .new-teamschedule-layer').length, 20);
		equal($('.new-teamschedule-view .teammates-schedules-column .new-teamschedule-layer strong:visible').length, 0);
	});

	test('should show shift category for my schedule', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		equal($('.new-teamschedule-view .new-teamschedule-agent-name.my-name span.shift-category-cell').text(), 'AM');
	});

	test('should show day off short name for my schedule', function() {
		fakeTeamScheduleData.MySchedule.ShiftCategory.Name = 'Day Off';
		fakeTeamScheduleData.MySchedule.ShiftCategory.ShortName = 'DO';

		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		equal($('.new-teamschedule-view .new-teamschedule-agent-name.my-name span.shift-category-cell').text(), 'DO');
	});

	test('should reload schedule after clicking on "previous day" icon', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);
		$('.previous-day').click();

		equal(fetchTeamScheduleDataRequestCount, 2);
	});

	test('should reload schedule after clicking on "next day" icon', function() {
		$('body').append(agentSchedulesHtml);
		initVm();
		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);
		$('.next-day').click();

		equal(fetchTeamScheduleDataRequestCount, 2);
	});

	test('should reset skip count after changing selected date', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		var today = moment();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);
		vm.paging.skip = 10;

		vm.selectedDate(today.add(9, 'days'));

		equal(vm.paging.skip, 0);
	});

	test('should reset skip count after clicking on "previous day" icon', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);
		vm.paging.skip = 10;
		$('.previous-day').click();

		equal(vm.paging.skip, 0);
	});

	test('should reset skip count after clicking on "next day" icon', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);
		vm.paging.skip = 10;
		$('.next-day').click();

		equal(vm.paging.skip, 0);
	});

	test('should filter agent schedules', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		$('.new-teamschedule-filter').click();

		$('.new-teamschedule-view input.form-control').val('Kleinsmith5');
		$('.new-teamschedule-view input.form-control').change();
		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal(vm.currentPageNum(), 1);
		equal(
			$('.teammates-agent-name-row .new-teamschedule-agent-name:nth-child(1) .text-name').text(),
			'Jon Kleinsmith5'
		);
	});

	test('should update agent names after loaded schedules', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		equal(
			$('.teammates-agent-name-row .new-teamschedule-agent-name:nth-child(1) .text-name').text(),
			'Jon Kleinsmith1'
		);
		equal(
			$('.teammates-agent-name-row .new-teamschedule-agent-name:nth-child(10) .text-name').text(),
			'Jon Kleinsmith10'
		);
	});

	test('should update agent names after loaded more schedules', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		vm.readMoreTeamScheduleData({
			AgentSchedules: fakeOriginalAgentSchedulesData.slice(10),
			TimeLine: [
				{ Time: '05:00:00', TimeLineDisplay: '05:00', PositionPercentage: 0.0714, TimeFixedFormat: null },
				{ Time: '06:00:00', TimeLineDisplay: '06:00', PositionPercentage: 0.3571, TimeFixedFormat: null }
			]
		});

		equal(
			$('.teammates-agent-name-row .new-teamschedule-agent-name:nth-child(1) .text-name').text(),
			'Jon Kleinsmith1'
		);
		equal(
			$('.teammates-agent-name-row .new-teamschedule-agent-name:nth-child(10) .text-name').text(),
			'Jon Kleinsmith10'
		);
		equal(
			$('.teammates-agent-name-row .new-teamschedule-agent-name:nth-child(11) .text-name').text(),
			'Jon Kleinsmith11'
		);
		equal(
			$('.teammates-agent-name-row .new-teamschedule-agent-name:nth-child(20) .text-name').text(),
			'Jon Kleinsmith20'
		);
	});

	test('should reset search name to empty after click cancel button in panel', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		vm.toggleFilterPanel();

		vm.searchNameText('test search name text');

		$('.new-teamschedule-submit-buttons button:first-child').click();

		equal(vm.searchNameText(), '');
	});

	test('should reset search name text to last submitted value after click cancel button in panel', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);
		vm.searchNameText('10');
		vm.submitSearchForm();

		vm.toggleFilterPanel();

		vm.searchNameText('test search name text');

		$('.new-teamschedule-submit-buttons button:first-child').click();

		equal(vm.searchNameText(), '10');
	});

	test('should reset to selected team default team after click cancel button in panel', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		vm.toggleFilterPanel();

		vm.selectedTeam('d7a9c243-8cd8-406e-9889-9b5e015ab495');

		$('.new-teamschedule-submit-buttons button:first-child').click();

		equal(vm.selectedTeam(), fakeDefaultTeamData.DefaultTeam);
	});

	test('should reset selected team to last submitted team after click cancel button in panel', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		vm.selectedTeam('allTeams');
		vm.submitSearchForm();

		vm.toggleFilterPanel();

		vm.selectedTeam('a74e1f94-7662-4a7f-9746-a56e00a66f17');

		$('.new-teamschedule-submit-buttons button:first-child').click();

		equal(vm.selectedTeam(), 'allTeams');
	});

	test('should map dayOff of MySchedule', function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		equal(vm.mySchedule().isDayOff, true);
		equal(vm.mySchedule().dayOffName, 'Day off');
		equal(
			$('.my-schedule-column .new-teamschedule-layer-container .dayoff')
				.text()
				.replace(/(^\s*)|(\s*$)/g, ''),
			'Day off'
		);
	});

	test('should map notScheduled of MySchedule', function() {
		$('body').append(agentSchedulesHtml);
		fakeTeamScheduleData.MySchedule.IsNotScheduled = true;
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		equal(vm.mySchedule().isNotScheduled, true);
		equal($('.my-schedule-column .not-scheduled-text').text(), '@Resources.NotScheduled');
	});

	test("should map dayOff of agents' schedule", function() {
		$('body').append(agentSchedulesHtml);
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		vm.paging.take = 100;
		vm.toggleFilterPanel();
		vm.searchNameText('dayoffAgent');
		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal($('.teammates-schedules-container .dayoff').length > 0, true);
	});

	test("should map notScheduled of agents' schedule", function() {
		$('body').append(agentSchedulesHtml);
		if (fakeOriginalAgentSchedulesData.length > 0) {
			fakeOriginalAgentSchedulesData[0].IsNotScheduled = true;
		}
		initVm();

		ko.applyBindings(vm, $('.new-teamschedule-view')[0]);

		equal(vm.teamSchedules()[0].isNotScheduled, true);
		equal(
			$('.teammates-schedules-container .not-scheduled-text')
				.first()
				.text(),
			'@Resources.NotScheduled'
		);
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
			AgentSchedules: [],
			TimeLine: [
				{
					Time: '2018-07-22T23:00:00',
					TimeLineDisplay: '22/07/2018T23:00',
					PositionPercentage: 0.0,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T00:00:00',
					TimeLineDisplay: '23/07/2018T00:00',
					PositionPercentage: 0.032,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T01:00:00',
					TimeLineDisplay: '23/07/2018T01:00',
					PositionPercentage: 0.064,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T02:00:00',
					TimeLineDisplay: '23/07/2018T02:00',
					PositionPercentage: 0.096,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T03:00:00',
					TimeLineDisplay: '23/07/2018T03:00',
					PositionPercentage: 0.128,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T04:00:00',
					TimeLineDisplay: '23/07/2018T04:00',
					PositionPercentage: 0.16,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T05:00:00',
					TimeLineDisplay: '23/07/2018T05:00',
					PositionPercentage: 0.192,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T06:00:00',
					TimeLineDisplay: '23/07/2018T06:00',
					PositionPercentage: 0.224,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T07:00:00',
					TimeLineDisplay: '23/07/2018T07:00',
					PositionPercentage: 0.256,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T08:00:00',
					TimeLineDisplay: '23/07/2018T08:00',
					PositionPercentage: 0.288,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T09:00:00',
					TimeLineDisplay: '23/07/2018T09:00',
					PositionPercentage: 0.32,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T10:00:00',
					TimeLineDisplay: '23/07/2018T10:00',
					PositionPercentage: 0.352,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T11:00:00',
					TimeLineDisplay: '23/07/2018T11:00',
					PositionPercentage: 0.384,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T12:00:00',
					TimeLineDisplay: '23/07/2018T12:00',
					PositionPercentage: 0.416,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T13:00:00',
					TimeLineDisplay: '23/07/2018T13:00',
					PositionPercentage: 0.448,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T14:00:00',
					TimeLineDisplay: '23/07/2018T14:00',
					PositionPercentage: 0.48,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T15:00:00',
					TimeLineDisplay: '23/07/2018T15:00',
					PositionPercentage: 0.512,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T16:00:00',
					TimeLineDisplay: '23/07/2018T16:00',
					PositionPercentage: 0.544,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T17:00:00',
					TimeLineDisplay: '23/07/2018T17:00',
					PositionPercentage: 0.576,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T18:00:00',
					TimeLineDisplay: '23/07/2018T18:00',
					PositionPercentage: 0.608,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T19:00:00',
					TimeLineDisplay: '23/07/2018T19:00',
					PositionPercentage: 0.64,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T20:00:00',
					TimeLineDisplay: '23/07/2018T20:00',
					PositionPercentage: 0.672,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T21:00:00',
					TimeLineDisplay: '23/07/2018T21:00',
					PositionPercentage: 0.704,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T22:00:00',
					TimeLineDisplay: '23/07/2018T22:00',
					PositionPercentage: 0.736,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-23T23:00:00',
					TimeLineDisplay: '23/07/2018T23:00',
					PositionPercentage: 0.768,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T00:00:00',
					TimeLineDisplay: '24/07/2018T00:00',
					PositionPercentage: 0.8,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T01:00:00',
					TimeLineDisplay: '24/07/2018T01:00',
					PositionPercentage: 0.832,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T02:00:00',
					TimeLineDisplay: '24/07/2018T02:00',
					PositionPercentage: 0.864,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T03:00:00',
					TimeLineDisplay: '24/07/2018T03:00',
					PositionPercentage: 0.896,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T04:00:00',
					TimeLineDisplay: '24/07/2018T04:00',
					PositionPercentage: 0.928,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T05:00:00',
					TimeLineDisplay: '24/07/2018T05:00',
					PositionPercentage: 0.96,
					TimeFixedFormat: null
				},
				{
					Time: '2018-07-24T06:00:00',
					TimeLineDisplay: '24/07/2018T06:00',
					PositionPercentage: 0.992,
					TimeFixedFormat: null
				}
			],
			TimeLineLengthInMinutes: 210,
			PageCount: 4,
			MySchedule: {
				Name: 'Ashley Andeen',
				StartTimeUtc: '2018-05-24T05:00:00',
				PersonId: 'b46a2588-8861-42e3-ab03-9b5e015b257c',
				MinStart: null,
				Total: 1,
				DayOffName: 'Day off',
				ContractTimeInMinute: 480.0,
				Date: '',
				FixedDate: '',
				Header: '',
				HasMainShift: '',
				HasOvertime: '',
				IsFullDayAbsence: false,
				IsDayOff: true,
				IsNotScheduled: false,
				Summary: '',
				Periods: [
					{
						Title: 'Phone',
						TimeSpan: '05:00 - 06:45',
						StartTime: '2018-05-24T05:00:00',
						EndTime: '2018-05-24T06:45:00',
						Summary: '',
						StyleClassName: '',
						Meeting: '',
						StartPositionPercentage: '',
						EndPositionPercentage: '',
						Color: '#80FF80',
						IsOvertime: false,
						IsAbsenceConfidential: false,
						TitleTime: '05:00 - 06:45'
					}
				],
				DayOfWeekNumber: '',
				HasNotScheduled: '',
				ShiftCategory: {
					DisplayColor: '#80FF80',
					Id: null,
					Name: 'Early',
					ShortName: 'AM'
				}
			}
		};

		if (fakeOriginalAgentSchedulesData == undefined || fakeOriginalAgentSchedulesData == null) {
			fakeOriginalAgentSchedulesData = [];
			for (var i = 0; i < 20; i++) {
				var agentSchedule = {
					Name: 'Jon Kleinsmith' + (i + 1),
					StartTimeUtc: '2018-05-24T05:00:00',
					PersonId: 'a74e1f94-6331-4a7f-9746-9b5e015b257c',
					MinStart: null,
					Total: 1,
					DayOffName: null,
					ContractTimeInMinute: 480.0,
					Date: '',
					FixedDate: '',
					Header: '',
					HasMainShift: '',
					HasOvertime: '',
					IsFullDayAbsence: false,
					IsDayOff: false,
					IsNotScheduled: false,
					Summary: '',
					Periods: [
						{
							Title: 'Email',
							TimeSpan: '05:00 - 06:45',
							StartTime: '2018-05-24T05:00:00',
							EndTime: '2018-05-24T06:45:00',
							Summary: '',
							StyleClassName: '',
							Meeting: '',
							StartPositionPercentage: '',
							EndPositionPercentage: '',
							Color: '#80FF80',
							IsOvertime: false,
							IsAbsenceConfidential: false,
							TitleTime: '05:00 - 06:45'
						}
					],
					DayOfWeekNumber: '',
					HasNotScheduled: '',
					ShiftCategory: {
						DisplayColor: '#80FF80',
						Id: null,
						Name: 'Early',
						ShortName: 'AM'
					}
				};

				fakeOriginalAgentSchedulesData.push(agentSchedule);
			}

			fakeOriginalAgentSchedulesData.push({
				Name: 'dayoffAgent',
				IsDayOff: true,
				DayOffName: 'Day off',
				Periods: []
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
							if (data.ScheduleFilter.searchNameText) {
								if (
									fakeOriginalAgentSchedulesData[i].Name.indexOf(data.ScheduleFilter.searchNameText) >
									-1
								) {
									pagedAgentSchedules.push(fakeOriginalAgentSchedulesData[i]);
								}
							} else {
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
			'<div class="new-teamschedule-view relative" data-bind="style: {display: isMobileEnabled() || isDesktopEnabled() ? \'block\': \'none\'}">',
			'	<div class="navbar navbar-teleopti subnavbar">',
			'		<div class="container new-teamschedule-view-nav">',
			'			<ul class="nav navbar-nav navbar-teleopti row submenu">',
			'				<li class="mobile-datepicker">',
			'					<div class="input-group">',
			'						<span class="input-group-btn">',
			'							<button class="btn btn-default previous-day" data-bind="click: previousDay">',
			'								<i class="glyphicon glyphicon-chevron-left"></i>',
			'							</button>',
			'						</span>',
			'						<a class="text-center formatted-date-text moment-datepicker"></a>',
			'						<span class="input-group-btn">',
			'							<button class="btn btn-default next-day" data-bind="click: nextDay">',
			'								<i class="glyphicon glyphicon-chevron-right"></i>',
			'							</button>',
			'						</span>',
			'					</div>',
			'				</li>',
			'				<li class="mobile-today" data-bind="click: today">',
			'					<a>',
			'						<i class="glyphicon glyphicon-home"></i>',
			'					</a>',
			'				</li>',
			'				<li class="new-teamschedule-filter" data-bind="click: toggleFilterPanel">',
			'					<a>',
			'						<i class="glyphicon glyphicon-filter" data-bind="style: {color: hasFiltered() ? \'yellow\' : \'white\'}"></i>',
			'					</a>',
			'				</li>',
			'			</ul>',
			'		</div>',
			'	</div>',
			'	<div class="container relative">',
			'		<div class="agent-name-row container relative">',
			'			<!-- ko if: mySchedule -->',
			'			<div class="timeline-margin-block"></div>',
			'			<div class="new-teamschedule-agent-name my-name relative">',
			'				<span class="text-name">@Resources.MySchedule</span>',
			'				<span class="shift-category-cell" data-bind="text: mySchedule().shiftCategory.name, style: {background: mySchedule().shiftCategory.bgColor, color: mySchedule().shiftCategory.color}, css: {\'dayoff\': mySchedule().isDayOff}">',
			'				</span>',
			'			</div>',
			'			<!-- /ko -->',
			'			<div class="teammates-agent-name-row relative">',
			'				<!-- ko foreach: agentNames -->',
			'				<span class="new-teamschedule-agent-name" data-bind="tooltip: { title: $data.name, trigger: \'click\', placement: \'bottom\'}, hideTooltipAfterMouseLeave: true, adjustTooltipPositionOnMobileTeamSchedule: true">',
			'					<span class="text-name" data-bind="text: $data.name"></span>',
			'					<span class="shift-category-cell" data-bind="text: $data.shiftCategory.name, style: {background: $data.shiftCategory.bgColor, color: $data.shiftCategory.color}, css: {\'dayoff\': $data.isDayOff}"></span>',
			'				</span>',
			'				<!-- /ko -->',
			'			</div>',
			'		</div>',
			'		<div class="new-teamschedule-table relative container">',
			'			<div class="mobile-timeline" data-bind="style: {height: scheduleContainerHeight() + \'px\'}">',
			'				<!-- ko foreach: timeLines -->',
			'				<div class="mobile-timeline-label absolute" data-bind="style: {top: topPosition}, text: timeText, visible: isHour">',
			'				</div>',
			'				<!-- /ko -->',
			'			</div>',
			'			<div class="my-schedule-column relative" data-bind="style: {height: scheduleContainerHeight() + \'px\'}">',
			'				<!-- ko if: mySchedule -->',
			'				<div class="new-teamschedule-layer-container relative">',
			'					<!-- ko foreach: mySchedule().layers -->',
			"					<div class=\"new-teamschedule-layer cursorpointer absolute\" data-bind=\"tooltip: { title: tooltipText, html: true, trigger: 'click' }, style: styleJson, css:{'overtime-background-image-light': isOvertime && overTimeLighterBackgroundStyle(), 'overtime-background-image-dark': isOvertime && overTimeDarkerBackgroundStyle(), 'last-layer': isLastLayer}, hideTooltipAfterMouseLeave: true\">",
			'						<div class="activity-info" data-bind="visible: showTitle() && !isOvertimeAvailability()">',
			'							<strong data-bind="text: title()"></strong>',
			'							<!-- ko if: hasMeeting -->',
			'							<div class="meeting floatright">',
			'								<i class="meeting-icon mr10">',
			'									<i class="glyphicon glyphicon-user ml10"></i>',
			'								</i>',
			'							</div>',
			'							<!-- /ko -->',
			'							<span class="fullwidth displayblock" data-bind="visible: showDetail, text: timeSpan"></span>',
			'						</div>',
			'						<div data-bind="visible: showTitle() && isOvertimeAvailability()">',
			'							<i class="glyphicon glyphicon-time"></i>',
			'						</div>',
			'					</div>',
			'					<!-- /ko -->',
			'					<!--ko if:mySchedule().isDayOff-->',
			'					<div class="dayoff cursorpointer relative" data-bind="tooltip: { title: mySchedule().dayOffName, html: true, trigger: \'click\'}, hideTooltipAfterMouseLeave: true">',
			'						<span class="dayoff-text" data-bind="text: mySchedule().dayOffName"></span>',
			'					</div>',
			'					<!--/ko-->',
			'					<!-- ko if:mySchedule().isNotScheduled -->',
			'					<div class="not-scheduled-text">@Resources.NotScheduled</div>',
			'					<!-- /ko -->',
			'				</div>',
			'				<!-- /ko -->',
			'			</div>',
			'			<!-- ko if: teamSchedules -->',
			'			<div class="teammates-schedules-container relative" data-bind="style: {height: scheduleContainerHeight() + \'px\'}">',
			'				<!-- ko foreach: teamSchedules -->',
			'				<div class="teammates-schedules-column relative">',
			'					<div class="new-teamschedule-layer-container relative">',
			'						<!-- ko foreach: layers -->',
			"						<div class=\"new-teamschedule-layer cursorpointer absolute\" data-bind=\"tooltip: { title: tooltipText, html: true, trigger: 'click' }, style: styleJson, css:{'overtime-background-image-light': isOvertime && overTimeLighterBackgroundStyle(), 'overtime-background-image-dark': isOvertime && overTimeDarkerBackgroundStyle(), 'last-layer': isLastLayer}, hideTooltipAfterMouseLeave: true, adjustTooltipPositionOnMobileTeamSchedule: true\">",
			'							<div class="activity-info" data-bind="visible: showTitle() && !isOvertimeAvailability()">',
			'								<strong data-bind="text: title()"></strong>',
			'								<!-- ko if: hasMeeting -->',
			'								<div class="meeting floatright">',
			'									<i class="meeting-icon mr10">',
			'										<i class="glyphicon glyphicon-user ml10"></i>',
			'									</i>',
			'								</div>',
			'								<!-- /ko -->',
			'								<span class="fullwidth displayblock" data-bind="visible: showDetail, text: timeSpan"></span>',
			'							</div>',
			'							<div data-bind="visible: showTitle() && isOvertimeAvailability()">',
			'								<i class="glyphicon glyphicon-time"></i>',
			'							</div>',
			'						</div>',
			'						<!-- /ko -->',
			'						<!--ko if:isDayOff-->',
			'						<div class="dayoff cursorpointer relative" data-bind="tooltip: { title: dayOffName, html: true, trigger: \'click\' }, hideTooltipAfterMouseLeave: true, adjustTooltipPositionOnMobileTeamSchedule: true">',
			'							<span class="dayoff-text" data-bind="text: dayOffName"></span>',
			'						</div>',
			'						<!--/ko-->',
			'						<!-- ko if:isNotScheduled -->',
			'						<div class="not-scheduled-text">@Resources.NotScheduled</div>',
			'						<!-- /ko -->',
			'					</div>',
			'				</div>',
			'				<!-- /ko -->',
			'			</div>',
			'			<!-- /ko -->',
			'		</div>',
			'		<div class="teamschedule-scroll-block-container" data-bind="style: {border: isScrollbarVisible() ? \'1px dashed rgba(0, 0, 0, 0.1)\' : \'none\'}">',
			'			<!-- ko if: isScrollbarVisible -->',
			'			<div class="teamschedule-scroll-block">',
			'				<i class="glyphicon glyphicon-resize-horizontal"></i>',
			'			</div>',
			'			<!-- /ko -->',
			'		</div>',
			'		<!-- ko if: isPanelVisible -->',
			'		<div class="new-teamschedule-panel">',
			'			<div class="teamschedule-filter-component">',
			'				<p>',
			'					<b>@Resources.Team: </b>',
			'				</p>',
			'				<select data-bind="foreach: availableTeams, select2: { value: selectedTeam, minimumResultsForSearch: \'Infinity\'}" id="team-selection">',
			'					<optgroup data-bind="attr: { label: text }, foreach: children">',
			'						<option data-bind="text: text, value: id"></option>',
			'					</optgroup>',
			'				</select>',
			'			</div>',
			'			<div class="teamschedule-search-component">',
			'				<p>',
			'					<b>@Resources.AgentName: </b>',
			'				</p>',
			'				<form data-bind="submit: submitSearchForm">',
			'					<input type="search" class="form-control" placeholder=\'@Resources.SearchHintForName\' data-bind="value: searchNameText" />',
			'					<input type="submit" style="display: none"/>',
			'				</form>',
			'			</div>',
			'			<div class="empty-search-result">',
			'				<!-- ko if: emptySearchResult -->',
			'				<p><b>@Resources.NoResultForCurrentFilter</b></p>',
			'				<!--/ko-->',
			'			</div>',
			'			<div class="new-teamschedule-submit-buttons">',
			'				<button class="btn btn-default" data-bind="click: cancelClick">@Resources.Cancel</button>',
			'				<button class="btn btn-primary" data-bind="click: submitSearchForm">@Resources.Search</button>',
			'			</div>',
			'		</div>',
			'		<!--/ko-->',
			'	</div>',
			'</div>'
		].join('');
	}

	function setup() {
		fetchTeamScheduleDataRequestCount = 0;
		completeLoadedCount = 0;
		setUpHtml();
		setUpFakeData();
		setupAjax();

		Teleopti.MyTimeWeb.Common.DateFormat = dateFormat;

		$('body').append('<div class="new-teamschedule-view"></div>');
		Teleopti.MyTimeWeb.UserInfo = {
			WhenLoaded: function(whenLoadedCallBack) {
				var data = { WeekStart: 1 };
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
