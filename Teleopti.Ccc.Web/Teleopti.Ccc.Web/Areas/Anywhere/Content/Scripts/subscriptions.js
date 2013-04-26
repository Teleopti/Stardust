define([
		'jquery',
        'messagebroker',
        'signalrhubs',
        'helpers'
	], function (
		$,
	    messagebroker,
	    signalRHubs,
	    helpers
	) {
		
		var startPromise;

	    // why is signalr eating my exceptions?
		var logException = function (func) {
	        try {
	            func();
	        } catch (e) {
	            console.log(e);
	            throw e;
	        }
	    };
	    
		var teamScheduleHub = $.connection.teamScheduleHub;
	    var incomingTeamSchedule = null;
	    teamScheduleHub.client.incomingTeamSchedule = function (data) {
	        if (incomingTeamSchedule != null)
    	        logException(function() { incomingTeamSchedule(data); });
	    };
	    
	    var personScheduleHub = $.connection.personScheduleHub;
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

	    return {
	        start: start,
	        
	        subscribeTeamSchedule: function(teamId, date, callback) {
	            incomingTeamSchedule = callback;
	            startPromise.done(function() {
	                teamScheduleHub.server.subscribeTeamSchedule(teamId, date);
	            });
	        },
	        
	        subscribePersonSchedule: function (personId, date, callback) {
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
	                        if (momentDate.diff(startDate) >= 0 && momentDate.diff(endDate) <= 0)
	                            personScheduleHub.server.personSchedule(personId, date);
	                    }
	                });
	                
	            });
	        },
	        
	        unsubscribePersonSchedule: function () {
	            startPromise.done(function() {
	                incomingPersonSchedule = null;
	                messagebroker.unsubscribe(personScheduleSubscription);
	            });
	        }
	    };

	});
