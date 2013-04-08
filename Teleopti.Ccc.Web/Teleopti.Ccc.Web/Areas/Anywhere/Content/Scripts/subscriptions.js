define([
		'jquery',
		'noext!../../../../signalr/hubs',
        'messagebroker',
        'helpers'
	], function (
		$,
		signalrHubs,
	    messagebroker,
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
	    var incomingTeamSchedule = function() { };
	    teamScheduleHub.client.incomingTeamSchedule = function (data) {
	        logException(function() { incomingTeamSchedule(data); });
	    };
	    
		var personScheduleHub = $.connection.personScheduleHub;
		var incomingPersonSchedule = function() { };
		personScheduleHub.client.incomingPersonSchedule = function(data) {
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

	                messagebroker.subscribe({
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
	        }
	    };

	});
