
define([
		'knockout',
		'jquery',
		'helpers',
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
		helpers,
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

				var timeLine = new timeLineViewModel();
				var teamSchedule = new teamScheduleViewModel(timeLine);

				var schedule = $.connection.scheduleHub;

				var resize = function () {
					timeLine.WidthPixels($('.shift').width());
				};

				$(window)
					.resize(resize)
					.bind('orientationchange', resize)
					.ready(resize);

				var loadSchedules = function () {
					schedule.server.subscribeTeamSchedule('34590A63-6331-4921-BC9F-9B5E015AB495', $('#date-selection').val()).done(function (schedules) {
						timeLine.Agents.removeAll();
						teamSchedule.Agents.removeAll();

						ko.utils.arrayForEach(schedules, function (s) {
							var agent = new agentViewModel(timeLine, s);
							teamSchedule.AddAgent(agent);
						});

						teamSchedule.Agents.sort(function (a, b) {
							var firstStartMinutes = a.FirstStartMinute();
							var secondStartMinutes = b.FirstStartMinute();
							return firstStartMinutes == secondStartMinutes ? (a.LastEndMinute() == b.LastEndMinute() ? 0 : a.LastEndMinute() < b.LastEndMinute() ? -1 : 1) : firstStartMinutes < secondStartMinutes ? -1 : 1;
						});

						resize();
					});
				};

				$('#date-selection').val(moment().format('YYYY-MM-DD'));
				$.connection.hub.url = 'signalr';
				$.connection.hub.start()
					.done(function () {
						loadSchedules();

						ko.applyBindings({
							TeamSchedule: teamSchedule
						});
					});

				/*
				$('.agent').click(function () {
				navigation.GotoAgentSchedule($(this).data('agent-id'), $('#date-selection').attr('value'));
				});
				*/

				$('.team-schedule').swipeListener({
					swipeLeft: function () {
						var dateValue = $('#date-selection').attr('value');
						var date = moment(dateValue).add('d', 1);
						$('#date-selection').attr('value', date.format('YYYY-MM-DD'));
						loadSchedules();
					},
					swipeRight: function () {
						var dateValue = $('#date-selection').attr('value');
						var date = moment(dateValue).add('d', -1);
						$('#date-selection').attr('value', date.format('YYYY-MM-DD'));
						loadSchedules();
					}
				});

				$('#date-selection').datepicker({
					pullRight: true,
					format: 'yyyy-mm-dd',
					weekStart: 1,
					autoclose: true
				}).on('changeDate', loadSchedules);
			}
		};
	});

