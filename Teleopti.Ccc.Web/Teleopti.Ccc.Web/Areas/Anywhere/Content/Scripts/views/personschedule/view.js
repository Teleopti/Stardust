
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions',
		'text!templates/personschedule/view.html'
	], function (
		ko,
		personScheduleViewModel,
		subscriptions,
		view
	) {

		//var hub = $.connection.personScheduleHub;
		var personSchedule = new personScheduleViewModel();

		return {
			display: function (options) {

				options.renderHtml(view);

				personSchedule.Date(moment(options.date, 'YYYYMMDD'));

				subscriptions.subscribePersonSchedule(
					options.id,
					personSchedule.Date().toDate(),
					function(data) {
						personSchedule.SetData(data);
					}
				);

//				options.startedPromise.done(function () {
//					hub
//						.server
//						.subscribePersonScheduleViewModel(options.id, personSchedule.Date().toDate())
//						.done(function (data) {
//							personSchedule.SetData(data);
//						});
//				});

				ko.applyBindings(personSchedule, options.bindingElement);
			}
		};
	});

