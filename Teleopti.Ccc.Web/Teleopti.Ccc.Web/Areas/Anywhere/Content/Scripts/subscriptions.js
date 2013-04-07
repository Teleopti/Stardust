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

		//var subscription = function (options) {

		//	var self = this;

		//	this.subscribeMethod = options.subscribeMethod;
			
		//	this.incomingData = function (data) {
		//		if (self.callback) {
		//			try {
		//				self.callback(data);
		//			} catch(e) {
		//				// why is signalr eating my exceptions?
		//				console.log(e);
		//				throw e;
		//			} 
		//		}
		//	};

		//	options.clientIncomingMethodSetter(this.incomingData);

		//	this.subscribe = function() {
		//		var argumentsArray = Array.prototype.slice.call(arguments);
		//		var serverArguments = argumentsArray.slice(0, arguments.length - 1);
		//		var callbackArgument = arguments[arguments.length - 1];
		//		self.callback = callbackArgument;
		//		startPromise.done(function() {
		//			self.subscribeMethod.apply(self, serverArguments);
		//		});
		//	};

		//};

		//var subscriptions = [];

		//subscriptions.push(new subscription({
		//    subscribeMethod: teamScheduleHub.server.subscribeTeamSchedule,
		//    clientIncomingMethodSetter: function (method) {
		//        teamScheduleHub.client.incomingTeamSchedule = method;
		//    }
		//}));

		//subscriptions.push(new subscription({
		//    subscribeMethod: personScheduleHub.server.subscribePersonSchedule,
		//    clientIncomingMethodSetter: function (method) {
		//        personScheduleHub.client.incomingPersonSchedule = method;
		//    }
		//}));

		var teamScheduleHub = $.connection.teamScheduleHub;
	    var incomingTeamSchedule = function() { };
	    teamScheduleHub.client.incomingTeamSchedule = function(data) {
	        try {
	            incomingTeamSchedule(data);
	        } catch (e) {
	            // why is signalr eating my exceptions?
	            console.log(e);
	            throw e;
	        } 
	    };
	    
		var personScheduleHub = $.connection.personScheduleHub;
		var incomingPersonSchedule = function() { };
		personScheduleHub.client.incomingPersonSchedule = function(data) {
		    try {
		        incomingPersonSchedule(data);
		    } catch (e) {
		        // why is signalr eating my exceptions?
		        console.log(e);
		        throw e;
		    }
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
	        subscribePersonSchedule: function(personId, date, callback) {
	            incomingPersonSchedule = callback;
	            startPromise.done(function() {
	                personScheduleHub.server.subscribePersonSchedule(personId, date);
	            });
	            messagebroker.subscribe({
	                domainType: 'IPersonScheduleDayReadModel',
	                callback: function (notification) {
	                    personScheduleHub.server.publishPersonSchedule(personId, date);
	                }
	            });
	        },
	        //subscribeTeamSchedule: subscriptions[0].subscribe,
		    //subscribePersonSchedule: subscriptions[1].subscribe,
	    };

	});
