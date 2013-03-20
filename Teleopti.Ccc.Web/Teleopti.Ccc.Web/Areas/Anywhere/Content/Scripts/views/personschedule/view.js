
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions',
		'helpers',
		'text!templates/personschedule/view.html'
	], function (
		ko,
		personScheduleViewModel,
		subscriptions,
		helpers,
		view
	) {

		var personSchedule = new personScheduleViewModel();

		var resize = function () {
			personSchedule.TimeLine.WidthPixels($('.shift').width());
		};

		$(window)
			.resize(resize)
			.bind('orientationchange', resize)
			.ready(resize);

		return {
			display: function (options) {

				options.renderHtml(view);

				personSchedule.Id(options.id);
				personSchedule.Date(moment.utc(options.date, 'YYYYMMDD'));

				subscriptions.subscribePersonSchedule(
					options.id,
					helpers.Date.AsUTCDate(personSchedule.Date().toDate()),
					function (data) {
						personSchedule.SetData(data);
					}
				);

				ko.applyBindings(personSchedule, options.bindingElement);
			}
		};
	});

