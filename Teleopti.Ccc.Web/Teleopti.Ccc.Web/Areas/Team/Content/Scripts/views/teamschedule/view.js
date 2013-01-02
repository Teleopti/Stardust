
define([
		'knockout',
		'jquery',
		'navigation',
		'signalrHubs',
		'swipeListener',
		'moment',
		'views/teamschedule/vm',
		'views/teamschedule/timeline',
		'views/teamschedule/agent',
		'text!templates/teamschedule/view.html'
	], function (
		ko,
		$,
		swipeListener,
		momentX,
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
					date = moment().sod();
				} else {
					date = moment(date, 'YYYYMMDD');
				}

				var timeLine = new timeLineViewModel();
				var teamSchedule = new teamScheduleViewModel(timeLine, date);

				var previousOffset;
				var teamScheduleContainer = $('.team-schedule');

				var schedule = $.connection.scheduleHub;

				var resize = function () {
					previousOffset = teamScheduleContainer.offset().left;
					timeLine.WidthPixels($('.shift').width());
				};

				$(window)
					.resize(resize)
					.bind('orientationchange', resize)
					.ready(resize);

				var utcFromMoment = function (momentDate) {
					return new Date(Date.UTC(momentDate.year(), momentDate.month(), momentDate.date()));
				};

				var loadSchedules = function () {
					var queryDate = teamSchedule.SelectedDate();
					queryDate.utc();

					teamSchedule.isLoading(true);
					schedule.server.subscribeTeamSchedule(teamSchedule.SelectedTeam().Id, queryDate.toDate()).done(function (schedules) {
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

						teamSchedule.isLoading(false);
						resize();
					});
				};

				var loadAvailableTeams = function () {
					schedule.server.availableTeams(utcFromMoment(teamSchedule.SelectedDate())).done(function (details) {
						teamSchedule.AddTeams(details.Teams);

						teamSchedule.TeamDateCombination.subscribe(function () {
							loadSchedules();
						});

						teamSchedule.SelectedTeam(teamSchedule.Teams()[0]);
					});
				};

				$.connection.hub.url = 'signalr';
				$.connection.hub.start()
					.done(function () {
						loadAvailableTeams();

						ko.applyBindings({
							TeamSchedule: teamSchedule
						});
					})
					.fail(function (error) {
						$('.container > .row:first').html('<div class="alert"><button type="button" class="close" data-dismiss="alert">&times;</button><strong>Warning!</strong> ' + error + '.</div>');
					});

				teamScheduleContainer.swipeListener({
					swipeLeft: function () {
						teamScheduleContainer.offset({ left: previousOffset });
						teamSchedule.NextDay();
					},
					swipeRight: function () {
						teamScheduleContainer.offset({ left: previousOffset });
						teamSchedule.PreviousDay();
					},
					swipeMove: function (movementX, movementY) {
						teamScheduleContainer.offset({ left: -movementX });
					}
				});
			}
		};
	});

