define([
		'jquery',
        'messagebroker',
        'signalrhubs',
        'helpers',
        'errorview'
	], function (
		$,
	    messagebroker,
	    signalRHubs,
	    helpers,
	    errorview
	) {
		
		var startPromise;

	    // why is signalr eating my exceptions?
		var logException = function (func) {
	        try {
	            func();
	        } catch (e) {
	            errorview.display(e);
	            throw e;
	        }
	    };
	    
		var teamScheduleHub = $.connection.teamScheduleHub;
	    teamScheduleHub.client.exceptionHandler = errorview.display;
	    var incomingTeamSchedule = null;
		var teamScheduleSubscription = null;
	    teamScheduleHub.client.incomingTeamSchedule = function (data) {
	        if (incomingTeamSchedule != null)
    	        logException(function() { incomingTeamSchedule(data); });
	    };
	    
	    var personScheduleHub = $.connection.personScheduleHub;
	    personScheduleHub.client.exceptionHandler = errorview.display;
	    var personScheduleSubscription = null;
	    var incomingPersonSchedule = null;
	    personScheduleHub.client.incomingPersonSchedule = function (data) {
	        if (incomingPersonSchedule != null)
    		    logException(function () { incomingPersonSchedule(data); });
		};

		var start = function () {
		    startPromise = messagebroker.start();
			return startPromise;
		};

	    var unsubscribePersonSchedule = function() {
	        if (!personScheduleSubscription)
	            return;
	        startPromise.done(function() {
	            incomingPersonSchedule = null;
	            messagebroker.unsubscribe(personScheduleSubscription);
	            personScheduleSubscription = null;
	        });
	    };

		var unsubscribeTeamSchedule = function() {
			if (!teamScheduleSubscription)
				return;
			startPromise.done(function() {
				incomingTeamSchedule = null;
				messagebroker.unsubscribe(teamScheduleSubscription);
				teamScheduleSubscription = null;
			});
		};
	    
	    return {
	        start: start,
	        
	        subscribeTeamSchedule: function (teamId, date, callback, isApplicableNotification) {
		        unsubscribeTeamSchedule();
	            incomingTeamSchedule = callback;
	            startPromise.done(function() {
	            	teamScheduleHub.server.subscribeTeamSchedule(teamId, date);

	            	teamScheduleSubscription = messagebroker.subscribe({
		            	domainReferenceType: 'Person',
		            	domainType: 'IPersonScheduleDayReadModel',
		            	callback: function (notification) {
		            		if (!isApplicableNotification(notification)) {
		            			return;
		            		}
		            		var momentDate = moment(date);
		            		var startDate = helpers.Date.ToMoment(notification.StartDate);
		            		var endDate = helpers.Date.ToMoment(notification.EndDate);
		            		if (momentDate.diff(startDate) >= 0 && momentDate.diff(endDate) <= 0) {
		            			teamScheduleHub.server.subscribeTeamSchedule(teamId, date);
		            		}
		            	}
		            });
	            });
	        },
	        
	        subscribePersonSchedule: function (personId, date, callback) {
	            unsubscribePersonSchedule();
	            incomingPersonSchedule = callback;
	            startPromise.done(function () {

	                personScheduleHub.server.personSchedule(personId, date);

	                personScheduleSubscription = messagebroker.subscribe({
	                    domainReferenceType: 'Person',
	                    domainReferenceId: personId,
	                    domainType: 'IPersonScheduleDayReadModel',
	                    callback: function(notification) {
	                        var momentDate = moment(date);
	                        var startDate = helpers.Date.ToMoment(notification.StartDate);
	                        var endDate = helpers.Date.ToMoment(notification.EndDate);
	                        if (momentDate.diff(startDate) >= 0 && momentDate.diff(endDate) <= 0) {
	                            personScheduleHub.server.personSchedule(personId, date);
                            }
	                    }
	                });
	                
	            });
	        },
	        
	        unsubscribePersonSchedule: unsubscribePersonSchedule,
	        unsubscribeTeamSchedule: unsubscribeTeamSchedule
	    };

	});
