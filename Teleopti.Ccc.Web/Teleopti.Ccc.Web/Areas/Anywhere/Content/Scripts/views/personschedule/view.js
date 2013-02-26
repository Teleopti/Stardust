
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

				options.startedPromise.done(function () {
					var data = {
						Id: options.id,
						Name: "Mathias Syenbom",
						Site: "Moon",
						Team: "A-Team"
					};
					
					//					hub
					//						.server
					//						.subscribePersonScheduleViewModel(options.id, options.date)
					//						.done(function (data) {
					personSchedule.SetData(data);
					//						});
				});

				ko.applyBindings(personSchedule, options.bindingElement);
			}
		};
	});

