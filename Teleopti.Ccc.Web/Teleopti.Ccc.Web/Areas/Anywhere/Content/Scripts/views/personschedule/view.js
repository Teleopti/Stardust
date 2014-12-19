
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
		resize,
		jqueryui
	) {

	var viewModel;

	return {
		initialize: function (options) {
			options.renderHtml(view);
			viewModel = new personScheduleViewModel();

			resize.onresize(function () {
				viewModel.setTimelineWidth($('.time-line-for').width());
			});
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(viewModel, options.bindingElement);
		},

		display: function (options) {

			viewModel.Loading(true);
			viewModel.SetViewOptions(options);
			
			var personScheduleDeferred = $.Deferred();
			personsubscriptions.subscribePersonSchedule(
				viewModel.BusinessUnitId(),
				viewModel.PersonId(),
				helpers.Date.ToServer(viewModel.ScheduleDate()),
				function (data) {
					viewModel.UpdateData(data);
					resize.notify();
					personScheduleDeferred.resolve();
				}
			);

			var groupScheduleDeferred = $.Deferred();
			var receivedSchedules = [];
			groupsubscriptions.subscribeGroupSchedule(
				viewModel.BusinessUnitId(),
				viewModel.GroupId(),
				helpers.Date.ToServer(viewModel.ScheduleDate()),
				function(personId) {
					return personId == viewModel.PersonId();
				},
				function (data) {
					receivedSchedules.push.apply(receivedSchedules, data.Schedules);
					if (receivedSchedules.length == data.TotalCount) {
						viewModel.UpdateSchedules({ BaseDate: data.BaseDate, Schedules: receivedSchedules });
						resize.notify();
						groupScheduleDeferred.resolve();
					}
				}
			);

			return $.when(personScheduleDeferred, groupScheduleDeferred)
				.done(function () {
					viewModel.Loading(false);
					if (viewModel.MovingActivity()) {
					    $('.time-line-for').attr("data-subscription-done", " ");
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
									var workingShift = viewModel.WorkingShift();
									var minStartPixel = (workingShift.OriginalShiftStartMinutes - viewModel.TimeLine.StartMinutes()) * viewModel.TimeLine.PixelsPerMinute();
									var maxEndPixel = (workingShift.OriginalShiftEndMinutes - viewModel.TimeLine.StartMinutes()) * viewModel.TimeLine.PixelsPerMinute();
									var pixelTolerance = 1; /*make activity can be moved to the beginning or the end of the shift. bug 30603*/
									if (ui.position.left + ui.helper[0].offsetWidth <= (maxEndPixel + pixelTolerance) &&
										ui.position.left >= (minStartPixel - pixelTolerance)) {
									    var pixelsChanged = ui.position.left - ui.originalPosition.left;
                                        viewModel.updateStartTime(pixelsChanged);
										if (viewModel.MoveActivityForm.isMovingToAnotherDay()) 
										    viewModel.MoveActivityForm.reset();
									}
								}
							});
						}
						viewModel.MoveActivityForm.reset();
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

