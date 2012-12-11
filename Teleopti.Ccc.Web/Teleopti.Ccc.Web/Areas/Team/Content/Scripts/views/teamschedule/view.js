
define([
		'knockout',
		'jquery',
		'helpers',
		'navigation',
		'signalrHubs',
		'swipeListener',
		'moment',
		'views/teamschedule/fakeData',
		'views/teamschedule/vm',
		'views/teamschedule/timeline',
		'text!templates/teamschedule/view.html'
	], function (
		ko,
		$,
		helpers,
		swipeListener,
		momentX,
		navigation,
		signalrHubs,
		fakeData,
		teamScheduleViewModel, timeLineViewModel,
		view
	) {
		return {
			display: function (options) {

				options.renderHtml(view);

				$('#date-selection').datepicker({
					pullRight: true,
					format: 'yyyy-mm-dd',
					weekStart: 1
				}).on('changeDate', function (ev) {
					$('#date-selection').datepicker('hide');
				});

				var timeLine = new timeLineViewModel();

				var teamSchedule = new teamScheduleViewModel(timeLine);

				var schedule = $.connection.scheduleHub;
				schedule.client.teamScheduleLoaded = function (schedules) {
					ko.utils.arrayForEach(schedules, function (s) { teamSchedule.AddAgent(s); });
				};

				$.connection.hub.start()
					.done(function () { schedule.server.subscribeTeamSchedule('34590A63-6331-4921-BC9F-9B5E015AB495', $('#date-selection').val()); });

				var resize = function () {
					timeLine.WidthPixels($('.shift').width());
				};
				$(window)
					.resize(resize)
					.bind('orientationchange', resize)
					.ready(resize);

				ko.applyBindings({
					TeamSchedule: teamSchedule
				});

				/*
				$('.agent').click(function () {
				navigation.GotoAgentSchedule($(this).data('agent-id'), $('#date-selection').attr('value'));
				});

				$('.resources').click(function () {
				navigation.GotoResources($('#date-selection').attr('value'));
				});

				$('.resource-layer').tooltip();
				*/

				$('.team-schedule').swipeListener({
					swipeLeft: function () {
						var dateValue = $('#date-selection').attr('value');
						var date = moment(dateValue).add('d', -1);
						$('#date-selection').attr('value', date.format('YYYY-MM-DD'));
					},
					swipeRight: function () {
						var dateValue = $('#date-selection').attr('value');
						var date = moment(dateValue).add('d', 1);
						$('#date-selection').attr('value', date.format('YYYY-MM-DD'));
					}
				});
			}
		};

	});

