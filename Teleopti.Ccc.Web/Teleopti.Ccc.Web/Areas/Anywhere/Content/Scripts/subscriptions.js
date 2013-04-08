define([
		'jquery',
		'noext!../../../../signalr/hubs',
        'messagebroker'
	], function (
		$,
		signalrHubs,
	    messagebroker
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
	            startPromise.done(function() {
	                personScheduleHub.server.personSchedule(personId, date);
	            });
	            messagebroker.subscribe({
	                domainType: 'IPersonScheduleDayReadModel',
	                callback: function (notification) {
	                    personScheduleHub.server.personSchedule(personId, date);
	                }
	            });
	        }
	    };

	});
