
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
			initialize: function (options) {

				options.renderHtml(view);

				personSchedule = new personScheduleViewModel();

				var resize = function () {
				    personSchedule.TimeLine.WidthPixels($('.shift').width());
				};

				$(window)
                    .resize(resize)
                    .bind('orientationchange', resize)
                    .ready(resize);

				ko.applyBindings(personSchedule, options.bindingElement);

			},
		    
			display: function(options) {

			    var date = moment(options.date, 'YYYYMMDD');
			    
			    if (personSchedule.Id() == options.id && personSchedule.Date().diff(date) == 0)
			        return;

			    personSchedule.Loading(true);
			    
			    personSchedule.Id(options.id);
			    personSchedule.Date(date);

			    subscriptions.subscribePersonSchedule(
					options.id,
					helpers.Date.AsUTCDate(personSchedule.Date().toDate()),
					function (data) {
					    personSchedule.SetData(data);
					    personSchedule.Loading(false);
					}
				);

			},

            clearaction: function(options) {
                personSchedule.AddingFullDayAbsence(false);
            },
			
            addfulldayabsence: function(options) {
                personSchedule.AddingFullDayAbsence(true);
            }
		};
	});

