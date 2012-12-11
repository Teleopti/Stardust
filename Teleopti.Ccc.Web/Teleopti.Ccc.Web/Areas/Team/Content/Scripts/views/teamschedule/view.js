
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
				schedule.client.teamScheduleLoaded = function (schedules) {

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
				};

				var resize = function () {
					timeLine.WidthPixels($('.shift').width());
				};

				$(window)
					.resize(resize)
					.bind('orientationchange', resize)
					.ready(resize);

				$('#date-selection').val(moment().format('YYYY-MM-DD'));
				$.connection.hub.url = 'signalr';
				$.connection.hub.start()
					.done(function () {
						schedule.server.subscribeTeamSchedule('34590A63-6331-4921-BC9F-9B5E015AB495', $('#date-selection').val());
					});

				/*
				$('.agent').click(function () {
				navigation.GotoAgentSchedule($(this).data('agent-id'), $('#date-selection').attr('value'));
				});
				*/

				ko.applyBindings({
					TeamSchedule: teamSchedule
				});

				$('.team-schedule').swipeListener({
					swipeLeft: function () {
						var dateValue = $('#date-selection').attr('value');
						var date = moment(dateValue).add('d', 1);
						$('#date-selection').attr('value', date.format('YYYY-MM-DD'));
						schedule.server.subscribeTeamSchedule('34590A63-6331-4921-BC9F-9B5E015AB495', $('#date-selection').val());
					},
					swipeRight: function () {
						var dateValue = $('#date-selection').attr('value');
						var date = moment(dateValue).add('d', -1);
						$('#date-selection').attr('value', date.format('YYYY-MM-DD'));
						schedule.server.subscribeTeamSchedule('34590A63-6331-4921-BC9F-9B5E015AB495', $('#date-selection').val());
					}
				});

				$('#date-selection').datepicker({
					pullRight: true,
					format: 'yyyy-mm-dd',
					weekStart: 1,
					autoclose: true
				}).on('changeDate', function (ev) {
					schedule.server.subscribeTeamSchedule('34590A63-6331-4921-BC9F-9B5E015AB495', $('#date-selection').val());
				});
			}
		};
	});

