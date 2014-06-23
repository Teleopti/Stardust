
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions.personschedule',
		'subscriptions.groupschedule',
		'helpers',
		'text!templates/personschedule/view.html',
		'resizeevent',
		'jqueryui'
], function (
		ko,
		personScheduleViewModel,
		personsubscriptions,
		groupsubscriptions,
		helpers,
		view,
		resize
	) {

	var viewModel;

	return {
		initialize: function (options) {

			options.renderHtml(view);
			viewModel = new personScheduleViewModel();

			resize.onresize(function () {
				viewModel.TimeLine.WidthPixels($('.time-line-for').width());
			});

			ko.applyBindings(viewModel, options.bindingElement);
		},

		display: function (options) {
			viewModel.Loading(true);

			viewModel.SetViewOptions(options);
			
			var personScheduleDeferred = $.Deferred();
			personsubscriptions.subscribePersonSchedule(
				viewModel.PersonId(),
				helpers.Date.ToServer(viewModel.ScheduleDate()),
				function (data) {
					viewModel.UpdateData(data);
					resize.notify();
					personScheduleDeferred.resolve();
				}
			);

			var groupScheduleDeferred = $.Deferred();
			groupsubscriptions.subscribeGroupSchedule(
				viewModel.GroupId(),
				helpers.Date.ToServer(viewModel.ScheduleDate()),
				function (data) {
					viewModel.UpdateSchedules(data);
					resize.notify();
					groupScheduleDeferred.resolve();
				}
			);

			return $.when(personScheduleDeferred, groupScheduleDeferred)
				.done(function () {
					viewModel.Loading(false);

					// bind events
					var activeLayer = $(".layer.active");
					if (activeLayer.length !== 0) {
						var pixelByNMinutes = viewModel.lengthMinutesToPixels(15);
						var initialX = $(".layer.active")[0].offsetLeft;
						$(".layer.active").draggable({
							axis: 'x',
							containment: 'parent',
							grid: [pixelByNMinutes, 0],
							drag: function (e) {
								var pixelsChanged = e.target.offsetLeft - initialX;
								viewModel.updateStartTime(pixelsChanged);
							}
						});
					}
				});
		},

		dispose: function (options) {
			personsubscriptions.unsubscribePersonSchedule();
			$(".datepicker.dropdown-menu").remove();
		},

		clearaction: function (options) {
			viewModel.AddingFullDayAbsence(false);
			viewModel.AddingActivity(false);
			viewModel.AddingIntradayAbsence(false);
			viewModel.MovingActivity(false);
		},

		addfulldayabsence: function (options) {
			viewModel.AddingFullDayAbsence(true);
		},

		addactivity: function (options) {
			viewModel.AddingActivity(true);
		},

		addintradayabsence: function (options) {
			viewModel.AddingIntradayAbsence(true);
		},

		moveactivity: function (options) {
			viewModel.MovingActivity(true);
			
		},

		setDateFromTest: function (date) {
			viewModel.AddFullDayAbsenceForm.EndDate(moment(date));
		}
	};
});

