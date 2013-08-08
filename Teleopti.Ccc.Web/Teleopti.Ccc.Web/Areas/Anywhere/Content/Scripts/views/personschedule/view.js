
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions',
		'helpers',
		'text!templates/personschedule/view.html',
        'resizeevent',
        'pagelog'
	], function (
		ko,
		personScheduleViewModel,
		subscriptions,
		helpers,
		view,
	    resize,
	    log
	) {

	    var personSchedule;
	    
		return {
			initialize: function (options) {

			    log.log("initialize");
			    
				options.renderHtml(view);

				personSchedule = new personScheduleViewModel();

			    resize.onresize(function() {
			        personSchedule.TimeLine.WidthPixels($('.shift').width());
			    });
			    
				ko.applyBindings(personSchedule, options.bindingElement);

				log.log("/initialize");
			},
		    
			display: function(options) {

			    log.log("display");

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
					    log.log("subscribePersonSchedule");
					    resize.notify();
					    personSchedule.SetData(data);
					    personSchedule.Loading(false);
					    deferred.resolve();
					    log.log("/subscribePersonSchedule");
					}
				);

			    log.log("/display");
			    return deferred.promise();
			},

            dispose: function(options) {
                subscriptions.unsubscribePersonSchedule();
                $(".datepicker.dropdown-menu").remove();
            },
			
            clearaction: function(options) {
                personSchedule.AddingFullDayAbsence(false);
            },
			
            addfulldayabsence: function(options) {
                log.log("addfulldayabsence");
                personSchedule.AddingFullDayAbsence(true);
                log.log("/addfulldayabsence");
            },
			
            setDateFromTest: function (date) {
                personSchedule.AddFullDayAbsenceForm.EndDate(moment(date));
            }
		};
	});

