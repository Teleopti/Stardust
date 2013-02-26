
define([
		'knockout',
		'views/personschedule/vm',
		'text!templates/personschedule/view.html'
	], function (
		ko,
		personScheduleViewModel,
		view
	) {

		var hub = $.connection.personScheduleHub;
		var personSchedule = new personScheduleViewModel();

		return {
			display: function (options) {

				options.renderHtml(view);

				personSchedule.Date(moment(options.date, 'YYYYMMDD'));

				options.startedPromise.done(function() {
					hub
						.server
						.subscribePersonScheduleViewModel(options.id, personSchedule.Date().toDate())
						.done(function(data) {
							personSchedule.SetData(data);
						});
				});

				ko.applyBindings(personSchedule, options.bindingElement);
			}
		};
	});

