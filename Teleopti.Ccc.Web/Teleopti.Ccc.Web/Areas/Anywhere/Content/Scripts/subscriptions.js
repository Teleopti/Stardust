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

		var dailyStaffingMetricsSubscription = null;

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
		
		var unsubscribeDailyStaffingMetrics = function () {
			if (!dailyStaffingMetricsSubscription)
				return;
			startPromise.done(function () {
				messagebroker.unsubscribe(dailyStaffingMetricsSubscription);
				dailyStaffingMetricsSubscription = null;
			});
		};

		var loadDailyStaffingMetrics = function(date, skillId, callback) {
			$.ajax({
				url: 'StaffingMetrics/DailyStaffingMetrics',
				cache: false,
				dataType: 'json',
				data: {
					skillId: skillId,
					date: date
				},
				success: function(data, textStatus, jqXHR) {
					callback(data);
				}
			});
		};

		var isMatchingDates = function(date, notificationStartDate, notificationEndDate) {
			var momentDate = moment(date);
			var startDate = helpers.Date.ToMoment(notificationStartDate).startOf('day');
			var endDate = helpers.Date.ToMoment(notificationEndDate).startOf('day');

			if (momentDate.diff(startDate) >= 0 && momentDate.diff(endDate) <= 0) return true;

			return false;
		};

	    return {
	        start: start,
	        
	    	subscribeDailyStaffingMetrics: function(date, skillId, callback) {
	    		unsubscribeDailyStaffingMetrics();
	    		startPromise.done(function () {
	    			loadDailyStaffingMetrics(date, skillId, callback);
	    			
	    			dailyStaffingMetricsSubscription = messagebroker.subscribe({
	    				domainType: 'IScheduledResourcesReadModel',
	    				callback: function (notification) {
	    					if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
	    						loadDailyStaffingMetrics(date, skillId, callback);
	    					}
	    				}
	    			});
	    		});
	    	},

	    	subscribeTeamSchedule: function (groupId, date, callback, isApplicableNotification) {
		        unsubscribeTeamSchedule();
	            incomingTeamSchedule = callback;
	            startPromise.done(function() {
	            	teamScheduleHub.server.subscribeTeamSchedule(groupId, date);

	            	teamScheduleSubscription = messagebroker.subscribe({
		            	domainReferenceType: 'Person',
		            	domainType: 'IPersonScheduleDayReadModel',
		            	callback: function (notification) {
		            		if (!isApplicableNotification(notification)) {
		            			return;
		            		}
		            		if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
		            			teamScheduleHub.server.subscribeTeamSchedule(groupId, date);
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
	                    	if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
	                            personScheduleHub.server.personSchedule(personId, date);
                            }
	                    }
	                });
	                
	            });
	        },
	        
	        unsubscribePersonSchedule: unsubscribePersonSchedule,
	        unsubscribeTeamSchedule: unsubscribeTeamSchedule,
	        unsubscribeDailyStaffingMetrics: unsubscribeDailyStaffingMetrics
	    };

	});
