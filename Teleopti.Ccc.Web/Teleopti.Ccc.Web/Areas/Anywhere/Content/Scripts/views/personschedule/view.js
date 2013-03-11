
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions',
		'helpers',
		'text!templates/personschedule/view.html',
	], function (
		ko,
		personScheduleViewModel,
		subscriptions,
		helpers,
		view
	) {

	    var personSchedule;
	    
		return {
			display: function (options) {

				options.renderHtml(view);

				personSchedule = new personScheduleViewModel();
				personSchedule.Id(options.id);
				personSchedule.Date(moment.utc(options.date, 'YYYYMMDD'));

				var resize = function () {
				    personSchedule.TimeLine.WidthPixels($('.shift').width());
				};

				$(window)
                    .resize(resize)
                    .bind('orientationchange', resize)
                    .ready(resize);

				subscriptions.subscribePersonSchedule(
					options.id,
					helpers.Date.AsUTCDate(personSchedule.Date().toDate()),
					function (data) {
						personSchedule.SetData(data);
					}
				);

				ko.applyBindings(personSchedule, options.bindingElement);

			},
			
            clearaction: function(options) {
                personSchedule.AddingFullDayAbsence(false);
            },
			
            addfulldayabsence: function(options) {
                personSchedule.AddingFullDayAbsence(true);
            }
		};
	});

