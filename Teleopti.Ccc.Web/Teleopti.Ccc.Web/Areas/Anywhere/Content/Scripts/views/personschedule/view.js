
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions',
		'helpers',
		'text!templates/personschedule/view.html',
	'resizeevent'
], function (
		ko,
		personScheduleViewModel,
		subscriptions,
		helpers,
		view,
	    resize
	) {

	var personSchedule;

	return {
		initialize: function (options) {

			options.renderHtml(view);

			personSchedule = new personScheduleViewModel();

			resize.onresize(function () {
				personSchedule.TimeLine.WidthPixels($('.time-line-for').width());

			});

			ko.applyBindings(personSchedule, options.bindingElement);

		},

		display: function (options) {

			var deferred = $.Deferred();

			var date = moment(options.date, 'YYYYMMDD');

			if (personSchedule.Id() == options.id && personSchedule.Date().diff(date) == 0)
				return;

			personSchedule.Loading(true);

			personSchedule.Id(options.id);
			personSchedule.Date(date);

			subscriptions.subscribePersonSchedule(
				    options.id,
				    helpers.Date.ToServer(personSchedule.Date()),
				    function (data) {
				    	resize.notify();
				    	personSchedule.SetData(data);
				    	personSchedule.Loading(false);
				    	deferred.resolve();
				    }
			    );

			return deferred.promise();
		},

		dispose: function (options) {
			subscriptions.unsubscribePersonSchedule();
			$(".datepicker.dropdown-menu").remove();
		},

		clearaction: function (options) {
			personSchedule.AddingFullDayAbsence(false);
			personSchedule.AddingActivity(false);
		},

		addfulldayabsence: function (options) {
			personSchedule.AddingFullDayAbsence(true);
		},

		addactivity: function (options) {
			personSchedule.AddingActivity(true);
		},

		setDateFromTest: function (date) {
			personSchedule.AddFullDayAbsenceForm.EndDate(moment(date));
		}
	};
});

