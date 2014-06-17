
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions.personschedule',
		'subscriptions.groupschedule',
		'helpers',
		'text!templates/personschedule/view.html',
		'resizeevent'
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
			if(options.time)
				viewModel.SelectedStartTime(moment(options.time, 'HHmm'));

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

