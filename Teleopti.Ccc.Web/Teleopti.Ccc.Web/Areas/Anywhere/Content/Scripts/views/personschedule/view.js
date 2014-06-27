
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
				viewModel.setTimelineWidth($('.time-line-for').width());
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
					$('.time-line-for').attr("data-subscription-done", " ");
					if (viewModel.MovingActivity) { 
						// bind events
						var activeLayer = $(".layer.active");
						if (activeLayer.length !== 0) {

							$(".layer.active").draggable({
								helper: 'clone',
								cursor: "move",
								zIndex: 100,
								stack: ".layer",
								axis: 'x',
								containment: 'parent',
								stop: function (e, ui) {
									var workingShiftLayers = viewModel.WorkingShift().Layers().sort(function(a,b) { return a.StartPixels() > b.StartPixels(); });
									var minStartPixel = workingShiftLayers[0].StartPixels();
									var lastLayer = workingShiftLayers[workingShiftLayers.length - 1];
									var maxEndPixel = lastLayer.StartPixels() + lastLayer.LengthPixels();
									if (ui.position.left + ui.helper[0].offsetWidth <= maxEndPixel && 
										ui.position.left >= minStartPixel) {
										var pixelsChanged = ui.position.left - ui.originalPosition.left;
										viewModel.updateStartTime(pixelsChanged);
									}
								}
							});
						}
						viewModel.InitMoveActivityForm();
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

