
define([
		'knockout',
		'jquery',
		'navigation',
		'signalrHubs',
		'swipeListener',
		'moment',
		'datepicker',
		'views/teamschedule/vm',
		'views/teamschedule/timeline',
		'views/teamschedule/agent',
		'text!templates/teamschedule/view.html'
	], function (
		ko,
		$,
		swipeListener,
		momentX,
		datepicker,
		navigation,
		signalrHubs,
		teamScheduleViewModel,
		timeLineViewModel,
		agentViewModel,
		view
	) {
		return {
			display: function (options) {

				options.renderHtml(view);

				var date = options.date;
				if (date == undefined) {
					date = moment();
				} else {
					date = moment(date, 'YYYYMMDD');
				}

				var timeLine = new timeLineViewModel();
				var teamSchedule = new teamScheduleViewModel(timeLine, date.toDate());

				var schedule = $.connection.scheduleHub;

				var resize = function () {
					timeLine.WidthPixels($('.shift').width());
				};

				$(window)
					.resize(resize)
					.bind('orientationchange', resize)
					.ready(resize);

				var loadSchedules = function () {
					schedule.server.subscribeTeamSchedule(teamSchedule.SelectedTeam().Id, teamSchedule.SelectedDate()).done(function (schedules) {
						timeLine.Agents.removeAll();
						teamSchedule.Agents.removeAll();

						var newItems = ko.utils.arrayMap(schedules, function (s) {
							return new agentViewModel(timeLine, s);
						});
						teamSchedule.AddAgents(newItems);

						teamSchedule.Agents.sort(function (a, b) {
							var firstStartMinutes = a.FirstStartMinute();
							var secondStartMinutes = b.FirstStartMinute();
							return firstStartMinutes == secondStartMinutes ? (a.LastEndMinute() == b.LastEndMinute() ? 0 : a.LastEndMinute() < b.LastEndMinute() ? -1 : 1) : firstStartMinutes < secondStartMinutes ? -1 : 1;
						});

						resize();
					});
				};

				var loadAvailableTeams = function () {
					schedule.server.availableTeams(teamSchedule.SelectedDate()).done(function (details) {
						teamSchedule.AddTeams(details.Teams);
					});
				};

				$.connection.hub.url = 'signalr';
				$.connection.hub.start()
					.done(function () {
						loadAvailableTeams();

						ko.applyBindings({
							TeamSchedule: teamSchedule
						});
					});

				$('.team-schedule').swipeListener({
					swipeLeft: function () {
						teamSchedule.NextDay();
					},
					swipeRight: function () {
						teamSchedule.PreviousDay();
					}
				});

				teamSchedule.SelectedDate.subscribe(function () {
					loadSchedules();
				});

				teamSchedule.SelectedTeam.subscribe(function () {
					loadSchedules();
				});

				teamSchedule.SelectedTeam(teamSchedule.Teams()[0]);
			}
		};
	});

