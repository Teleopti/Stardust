
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions.personschedule',
		'subscriptions.groupschedule',
		'subscriptions.unsubscriber',
		'helpers',
		'text!templates/personschedule/view.html',
		'resizeevent',
		'jqueryui'
], function (
		ko,
		personScheduleViewModel,
		personsubscriptions,
		groupsubscriptions,
		unsubscriber,
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
					if (receivedSchedules.length === data.TotalCount) {
						viewModel.UpdateSchedules({ BaseDate: data.BaseDate, Schedules: receivedSchedules });
						resize.notify();
						groupScheduleDeferred.resolve();
						receivedSchedules = [];
					}
				}
			);

			return $.when(personScheduleDeferred, groupScheduleDeferred)
				.done(function () {
					viewModel.Loading(false);
					if (viewModel.MovingActivity()) {
						viewModel.initActivityDraggable();
					}
			});
		},

		dispose: function (options) {
			unsubscriber.unsubscribeAdherence();
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

