$(document).ready(function () {
	var agentSchedulesHtml = '',
		completeLoadedCount = 0,
		fakeTeamScheduleData,
		fakeOriginalAgentSchedulesData,
		fakeDefaultTeamData,
		fakeAvailableTeamsData,
		agentCount = 200,
		ajax,
		fetchTeamScheduleDataRequestCount,
		constants = Teleopti.MyTimeWeb.Common.Constants,
		dateFormat = constants.serviceDateTimeFormat.dateOnly,
		selectedDate = moment().format(dateFormat),
		ajaxOption = {},
		vm;

	module('Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule', {
		setup: function () {
			Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTeamScheduleView_75989');
			Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTeamScheduleViewDesktop_76313');
			setup();

			Teleopti.MyTimeWeb.Common.SetupCalendar({
				UseJalaaliCalendar: false,
				DateFormat: 'YYYY-MM-DD',
				TimeFormat: 'HH:mm tt',
				AMDesignator: 'AM',
				PMDesignator: 'PM'
			});
		},
		teardown: function () {
			Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTeamScheduleView_75989');
			Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTeamScheduleViewDesktop_76313');

			$('#page').remove();

			completeLoadedCount = 0;
			fakeTeamScheduleData = undefined;
			fakeOriginalAgentSchedulesData = undefined;
			fakeDefaultTeamData = undefined;
			fakeAvailableTeamsData = undefined;
			fetchTeamScheduleDataRequestCount = 0;
		}
	});

	test('should select today as selectedDate(displayDate)', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		var today = moment().format(dateFormat);

		equal(vm.selectedDate().format(dateFormat), today);
		equal($('.new-teamschedule-view-nav .mobile-datepicker button.formatted-date-text').text(), today);
	});

	test('should format displayDate', function () {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		$('body').append(agentSchedulesHtml);
		initVm();

		equal(
			$('.new-teamschedule-view-nav .mobile-datepicker button.formatted-date-text').text(),
			Teleopti.MyTimeWeb.Common.FormatDate(moment())
		);
	});

	test('should change selected date after clicking on "previous day" icon', function () {
		$('body').append(agentSchedulesHtml);
		initVm();
		var today = moment();

		$('.previous-day').click();

		var expectDateStr = today.add(-1, 'days').format(dateFormat);

		equal(vm.selectedDate().format(dateFormat), expectDateStr);
		equal($('.new-teamschedule-view-nav .mobile-datepicker button.formatted-date-text').text(), expectDateStr);
	});

	test('should change selected date after clicking on "next day" icon', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		var today = moment();

		$('.next-day').click();

		var expectDateStr = today.add(1, 'days').format(dateFormat);

		equal(vm.selectedDate().format(dateFormat), expectDateStr);
		equal($('.new-teamschedule-view-nav .mobile-datepicker button.formatted-date-text').text(), expectDateStr);
	});

	test('should change display date after changing selected date', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		var today = moment();

		vm.selectedDate(today.add(9, 'days'));

		var expectDateStr = vm.selectedDate().format(dateFormat);

		equal($('.new-teamschedule-view-nav .mobile-datepicker button.formatted-date-text').text(), expectDateStr);
	});

	test('should reset loaded agent index after changing selected date', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		var today = moment();

		vm.loadedAgentIndex = 100;

		vm.selectedDate(today.add(9, 'days'));

		equal(vm.loadedAgentIndex, 49);
	});

	test('should reset the filling block width after changing selected date', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		var today = moment();

		vm.loadedAgentIndex = 100;

		vm.selectedDate(today.add(9, 'days'));

		equal($('.left-filling-block').width(), 0);
	});

	test('should render sites and teams on mobile', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.new-teamschedule-team-filter').click();

		var selector = '.new-teamschedule-panel .new-teamschedule-filter-component select';
		equal($(selector)[0].length, 11);

		equal($($(selector)[0][0]).text(), 'All Teams');
		equal($($(selector)[0][1]).text(), 'London/Students');
		equal($($(selector)[0][2]).text(), 'London/Team Flexible');
		equal($($(selector)[0][3]).text(), 'London/Team Outbound');
		equal($($(selector)[0][4]).text(), 'London/Team Preferences');
		equal($($(selector)[0][5]).text(), 'London/Team Rotations');
		equal($($(selector)[0][10]).text(), 'Contract Group/Team Rotations');
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should select default team on mobile', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.new-teamschedule-team-filter').click();

		equal(completeLoadedCount, 1);
		equal(
			$('.new-teamschedule-panel .new-teamschedule-filter-component select').val(),
			fakeDefaultTeamData.DefaultTeam
		);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should handle sites and teams hierarchy when user has no group page permission', function () {
		fakeAvailableTeamsData = {
			teams: [{ id: '34590a63-6331-4921-bc9f-9b5e015ab495', text: 'London/Team Preferences' }],
			allTeam: { id: 'allTeams', text: 'All Teams' }
		};
		fakeDefaultTeamData = {
			DefaultTeam: fakeAvailableTeamsData.teams[0].id,
			DefaultTeamName: fakeAvailableTeamsData.teams[0].text
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		var selector = '.new-teamschedule-filter-component select';
		equal($(selector)[0].length, 1);
		equal($($(selector)[0][0]).text(), 'London/Team Preferences');
		vm.loadGroupAndTeams();

		equal($(selector)[0].length, 2);
		equal($($(selector)[0][0]).text(), 'All Teams');
		equal($($(selector)[0][1]).text(), 'London/Team Preferences');
	});

	test('should set correct selected team id when click "AllTeams" item', function () {
		
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.new-teamschedule-team-filter').click();
		vm.loadGroupAndTeams(function () {
			vm.isTeamsAndGroupsLoaded(true);
		});

		var selectedTeam = fakeAvailableTeamsData.allTeam;
		$('#teams-and-groups-selector')
			.select2('data', { id: selectedTeam.id, text: selectedTeam.text })
			.trigger('change');

		equal(vm.selectedTeamIds.length, 5);
		equal(vm.selectedTeamIds[0], '2e92f1c8-a9cd-406b-ab08-6101e0be66aa');
		equal(vm.selectedTeamIds[1], '98b34e25-bd10-4c5a-82a3-248b919d8bef');
		equal(vm.selectedTeamIds[2], '44a12ef1-8db6-4f4f-8e60-00c08c97779a');
		equal(vm.selectedTeamIds[3], 'de42aa3e-2faa-4437-ab54-11b822c63429');
		equal(vm.selectedTeamIds[4], '23e29d14-21cf-43ad-a482-b4c5c6bb3890');
	});

	test('should not load sites and teams until user toggle the selection list on iPad', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAniPad;
		Teleopti.MyTimeWeb.Common.IsHostAniPad = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		var selector = '.new-teamschedule-filter-component select';
		equal($(selector)[0].length, 1);

		equal(
			$($(selector)[0])
				.text()
				.replace(/\s/g, ''),
			'London/Students'
		);
		equal(vm.isTeamsAndGroupsLoaded(), false);

		Teleopti.MyTimeWeb.Common.IsHostAniPad = tempFn;
	});

	test('should not load sites and teams until user toggle the selection list on desktop', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		var tempFn2 = Teleopti.MyTimeWeb.Common.IsHostAniPad;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return false;
		};
		Teleopti.MyTimeWeb.Common.IsHostAniPad = function () {
			return false;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		var selector = '.new-teamschedule-filter-component select';
		equal($(selector)[0].length, 1);

		equal(
			$($(selector)[0])
				.text()
				.replace(/\s/g, ''),
			'London/Students'
		);
		equal(vm.isTeamsAndGroupsLoaded(), false);

		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
		Teleopti.MyTimeWeb.Common.IsHostAniPad = tempFn2;
	});

	test('should select default team on iPad', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAniPad;
		Teleopti.MyTimeWeb.Common.IsHostAniPad = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		equal(completeLoadedCount, 1);
		equal($('.new-teamschedule-filter-component select').val(), fakeDefaultTeamData.DefaultTeam);
		Teleopti.MyTimeWeb.Common.IsHostAniPad = tempFn;
	});

	test('should select default team on desktop', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		var tempFn2 = Teleopti.MyTimeWeb.Common.IsHostAniPad;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return false;
		};
		Teleopti.MyTimeWeb.Common.IsHostAniPad = function () {
			return false;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		equal(completeLoadedCount, 1);
		equal($('.new-teamschedule-filter-component select').val(), fakeDefaultTeamData.DefaultTeam);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
		Teleopti.MyTimeWeb.Common.IsHostAniPad = tempFn2;
	});

	test('should not trigger schedule reload when changing selected team without clicking search button on mobile', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		equal(completeLoadedCount, 1);

		$('.new-teamschedule-team-filter').click();
		equal($('.new-teamschedule-filter-component select').val(), fakeDefaultTeamData.DefaultTeam);

		var selectedTeam = fakeAvailableTeamsData.teams[1].children[1];
		$('#teams-and-groups-selector')
			.select2('data', { id: selectedTeam.id, text: selectedTeam.text })
			.trigger('change');

		equal(completeLoadedCount, 1);

		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should trigger schedule reload when changing selected team on desktop', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		var tempFn2 = Teleopti.MyTimeWeb.Common.IsHostAniPad;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return false;
		};
		Teleopti.MyTimeWeb.Common.IsHostAniPad = function () {
			return false;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		equal(completeLoadedCount, 1);
		equal($('.new-teamschedule-filter-component select').val(), fakeDefaultTeamData.DefaultTeam);

		vm.loadGroupAndTeams(function () {
			vm.isTeamsAndGroupsLoaded(true);
		});

		var selectedTeam = fakeAvailableTeamsData.teams[1].children[1];
		$('#teams-and-groups-selector')
			.select2('data', { id: selectedTeam.id, text: selectedTeam.text })
			.trigger('change');

		equal(completeLoadedCount, 2);

		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
		Teleopti.MyTimeWeb.Common.IsHostAniPad = tempFn2;
	});

	test('should trigger schedule reload when changing selected team on iPad', function () {
		var tempFn2 = Teleopti.MyTimeWeb.Common.IsHostAniPad;
		Teleopti.MyTimeWeb.Common.IsHostAniPad = function () {
			return false;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		equal(completeLoadedCount, 1);
		equal($('.new-teamschedule-filter-component select').val(), fakeDefaultTeamData.DefaultTeam);

		vm.loadGroupAndTeams(function () {
			vm.isTeamsAndGroupsLoaded(true);
		});

		var selectedTeam = fakeAvailableTeamsData.teams[1].children[1];
		$('#teams-and-groups-selector')
			.select2('data', { id: selectedTeam.id, text: selectedTeam.text })
			.trigger('change');

		equal(completeLoadedCount, 2);

		Teleopti.MyTimeWeb.Common.IsHostAniPad = tempFn2;
	});

	test('should render timeline', function () {
		$('body').append(agentSchedulesHtml);
		initVm();
		vm.selectedDate(moment(selectedDate));

		var labelSelector = '.new-teamschedule-view .mobile-timeline .mobile-timeline-label';
		equal($(labelSelector).length, 32);
		equal($(labelSelector)[0].innerText, '23:00 -1');
		equal($(labelSelector)[1].innerText, '00:00');
		equal($(labelSelector)[2].innerText, '01:00');
		equal($(labelSelector)[3].innerText, '02:00');
		equal($(labelSelector)[30].innerText, '05:00 +1');
		equal($(labelSelector)[31].innerText, '06:00 +1');
	});

	test('should update timeline after load more schedules', function () {
		$('body').append(agentSchedulesHtml);
		initVm();
		vm.selectedDate(moment(selectedDate));

		vm.readMoreTeamScheduleData({
			AgentSchedules: [],
			TimeLine: [
				{
					Time: moment(selectedDate).format(dateFormat) + 'T08:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T08:00',
					PositionPercentage: 0.0,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T09:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T09:00',
					PositionPercentage: 0.0412,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T10:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T10:00',
					PositionPercentage: 0.0825,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T11:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T11:00',
					PositionPercentage: 0.1237,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T12:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T12:00',
					PositionPercentage: 0.1649,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T13:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T13:00',
					PositionPercentage: 0.2062,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T14:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T14:00',
					PositionPercentage: 0.2474,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T15:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T15:00',
					PositionPercentage: 0.2887,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T16:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T16:00',
					PositionPercentage: 0.3299,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T17:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T17:00',
					PositionPercentage: 0.3711,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T18:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T18:00',
					PositionPercentage: 0.4124,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T19:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T19:00',
					PositionPercentage: 0.4536,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T20:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T20:00',
					PositionPercentage: 0.4948,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T21:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T21:00',
					PositionPercentage: 0.5361,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T22:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T22:00',
					PositionPercentage: 0.5773,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T23:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T23:00',
					PositionPercentage: 0.6186,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T00:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T00:00',
					PositionPercentage: 0.6598,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T01:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T01:00',
					PositionPercentage: 0.701,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T02:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T02:00',
					PositionPercentage: 0.7423,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T03:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T03:00',
					PositionPercentage: 0.7835,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T04:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T04:00',
					PositionPercentage: 0.8247,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T05:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T05:00',
					PositionPercentage: 0.866,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T06:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T06:00',
					PositionPercentage: 0.9072,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T07:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T07:00',
					PositionPercentage: 0.9485,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T08:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T08:00',
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

	test('should render overnight timeline', function () {
		$('body').append(agentSchedulesHtml);
		initVm();
		vm.selectedDate(moment(selectedDate));

		vm = Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.Vm();

		var labelSelector = '.new-teamschedule-view .mobile-timeline .mobile-timeline-label:visible';
		equal($(labelSelector).length, 32);
		equal($(labelSelector)[0].innerText, '23:00 -1');
		equal($(labelSelector)[1].innerText, '00:00');
		equal($(labelSelector)[2].innerText, '01:00');
		equal($(labelSelector)[3].innerText, '02:00');
		equal($(labelSelector)[30].innerText, '05:00 +1');
		equal($(labelSelector)[31].innerText, '06:00 +1');
	});

	test('should render my schedule', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		equal(
			$('.new-teamschedule-view .new-teamschedule-agent-name.my-name .text-name').text(),
			'@Resources.MySchedule'
		);
		equal($('.new-teamschedule-view .my-schedule-column .new-teamschedule-layer').length, 1);
		equal($('.new-teamschedule-view .my-schedule-column .new-teamschedule-layer strong').text(), 'Phone');
	});

	test('should render overtime on day off', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		equal($('.new-teamschedule-view .my-schedule-column .new-teamschedule-layer.overtime-layer').length, 1);
		equal(
			$('.new-teamschedule-view .my-schedule-column .new-teamschedule-layer.overtime-layer strong').text(),
			'Phone'
		);
	});

	test("should render teammates's schedules", function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[0])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith1'
		);
		equal($('.new-teamschedule-view .teammates-schedules-column .new-teamschedule-layer').length, 50);
		equal(
			$('.new-teamschedule-view .teammates-schedules-column .new-teamschedule-layer strong:visible').length,
			50
		);
	});

	test('should not show title when the activity length is less than 30 minutes', function () {
		fakeOriginalAgentSchedulesData.splice(1);
		fakeOriginalAgentSchedulesData[0].Periods = [
			{
				Title: 'Email',
				TimeSpan: '06:30 - 06:47',
				StartTime: selectedDate + 'T06:30:00',
				EndTime: selectedDate + 'T06:47:00',
				Summary: '',
				StyleClassName: '',
				Meeting: '',
				StartPositionPercentage: 0.0,
				EndPositionPercentage: 0.05,
				Color: '#80FF80',
				IsOvertime: false,
				IsAbsenceConfidential: false,
				TitleTime: '06:30 - 06:47'
			}
		];

		$('body').append(agentSchedulesHtml);
		initVm();

		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[0])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith1'
		);
		equal($('.new-teamschedule-view .teammates-schedules-column .new-teamschedule-layer').length, 1);
		equal($('.new-teamschedule-view .teammates-schedules-column .new-teamschedule-layer strong:visible').length, 0);
	});

	test('should show shift category for my schedule', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		equal($('.new-teamschedule-view .new-teamschedule-agent-name.my-name span.shift-category-cell').text(), 'AM');
	});

	test('should show day off short name for my schedule', function () {
		fakeTeamScheduleData.MySchedule.ShiftCategory.Name = 'Day Off';
		fakeTeamScheduleData.MySchedule.ShiftCategory.ShortName = 'DO';

		$('body').append(agentSchedulesHtml);
		initVm();

		equal($('.new-teamschedule-view .new-teamschedule-agent-name.my-name span.shift-category-cell').text(), 'DO');
	});

	test('should reload schedule after clicking on "previous day" icon', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.previous-day').click();

		equal(fetchTeamScheduleDataRequestCount, 2);
	});

	test('should reload schedule after clicking on "next day" icon', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.next-day').click();

		equal(fetchTeamScheduleDataRequestCount, 2);
	});

	test('should reset skip count after changing selected date', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		var today = moment();

		vm.paging.skip = 10;

		vm.selectedDate(today.add(9, 'days'));

		equal(vm.paging.skip, 0);
	});

	test('should reset skip count after clicking on "previous day" icon', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		vm.paging.skip = 10;
		$('.previous-day').click();

		equal(vm.paging.skip, 0);
	});

	test('should reset skip count after clicking on "next day" icon', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		vm.paging.skip = 10;
		$('.next-day').click();

		equal(vm.paging.skip, 0);
	});

	test('should filter agent schedules on mobile', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.new-teamschedule-team-filter').click();

		$('.new-teamschedule-view input.form-control').val('Kleinsmith5');
		$('.new-teamschedule-view input.form-control').change();
		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal(vm.currentPageNum, 1);
		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[0])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith5'
		);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should update agent names after loaded schedules', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[0])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith1'
		);
		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[9])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith10'
		);
	});

	test('should update agent names after loaded more schedules', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		vm.readMoreTeamScheduleData({
			AgentSchedules: fakeOriginalAgentSchedulesData.slice(10),
			TimeLine: [
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T07:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T07:00',
					PositionPercentage: 0.9485,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T08:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T08:00',
					PositionPercentage: 0.9897,
					TimeFixedFormat: null
				}
			]
		});

		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[0])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith1'
		);
		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[9])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith10'
		);
		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[10])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith11'
		);
		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[19])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith20'
		);
	});

	test('should reset search name to empty after click cancel button in panel', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.new-teamschedule-team-filter').click();

		$('.new-teamschedule-view input.form-control').val('test search name text');
		$('.new-teamschedule-view input.form-control').change();

		$('.new-teamschedule-submit-buttons .btn-default').click();

		equal(vm.searchNameText(), '');
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should reset search name text to last submitted value after click cancel button in panel', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		vm.searchNameText('10');
		vm.submitSearchForm();

		$('.new-teamschedule-team-filter').click();

		vm.searchNameText('test search name text');

		$('.new-teamschedule-submit-buttons button:first-child').click();

		equal(vm.searchNameText(), '10');
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should reset to selected team default team after click cancel button in panel', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.new-teamschedule-team-filter').click();

		vm.selectedTeam('d7a9c243-8cd8-406e-9889-9b5e015ab495');

		$('.new-teamschedule-submit-buttons button:first-child').click();

		equal(vm.selectedTeam(), fakeDefaultTeamData.DefaultTeam);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should reset selected team to last submitted team after click cancel button in panel', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		vm.selectedTeam('allTeams');
		vm.submitSearchForm();

		$('.new-teamschedule-team-filter').click();

		vm.selectedTeam('a74e1f94-7662-4a7f-9746-a56e00a66f17');

		$('.new-teamschedule-submit-buttons button:first-child').click();

		equal(vm.selectedTeam(), 'allTeams');
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should map dayOff of MySchedule', function () {
		fakeTeamScheduleData.MySchedule.Periods = [];
		$('body').append(agentSchedulesHtml);
		initVm();

		equal(vm.mySchedule().isDayOff, true);
		equal(vm.mySchedule().dayOffName, 'Day off');

		equal(
			$('.my-schedule-column .new-teamschedule-layer-container .dayoff')
				.text()
				.replace(/(^\s*)|(\s*$)/g, ''),
			'Day off'
		);
	});

	test('should map dayOff of agent schedule', function () {
		fakeTeamScheduleData.AgentSchedules[0] = {
			Name: 'Test Day Off agent',
			Periods: [],
			IsDayOff: true,
			DayOffName: 'Day off',
			IsNotScheduled: false,
			ShiftCategory: {
				Id: null,
				ShortName: 'DO',
				Name: 'Day off',
				DisplayColor: '#808080'
			}
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		vm.searchNameText('Test Day Off agent');
		$('.new-teamschedule-search-container button[type="submit"]').click();

		equal(
			$('.teammates-schedules-column .new-teamschedule-layer-container .dayoff')
				.text()
				.replace(/(^\s*)|(\s*$)/g, ''),
			'Day off'
		);
	});

	test('should map day off but not show "Day off" text when having overtime on day offs', function () {
		fakeTeamScheduleData.AgentSchedules[0] = {
			Name: 'Test Day Off agent',
			Periods: [
				{
					Title: 'E-mail',
					TimeSpan: '01:00 PM - 02:00 PM',
					Color: '128,128,255',
					StartTime: selectedDate + 'T13:00:00',
					EndTime: selectedDate + 'T14:00:00',
					IsOvertime: true,
					StartPositionPercentage: 0.5526,
					EndPositionPercentage: 0.6579,
					Meeting: null
				}
			],
			IsDayOff: true,
			DayOffName: 'Day off',
			IsNotScheduled: false,
			ShiftCategory: {
				Id: null,
				ShortName: 'DO',
				Name: 'Day off',
				DisplayColor: '#808080'
			}
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		vm.searchNameText('Test Day Off agent');
		$('.new-teamschedule-search-container button[type="submit"]').click();

		equal(
			$('.teammates-schedules-column .new-teamschedule-layer-container .dayoff')
				.text()
				.replace(/(^\s*)|(\s*$)/g, ''),
			''
		);

		equal($('.teammates-schedules-column .new-teamschedule-layer-container .dayoff').length, 1);
	});

	test('should map notScheduled of MySchedule', function () {
		$('body').append(agentSchedulesHtml);
		fakeTeamScheduleData.MySchedule.IsNotScheduled = true;
		initVm();

		equal(vm.mySchedule().isNotScheduled, true);
		equal($('.my-schedule-column .not-scheduled-text').text(), '@Resources.NotScheduled');
	});

	test("should map notScheduled of agents' schedule", function () {
		$('body').append(agentSchedulesHtml);
		if (fakeOriginalAgentSchedulesData.length > 0) {
			fakeOriginalAgentSchedulesData[0].IsNotScheduled = true;
		}
		initVm();
		vm.selectedDate(moment('2018-07-23'));

		equal(vm.teamSchedules()[0].isNotScheduled, true);
		equal(
			$('.teammates-schedules-container .not-scheduled-text')
				.first()
				.text(),
			'@Resources.NotScheduled'
		);
	});

	test("should filter agents' day off after turn on show only day off toggle on desktop", function () {
		$('body').append(agentSchedulesHtml);

		if (fakeOriginalAgentSchedulesData.length > 0) {
			fakeOriginalAgentSchedulesData[0].IsDayOff = true;
		}
		initVm();

		$('.new-teamschedule-time-filter').click();
		$('.show-only-day-off-toggle input').click();

		equal(completeLoadedCount, 2);
		equal($('.teammates-schedules-container .dayoff').length == 1, true);
	});

	test('should filter agents using search name after switch show only day off toggle on desktop', function () {
		$('body').append(agentSchedulesHtml);

		if (fakeOriginalAgentSchedulesData.length > 0) {
			fakeOriginalAgentSchedulesData[0].IsDayOff = true;
		}
		initVm();

		vm.searchNameText('this is a test search name text');
		$('.new-teamschedule-time-filter').click();
		$('.show-only-day-off-toggle input').click();

		equal(completeLoadedCount, 2);
		equal(ajaxOption.ScheduleFilter.searchNameText, 'this is a test search name text');
	});

	test('should reset paging after turn on show only day off toggle', function () {
		$('body').append(agentSchedulesHtml);

		if (fakeOriginalAgentSchedulesData.length > 0) {
			fakeOriginalAgentSchedulesData[0].IsDayOff = true;
		}
		initVm();

		vm.paging.take = 20;
		vm.paging.skip = 20;

		$('.new-teamschedule-time-filter').click();
		$('.show-only-day-off-toggle input').click();

		equal(completeLoadedCount, 2);
		equal(vm.paging.take, 20);
		equal(vm.paging.skip, 0);
		equal($('.teammates-schedules-container .dayoff').length == 1, true);
	});

	test("should filter agents' with night shift only after turn on show only night shifts toggle on desktop", function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();
		$('.show-only-night-shift-toggle input').click();

		equal(completeLoadedCount, 2);
		equal(ajaxOption.ScheduleFilter.onlyNightShift, true);
		equal(vm.hasTimeFiltered(), true);
		equal($('.teammates-schedules-container .teammates-schedules-column').length == 2, true);
	});

	test('should keep panel open after turn on show only night shifts toggle on desktop', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		//turn on show only night shfits toggle
		$('.new-teamschedule-time-filter').click();
		$('.show-only-night-shift-toggle input').click();

		equal(vm.isPanelVisible(), true);
	});

	test('should keep panel open after turn off show only night shifts toggle on desktop', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		//turn on show only night shfits toggle
		$('.new-teamschedule-time-filter').click();
		$('.show-only-night-shift-toggle input').click();
		$('.new-teamschedule-time-filter').click();

		//turn off show only night shfits toggle
		$('.new-teamschedule-time-filter').click();
		$('.show-only-night-shift-toggle input').click();

		equal(vm.isPanelVisible(), true);
	});

	test('should filter schedules with start and end time when toggling off show only night shifts on desktop', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();
		vm.startTimeStart('06:00');
		vm.startTimeEnd('10:00');
		$('.new-teamschedule-submit-buttons .btn-primary').click(); //search schedule with start time

		$('.new-teamschedule-time-filter').click();
		$('.show-only-night-shift-toggle input').click();
		$('.show-only-night-shift-toggle input').click();

		equal(vm.hasTimeFiltered(), true);
		equal(ajaxOption.ScheduleFilter.filteredStartTimes, '06:00-10:00');
	});

	test('should set show only day off to false when changing show only night shifts toggle on desktop', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();
		$('.show-only-day-off-toggle input').click();

		$('.show-only-night-shift-toggle input').click();

		equal(completeLoadedCount, 3);
		equal(ajaxOption.ScheduleFilter.isDayOff, false);
	});

	test('should set show only night shifts to false when changing show only day off toggle on desktop', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();
		$('.show-only-night-shift-toggle input').click();

		$('.show-only-day-off-toggle input').click();

		equal(completeLoadedCount, 3);
		equal(ajaxOption.ScheduleFilter.isDayOff, true);
		equal(ajaxOption.ScheduleFilter.onlyNightShift, false);
		equal(vm.hasTimeFiltered(), true);
	});

	test('should restore show only day off toggle state after clicking cancel button on mobile', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);

		if (fakeOriginalAgentSchedulesData.length > 0) {
			fakeOriginalAgentSchedulesData[0].IsDayOff = true;
		}
		initVm();

		$('.new-teamschedule-team-filter button').click();
		$('.show-only-day-off-toggle input').click();
		$('.new-teamschedule-submit-buttons .btn-default').click();

		equal(completeLoadedCount, 1);
		equal(vm.showOnlyDayOff(), false);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should set showOnlyNightShift after turn on show only night shifts toggle and click search button on mobile', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.new-teamschedule-team-filter button').click();
		$('.show-only-night-shift-toggle input').click();
		$('.new-teamschedule-submit-buttons .btn-primary').click();

		equal(vm.showOnlyNightShift(), true);
		equal(vm.hasTimeFiltered(), true);

		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should close panel after clicking search button on desktop', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();
		$('.new-teamschedule-submit-buttons .btn-primary').click();

		equal(vm.isPanelVisible(), false);
	});

	test('should restore show only night shifts toggle state after clicking cancel button on mobile', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.new-teamschedule-team-filter button').click();
		$('.show-only-night-shift-toggle input').click();
		$('.new-teamschedule-submit-buttons .btn-primary').click();

		$('.new-teamschedule-team-filter button').click();
		$('.show-only-night-shift-toggle input').click();
		$('.new-teamschedule-submit-buttons .btn-default').click();

		equal(completeLoadedCount, 2);
		equal(vm.showOnlyNightShift(), true);
		equal(vm.hasTimeFiltered(), true);

		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should restore show only day off toggle state after clicking filter icon on mobile', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);

		if (fakeOriginalAgentSchedulesData.length > 0) {
			fakeOriginalAgentSchedulesData[0].IsDayOff = true;
		}
		initVm();

		$('.new-teamschedule-team-filter button').click();
		$('.show-only-day-off-toggle input').click();
		$('.new-teamschedule-team-filter button').click();

		equal(completeLoadedCount, 1);
		equal(vm.showOnlyDayOff(), false);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should restore show only night shifts toggle state after clicking filter icon on mobile', function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);
		initVm();

		$('.new-teamschedule-team-filter button').click();
		$('.show-only-night-shift-toggle input').click();
		$('.new-teamschedule-team-filter button').click();

		equal(completeLoadedCount, 1);
		equal(vm.showOnlyNightShift(), false);
		equal(vm.hasTimeFiltered(), false);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test("should not filter agents' day off immediately after turn on show only day off toggle but widthout clicking search on mobile", function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-team-filter').click();
		$('.show-only-day-off-toggle input').click();

		equal(completeLoadedCount, 1);
		equal($('.teammates-schedules-container .dayoff').length == 0, true);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test("should filter agents' day off after turn on show only day off toggle and click search on mobile", function () {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function () {
			return true;
		};
		$('body').append(agentSchedulesHtml);

		if (fakeOriginalAgentSchedulesData.length > 0) {
			fakeOriginalAgentSchedulesData[agentCount - 1].IsDayOff = true;
			fakeOriginalAgentSchedulesData[agentCount - 1].Periods = [];
		}
		initVm();

		$('.new-teamschedule-team-filter').click();
		$('.show-only-day-off-toggle input').click();
		$('.new-teamschedule-submit-buttons .btn-primary').click();

		equal(completeLoadedCount, 2);
		equal($('.teammates-schedules-container .dayoff').length == 1, true);
		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[0])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith200'
		);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should filter agents using start time slider', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();

		vm.startTimeStart('06:00');
		vm.startTimeEnd('10:00');

		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal(completeLoadedCount, 2);
		equal($($('.teammates-agent-name-row .new-teamschedule-agent-name')[0]).find('.text-name').length, 0);
	});

	test('should not filter agents using start time slider if both start time start and start time end are zero', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();

		vm.startTimeStart('00:00');
		vm.startTimeEnd('00:00');

		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal(completeLoadedCount, 2);
		equal(ajaxOption.ScheduleFilter.filteredStartTimes === '', true);
	});

	test('should reset start time filter after clicking start time cross icon', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();

		$('.start-time-slider').slider('values', [60, 120]);
		vm.startTimeStart('01:00');
		vm.startTimeEnd('02:00');
		$('.start-time-clear-button').click();
		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal(completeLoadedCount, 2);
		equal(vm.startTimeStart(), '00:00');
		equal(vm.startTimeEnd(), '00:00');
		equal(ajaxOption.ScheduleFilter.filteredStartTimes === '', true);
	});

	test('should filter agents using end time slider', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();

		vm.endTimeStart('06:00');
		vm.endTimeEnd('10:00');

		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal(completeLoadedCount, 2);
		equal($($('.teammates-agent-name-row .new-teamschedule-agent-name')[0]).find('.text-name').length, 1);
		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[0])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith1'
		);
	});

	test('should not filter agents using end time slider if both end time start and end time end are zero', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();

		vm.endTimeStart('00:00');
		vm.endTimeEnd('00:00');

		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal(completeLoadedCount, 2);
		equal(ajaxOption.ScheduleFilter.filteredStartTimes === '', true);
	});

	test('should reset end time filter after clicking end time cross icon', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();

		$('.end-time-slider').slider('values', [60, 120]);
		vm.endTimeStart('01:00');
		vm.endTimeEnd('02:00');
		$('.end-time-clear-button').click();
		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal(completeLoadedCount, 2);
		equal(vm.endTimeStart(), '00:00');
		equal(vm.endTimeEnd(), '00:00');
		equal(ajaxOption.ScheduleFilter.filteredEndTimes === '', true);
	});

	test('should filter agents using both start and end time slider', function () {
		$('body').append(agentSchedulesHtml);

		initVm();

		$('.new-teamschedule-time-filter').click();

		vm.startTimeStart('04:00');
		vm.startTimeEnd('07:00');

		vm.endTimeStart('05:00');
		vm.endTimeEnd('10:00');

		$('.new-teamschedule-submit-buttons button.btn-primary').click();

		equal(completeLoadedCount, 2);
		equal($('.teammates-agent-name-row .new-teamschedule-agent-name .text-name').length, 50);
		equal(
			$($('.teammates-agent-name-row .new-teamschedule-agent-name')[0])
				.find('.text-name')
				.text(),
			'Jon Kleinsmith1'
		);
	});

	test('should not show meeting info for agents schedule', function () {
		$('body').append(agentSchedulesHtml);
		initVm();

		$(".teammates-schedules-column .new-teamschedule-layer").click();
		equal($("div.meeting-detail").length, 0);
	});

	function setup() {
		fetchTeamScheduleDataRequestCount = 0;
		completeLoadedCount = 0;
		setUpHtml();
		setUpFakeData();
		setupAjax();

		Teleopti.MyTimeWeb.UserInfo = {
			WhenLoaded: function (whenLoadedCallBack) {
				var data = { WeekStart: 1 };
				whenLoadedCallBack(data);
			}
		};
	}

	function setUpFakeData() {
		fakeAvailableTeamsData = {
			allTeam: { id: 'allTeams', text: 'All Teams' },
			teams: [
				{
					children: [
						{ id: '2e92f1c8-a9cd-406b-ab08-6101e0be66aa', text: 'London/Students' },
						{ id: '98b34e25-bd10-4c5a-82a3-248b919d8bef', text: 'London/Team Flexible' },
						{ id: '44a12ef1-8db6-4f4f-8e60-00c08c97779a', text: 'London/Team Outbound' },
						{ id: 'de42aa3e-2faa-4437-ab54-11b822c63429', text: 'London/Team Preferences' },
						{ id: '23e29d14-21cf-43ad-a482-b4c5c6bb3890', text: 'London/Team Rotations' }
					],
					PageId: '7ce00b41-0722-4b36-91dd-0a3b63c545cf',
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
		fakeDefaultTeamData = {
			DefaultTeam: fakeAvailableTeamsData.teams[0].children[0].id,
			DefaultTeamName: fakeAvailableTeamsData.teams[0].children[0].text
		};

		fakeTeamScheduleData = {
			AgentSchedules: [],
			TimeLine: [
				{
					Time:
						moment(selectedDate)
							.add(-1, 'day')
							.format(dateFormat) + 'T23:00:00',
					TimeLineDisplay: '22/07/2018T23:00',
					PositionPercentage: 0.0,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T00:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T00:00',
					PositionPercentage: 0.032,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T01:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T01:00',
					PositionPercentage: 0.064,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T02:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T02:00',
					PositionPercentage: 0.096,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T03:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T03:00',
					PositionPercentage: 0.128,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T04:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T04:00',
					PositionPercentage: 0.16,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T05:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T05:00',
					PositionPercentage: 0.192,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T06:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T06:00',
					PositionPercentage: 0.224,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T07:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T07:00',
					PositionPercentage: 0.256,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T08:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T08:00',
					PositionPercentage: 0.288,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T09:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T09:00',
					PositionPercentage: 0.32,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T10:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T10:00',
					PositionPercentage: 0.352,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T11:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T11:00',
					PositionPercentage: 0.384,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T12:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T12:00',
					PositionPercentage: 0.416,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T13:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T13:00',
					PositionPercentage: 0.448,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T14:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T14:00',
					PositionPercentage: 0.48,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T15:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T15:00',
					PositionPercentage: 0.512,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T16:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T16:00',
					PositionPercentage: 0.544,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T17:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T17:00',
					PositionPercentage: 0.576,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T18:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T18:00',
					PositionPercentage: 0.608,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T19:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T19:00',
					PositionPercentage: 0.64,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T20:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T20:00',
					PositionPercentage: 0.672,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T21:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T21:00',
					PositionPercentage: 0.704,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T22:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T22:00',
					PositionPercentage: 0.736,
					TimeFixedFormat: null
				},
				{
					Time: moment(selectedDate).format(dateFormat) + 'T23:00:00',
					TimeLineDisplay: moment(selectedDate).format(dateFormat) + 'T23:00',
					PositionPercentage: 0.768,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T00:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T00:00',
					PositionPercentage: 0.8,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T01:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T01:00',
					PositionPercentage: 0.832,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T02:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T02:00',
					PositionPercentage: 0.864,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T03:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T03:00',
					PositionPercentage: 0.896,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T04:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T04:00',
					PositionPercentage: 0.928,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T05:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T05:00',
					PositionPercentage: 0.96,
					TimeFixedFormat: null
				},
				{
					Time:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T06:00:00',
					TimeLineDisplay:
						moment(selectedDate)
							.add(1, 'day')
							.format(dateFormat) + 'T06:00',
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
						StartTime: selectedDate + 'T05:00:00',
						EndTime: selectedDate + 'T06:45:00',
						Summary: '',
						StyleClassName: '',
						Meeting: '',
						StartPositionPercentage: '',
						EndPositionPercentage: '',
						Color: '#80FF80',
						IsOvertime: true,
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

		fakeOriginalAgentSchedulesData = [];
		for (var i = 0; i < agentCount; i++) {
			var agentSchedule = {
				Name: 'Jon Kleinsmith' + (i + 1),
				StartTimeUtc: selectedDate + 'T05:00:00',
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
						Title: 'Administration',
						TimeSpan: '05:00 - 06:45',
						StartTime: selectedDate + 'T05:00:00',
						EndTime: selectedDate + 'T06:45:00',
						Summary: '',
						StyleClassName: '',
						Meeting: { Title: "dp meeting", Location: "dp meeting", Description: "another plan" },
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

		fakeTeamScheduleData.AgentSchedules = fakeOriginalAgentSchedulesData;
	}

	function setupAjax() {
		ajax = {
			Ajax: function (option) {
				if (option.url === 'Team/TeamsAndGroupsWithAllTeam') {
					option.success(fakeAvailableTeamsData);
				}

				if (option.url === '../api/TeamSchedule/DefaultTeam') {
					option.success(fakeDefaultTeamData);
				}

				if (option.url === '../api/TeamSchedule/TeamSchedule') {
					fetchTeamScheduleDataRequestCount++;

					ajaxOption = JSON.parse(option.data);
					var skip = ajaxOption.Paging.Skip;
					var take = ajaxOption.Paging.Take;

					var filteredAgentSchedulesData = [];
					filteredAgentSchedulesData = fakeOriginalAgentSchedulesData;

					if (ajaxOption.ScheduleFilter.isDayOff) {
						filteredAgentSchedulesData = filteredAgentSchedulesData.filter(function (s) {
							return s.IsDayOff;
						});
					}

					if (ajaxOption.ScheduleFilter.filteredStartTimes) {
						var start = ajaxOption.ScheduleFilter.filteredStartTimes.split('-')[0];
						var startHour = parseInt(start.split(':')[0]);
						var startMinute = parseInt(start.split(':')[1]);

						var end = ajaxOption.ScheduleFilter.filteredStartTimes.split('-')[1];
						var endHour = parseInt(end.split(':')[0]);
						var endMinute = parseInt(end.split(':')[1]);

						filteredAgentSchedulesData = filteredAgentSchedulesData.filter(function (s) {
							var startTime = moment(s.Periods[0].StartTime);
							return (
								(startTime.hours() > startHour ||
									(startTime.hours() == startHour && startTime.minutes() > startMinute)) &&
								(startTime.hours() < endHour ||
									(startTime.hours() == endHour && startTime.minutes() < endMinute))
							);
						});
					}

					if (ajaxOption.ScheduleFilter.filteredEndTimes) {
						var start = ajaxOption.ScheduleFilter.filteredEndTimes.split('-')[0];
						var startHour = parseInt(start.split(':')[0]);
						var startMinute = parseInt(start.split(':')[1]);

						var end = ajaxOption.ScheduleFilter.filteredEndTimes.split('-')[1];
						var endHour = parseInt(end.split(':')[0]);
						var endMinute = parseInt(end.split(':')[1]);

						filteredAgentSchedulesData = filteredAgentSchedulesData.filter(function (s) {
							var endTime = moment(s.Periods[0].EndTime);
							return (
								(endTime.hours() > startHour ||
									(endTime.hours() == startHour && endTime.minutes() > startMinute)) &&
								(endTime.hours() < endHour ||
									(endTime.hours() == endHour && endTime.minutes() < endMinute))
							);
						});
					}

					if (ajaxOption.ScheduleFilter.onlyNightShift) {
						//only night shifts: simply return two agents
						filteredAgentSchedulesData = fakeOriginalAgentSchedulesData.slice(0, 2);
					}

					var pagedAgentSchedules = [];
					for (var i = skip; i < skip + take; i++) {
						if (filteredAgentSchedulesData[i]) {
							if (ajaxOption.ScheduleFilter.searchNameText) {
								if (
									filteredAgentSchedulesData[i].Name.indexOf(
										ajaxOption.ScheduleFilter.searchNameText
									) > -1
								) {
									pagedAgentSchedules.push(filteredAgentSchedulesData[i]);
								}
							} else {
								pagedAgentSchedules.push(filteredAgentSchedulesData[i]);
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
			'<div class="nav navbar teleopti-toolbar new-teamschedule-view-nav" data-bind="css: {\'new-teamschedule-view-nav-mobile\': isHostAMobile}">',
			'	<div class="container">',
			'		<ul class="nav navbar-nav teleopti-toolbar row submenu">',
			'			<li class="mobile-datepicker">',
			'				<div class="input-group">',
			'					<span class="input-group-btn">',
			'						<button class="btn btn-default previous-day" aria-label="@Resources.PreviousDay" data-bind="click: previousDay">',
			'							<i class="glyphicon glyphicon-chevron-left"></i>',
			'						</button>',
			'					</span>',
			'					<button class="btn btn-default formatted-date-text moment-datepicker" aria-haspopup="true"></button>',
			'					<span class="input-group-btn">',
			'						<button class="btn btn-default next-day" " aria-label="@Resources.NextDay" data-bind="click: nextDay">',
			'							<i class="glyphicon glyphicon-chevron-right"></i>',
			'						</button>',
			'					</span>',
			'				</div>',
			'			</li>',
			'			<!-- ko if: isHostAMobile || isHostAniPad -->',
			'			<li class="mobile-today" data-bind="click: today">',
			'				<button class="btn btn-default" aria-label="@Resources.Today">',
			'					<i class="glyphicon glyphicon-home"></i>',
			'				</button>',
			'			</li>',
			'			<!-- /ko -->',
			'			<!-- ko if: isHostADesktop -->',
			"			<li class=\"mobile-today\" data-bind=\"click: today, tooltip: { title: '@Resources.Today', html: true, trigger: 'hover', placement: 'bottom'}\">",
			'				<button class="btn btn-default" aria-label="@Resources.Today">',
			'					<i class="glyphicon glyphicon-home"></i>',
			'				</button>',
			'			</li>',
			'			<!-- /ko -->',
			'			<!-- ko if: isHostAMobile -->',
			'			<li class="new-teamschedule-team-filter" data-bind="css: {\'new-teamschedule-has-time-filter\': hasFilteredOnMobile}">',
			'				<button class="btn btn-default" aria-label="@Resources.Filter" aria-haspopup="true">',
			'					<i class="glyphicon glyphicon-filter"></i>',
			'					<span></span>',
			'				</button>',
			'			</li>',
			'			<!-- /ko -->',
			'			<!-- ko if: isHostADesktop || isHostAniPad -->',
			"			<li class=\"new-teamschedule-time-filter\" data-bind=\"css: {'new-teamschedule-has-time-filter': hasTimeFiltered}, tooltip: { title: buildFilterDetails(), html: true, trigger: 'hover', placement: 'bottom'}\">",
			'				<button class="btn btn-default" aria-label="@Resources.Filter" aria-haspopup="true">',
			'					<i class="glyphicon glyphicon-filter"></i>',
			'					<span></span>',
			'				</button>',
			'			</li>',
			'			<li class="new-teamschedule-filter-component relative">',
			'				<div data-bind="click: openTeamSelectorPanel"></div>',
			'				<select id="teams-and-groups-selector" data-bind="foreach: availableTeams, select2: {value: selectedTeam}">',
			'					<optgroup data-bind="attr: { label: text }, foreach: children">',
			'						<option data-bind="text: text, value: id"></option>',
			'					</optgroup>',
			'				</select>',
			'			</li>',
			'			<li class="new-teamschedule-search-container">',
			'				<form class="new-teamschedule-search-component relative" data-bind="submit: submitSearchForm">',
			'					<input type="search" class="form-control" placeholder=\'@Resources.SearchHintForName\' data-bind="value: searchNameText" />',
			'					<button type="submit">',
			'						<i class="glyphicon glyphicon-search"></i>',
			'					</button>',
			'				</form>',
			'			</li>',
			'			<!-- /ko -->',
			'		</ul>',
			'	</div>',
			'</div>',
			"<div class=\"container relative pagebody new-teamschedule-view relative\" data-bind=\"css: {'new-teamschedule-view-mobile': isHostAMobile && isMobileEnabled, 'new-teamschedule-view-ipad': isHostAniPad && isDesktopEnabled}, style: {display: isMobileEnabled || isDesktopEnabled ? 'block': 'none'}\">",
			'	<div class="agent-name-row container relative">',
			'		<!-- ko if: mySchedule -->',
			'		<div class="timeline-margin-block relative"></div>',
			'		<div class="new-teamschedule-agent-name my-name relative">',
			'			<span class="text-name">@Resources.MySchedule</span>',
			'			<span class="shift-category-cell" data-bind="text: mySchedule().shiftCategory.name, style: {background: mySchedule().shiftCategory.bgColor, color: mySchedule().shiftCategory.color}, css: {\'dayoff\': mySchedule().isDayOff}">',
			'			</span>',
			'		</div>',
			'		<!-- /ko -->',
			'		<div class="teammates-agent-name-row relative">',
			'			<div class="left-filling-block"></div>',
			'			<!-- ko foreach: agentNames -->',
			"			<span class=\"new-teamschedule-agent-name relative\" data-bind=\"tooltip: { title: $data.name, trigger: 'click', placement: 'bottom'}, attr: {'index': index}, hideTooltipAfterMouseLeave: true\">",
			'				<span class="text-name" data-bind="text: $data.name"></span>',
			'				<span class="shift-category-cell" data-bind="text: $data.shiftCategory.name, style: {background: $data.shiftCategory.bgColor, color: $data.shiftCategory.color}, css: {\'dayoff\': $data.isDayOff}"></span>',
			'			</span>',
			'			<!-- /ko -->',
			'		</div>',
			'		<!-- ko if: isAgentScheduleLoaded() && teamSchedules().length == 0 -->',
			'		<div class="new-teamschedule-no-result-indicator">@Resources.NoResultForCurrentFilter</div>',
			'		<!-- /ko -->',
			'	</div>',
			'	<div class="new-teamschedule-table relative container">',
			'		<div class="mobile-timeline relative" data-bind="style: {height: scheduleContainerHeight() + \'px\'}">',
			'			<!-- ko foreach: timeLines -->',
			'			<div class="mobile-timeline-label absolute" data-bind="style: {top: topPosition}, text: timeText, visible: isHour">',
			'			</div>',
			'			<!-- /ko -->',
			'		</div>',
			'		<div class="my-schedule-column relative" data-bind="style: {height: scheduleContainerHeight() + \'px\'}">',
			'			<!-- ko if: mySchedule -->',
			'			<div class="new-teamschedule-layer-container relative">',
			'				<!-- ko foreach: mySchedule().layers -->',
			"				<div class=\"new-teamschedule-layer cursorpointer absolute\" data-bind=\"tooltip: { title: tooltipText, html: true, trigger: 'click' }, style: styleJson, css:{'overtime-layer': isOvertime, 'overtime-background-image-light': isOvertime && overTimeLighterBackgroundStyle, 'overtime-background-image-dark': isOvertime && overTimeDarkerBackgroundStyle, 'last-layer': isLastLayer}, hideTooltipAfterMouseLeave: true\">",
			'					<div class="activity-info" data-bind="visible: showTitle && !isOvertimeAvailability">',
			'						<strong data-bind="text: title"></strong>',
			'						<span class="fullwidth displayblock" data-bind="visible: showDetail, text: timeSpan"></span>',
			'						<!-- ko if: hasMeeting -->',
			'						<div class="meeting mt5">',
			'							<i class="meeting-icon mr10">',
			'								<i class="glyphicon glyphicon-user ml10"></i>',
			'							</i>',
			'						</div>',
			'						<!-- /ko -->',
			'					</div>',
			'					<div data-bind="visible: showTitle && isOvertimeAvailability">',
			'						<i class="glyphicon glyphicon-time"></i>',
			'					</div>',
			'				</div>',
			'				<!-- /ko -->',
			'				<!--ko if: mySchedule().isDayOff-->',
			'				<div class="dayoff cursorpointer relative" data-bind="tooltip: { title: mySchedule().dayOffName, html: true, trigger: \'click\'}, hideTooltipAfterMouseLeave: true">',
			'					<!-- ko if: mySchedule().layers && mySchedule().layers.length == 0 -->',
			'					<span class="dayoff-text" data-bind="text: mySchedule().dayOffName"></span>',
			'					<!-- /ko -->',
			'				</div>',
			'				<!--/ko-->',
			'				<!-- ko if: mySchedule().isNotScheduled -->',
			'				<div class="not-scheduled-text" data-bind="tooltip: { title: \'@Resources.NotScheduled\', html: true, trigger: \'click\'}, hideTooltipAfterMouseLeave: true">@Resources.NotScheduled</div>',
			'				<!-- /ko -->',
			'			</div>',
			'			<!-- /ko -->',
			'		</div>',
			'		<!-- ko if: teamSchedules -->',
			'		<div class="teammates-schedules-container relative" data-bind="style: {height: scheduleContainerHeight() + \'px\'}">',
			'			<div class="left-filling-block"></div>',
			'			<!-- ko foreach: teamSchedules -->',
			'			<div class="teammates-schedules-column relative" data-bind="attr: {\'index\': index}">',
			'				<div class="new-teamschedule-layer-container relative">',
			'					<!-- ko foreach: layers -->',
			"					<div class=\"new-teamschedule-layer cursorpointer absolute\" data-bind=\"tooltip: { title: tooltipText, html: true, trigger: 'click' }, style: styleJson, css:{'overtime-layer': isOvertime, 'overtime-background-image-light': isOvertime && overTimeLighterBackgroundStyle, 'overtime-background-image-dark': isOvertime && overTimeDarkerBackgroundStyle, 'last-layer': isLastLayer}, hideTooltipAfterMouseLeave: true\">",
			'						<div class="activity-info" data-bind="visible: showTitle && !isOvertimeAvailability">',
			'							<strong data-bind="text: title"></strong>',
			'							<span class="fullwidth displayblock" data-bind="visible: showDetail && false, text: timeSpan"></span>',
			'							<!-- ko if: hasMeeting -->',
			'							<div class="meeting">',
			'								<i class="meeting-icon mr10">',
			'									<i class="glyphicon glyphicon-user ml10"></i>',
			'								</i>',
			'							</div>',
			'							<!-- /ko -->',
			'						</div>',
			'						<div data-bind="visible: showTitle && isOvertimeAvailability">',
			'							<i class="glyphicon glyphicon-time"></i>',
			'						</div>',
			'					</div>',
			'					<!-- /ko -->',
			'					<!--ko if:isDayOff-->',
			'					<div class="dayoff cursorpointer relative" data-bind="tooltip: { title: dayOffName, html: true, trigger: \'click\' }, hideTooltipAfterMouseLeave: true">',
			'						<!-- ko if: layers && layers.length == 0 -->',
			'						<span class="dayoff-text" data-bind="text: dayOffName"></span>',
			'						<!-- /ko -->',
			'					</div>',
			'					<!--/ko-->',
			'					<!-- ko if:isNotScheduled -->',
			'					<div class="not-scheduled-text" data-bind="tooltip: { title: \'@Resources.NotScheduled\', html: true, trigger: \'click\'}, hideTooltipAfterMouseLeave: true">@Resources.NotScheduled</div>',
			'					<!-- /ko -->',
			'				</div>',
			'			</div>',
			'			<!-- /ko -->',
			'		</div>',
			'		<!-- /ko -->',
			'	</div>',
			'	<!-- ko if: isHostADesktop -->',
			'	<div class="teamschedule-scroll-block-container" data-bind="style: {border: isScrollbarVisible() ? \'1px dashed rgba(0, 0, 0, 0.1)\' : \'none\'}">',
			'		<!-- ko if: isScrollbarVisible -->',
			'		<div class="teamschedule-scroll-block">',
			'			<i class="glyphicon glyphicon-resize-horizontal"></i>',
			'		</div>',
			'		<!-- /ko -->',
			'	</div>',
			'	<!-- /ko -->',
			'	<!-- ko if: isPanelVisible -->',
			'	<div class="new-teamschedule-panel" data-bind="css: {\'new-teamschedule-panel-mobile\': isHostAMobile}">',
			'		<!-- ko if: isHostAMobile-->',
			'		<div class="new-teamschedule-filter-component">',
			'			<label>',
			'				@Resources.Team:',
			'			</label>',
			'			<select data-bind="foreach: availableTeams, select2: { value: selectedTeam}">',
			'				<optgroup data-bind="attr: { label: text }, foreach: children">',
			'					<option data-bind="text: text, value: id"></option>',
			'				</optgroup>',
			'			</select>',
			'		</div>',
			'		<form class="new-teamschedule-search-component" data-bind="submit: submitSearchForm">',
			'			<label>',
			'				@Resources.AgentName:',
			'			</label>',
			'			<input type="search" class="form-control" placeholder=\'@Resources.SearchHintForName\' data-bind="value: searchNameText" />',
			'			<input type="submit" style="display: none" />',
			'		</form>',
			'		<!--/ko-->',
			'		<div class="new-teamschedule-time-slider-block relative">',
			'			<!-- ko if: showOnlyDayOff() || showOnlyNightShift() -->',
			'			<div class="new-teamschedule-time-slider-block-backdrop"></div>',
			'			<!-- /ko -->',
			'			<div class="new-teamschedule-time-slider-container cursorpointer">',
			'				<label>@Resources.StartTime:</label>',
			'				<!-- ko if: isHostAMobile-->',
			'				<span class="start-time-clear-button cursorpointer floatright">',
			'					<i class="glyphicon glyphicon-remove"></i>',
			'				</span>',
			'				<!--/ko-->',
			'				<div class="new-teamschedule-time-slider-line relative">',
			'					<span class="start-time-slider-start-label" data-bind="text: startTimeStart, visible: showStartTimeStart"></span>',
			'					<span class="start-time-slider-end-label" data-bind="text: startTimeEnd"></span>',
			'					<div class="start-time-slider"></div>',
			'				</div>',
			'				<!-- ko ifnot: isHostAMobile-->',
			'				<span class="start-time-clear-button cursorpointer">',
			'					<i class="glyphicon glyphicon-remove"></i>',
			'				</span>',
			'				<!--/ko-->',
			'			</div>',
			'			<div class="new-teamschedule-time-slider-container cursorpointer">',
			'				<label>@Resources.EndTime:</label>',
			'				<!-- ko if: isHostAMobile-->',
			'				<span class="end-time-clear-button cursorpointer floatright">',
			'					<i class="glyphicon glyphicon-remove"></i>',
			'				</span>',
			'				<!--/ko-->',
			'				<div class="new-teamschedule-time-slider-line relative">',
			'					<span class="end-time-slider-start-label" data-bind="text: endTimeStart, visible: showEndTimeStart"></span>',
			'					<span class="end-time-slider-end-label" data-bind="text: endTimeEnd"></span>',
			'					<div class="end-time-slider"></div>',
			'				</div>',
			'				<!-- ko ifnot: isHostAMobile-->',
			'				<span class="end-time-clear-button cursorpointer">',
			'					<i class="glyphicon glyphicon-remove"></i>',
			'				</span>',
			'				<!--/ko-->',
			'			</div>',
			'		</div>',
			'		<!-- ko if: isHostAMobile-->',
			'		<div class="new-teamschedule-toggle show-only-night-shift-toggle">',
			'			<input type="checkbox" id="show-only-night-shift-switch" data-bind="checked: showOnlyNightShift" />',
			'			<label class="relative" for="show-only-night-shift-switch">Night shift switch</label>',
			'			<span>@Resources.ShowOnlyNightShifts</span>',
			'		</div>',
			'		<div class="new-teamschedule-toggle show-only-day-off-toggle">',
			'			<input type="checkbox" id="show-only-day-off-switch" data-bind="checked: showOnlyDayOff" />',
			'			<label class="relative" for="show-only-day-off-switch">Day off switch</label>',
			'			<span>@Resources.ShowOnlyDaysOff</span>',
			'		</div>',
			'		<div class="empty-search-result">',
			'			<!-- ko if: emptySearchResult -->',
			'			<label>',
			'				@Resources.NoResultForCurrentFilter',
			'			</label>',
			'			<!--/ko-->',
			'		</div>',
			'		<div class="new-teamschedule-submit-buttons">',
			'			<button class="btn btn-default" data-bind="click: cancelClick">@Resources.Cancel</button>',
			'			<button class="btn btn-primary" data-bind="click: submitSearchForm">@Resources.Search</button>',
			'		</div>',
			'		<!--/ko-->',
			'		<!-- ko if: isHostADesktop || isHostAniPad -->',
			'		<div class="new-teamschedule-submit-buttons clearfix">',
			'			<div class="new-teamschedule-toggle show-only-night-shift-toggle">',
			'				<input type="checkbox" id="show-only-night-shift-switch" data-bind="checked: showOnlyNightShift" />',
			'				<label class="relative" for="show-only-night-shift-switch">Night shift switch</label>',
			'				<span>@Resources.ShowOnlyNightShifts</span>',
			'			</div>',
			'			<div class="new-teamschedule-toggle show-only-day-off-toggle">',
			'				<input type="checkbox" id="show-only-day-off-switch" data-bind="checked: showOnlyDayOff" />',
			'				<label class="relative" for="show-only-day-off-switch">Day off switch</label>',
			'				<span>@Resources.ShowOnlyDaysOff</span>',
			'			</div>',
			'			<button class="btn btn-primary pull-right" data-bind="click: submitSearchForm">@Resources.Search</button>',
			'			<button class="btn btn-default pull-right" data-bind="click: cancelClick">@Resources.Cancel</button>',
			'		</div>',
			'		<!--/ko-->',
			'	</div>',
			'	<!-- /ko -->',
			'</div>'
		].join('');

		agentSchedulesHtml = '<div id="page"> ' + agentSchedulesHtml + '</div>';
	}

	function initVm() {
		Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		vm = Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.Vm();
	}

	function fakeReadyForInteractionCallback() { }

	function fakeCompletelyLoadedCallback() {
		completeLoadedCount++;
	}
});
