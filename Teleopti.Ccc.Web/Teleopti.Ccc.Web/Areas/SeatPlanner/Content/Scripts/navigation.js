
define([
        'moment',
        'window'
], function (
        moment,
		window
    ) {

	var toDateString = function (date) {
		if (moment.isMoment(date))
			date = date.format("YYYYMMDD");
		return date;
	};

	return {
		GoToDefaultView: function () {
			window.setLocationHash('SeatPlannerPrototype/');
		},

		UrlForHome: function () {
			//if (teamScheduleAvailable)
			//	return window.baseLocation() + "#teamschedule/" + buid;
			//else if (realTimeAdherenceAvailable)
			//	return window.baseLocation() + "#realtimeadherencesites/" + buid;
			return window.baseLocation() + "#SeatPlannerPrototype/";
		},
		UrlForSeatPlanner: function () {

			return window.baseLocation() + "#SeatPlannerPrototype/";
			
		},
		UrlForSeatMap: function() {
			return window.baseLocation() + "#SeatMap/";
		}


		/*GotoRealTimeAdherenceTeams: function (buid, siteId) {
			window.setLocationHash('realtimeadherenceteams/' + buid + '/' + siteId);
		},

		GotoRealTimeAdherenceTeamDetails: function (buid, teamId) {
			window.setLocationHash('realtimeadherenceagents/' + buid + '/' + teamId);
		},
		GotoRealTimeAdherenceMultipleTeamDetails: function (buid) {
			window.setLocationHash('realtimeadherenceagents/' + buid + '/' + 'MultipleTeams');
		},
		GotoRealTimeAdherenceMultipleSiteDetails: function(buid) {
			window.setLocationHash('realtimeadherenceagents/' + buid + '/' + 'MultipleSites');
		},*/

		/*UrlForHome: function (buid, realTimeAdherenceAvailable, teamScheduleAvailable) {
			if(teamScheduleAvailable)
				return window.baseLocation() + "#teamschedule/" + buid;
			else if(realTimeAdherenceAvailable)
				return window.baseLocation() + "#realtimeadherencesites/" + buid;
			return window.baseLocation() + "#teamschedule/" + buid;
		},*/
		/*UrlForTeamScheduleToday: function(buid) {
			return window.baseLocation() + "#teamschedule/" + buid;
		},
		UrlForChangingSchedule : function(buid,teamId,personId,date) {
			return window.baseLocation() + "#teamschedule/" + buid + "/" + teamId + "/" + personId + "/" + toDateString(date);
		},
		UrlForAdherenceDetails : function(buid,personId) {
			return window.baseLocation() + "#manageadherence/" + buid + "/" + personId;
		},
		UrlForRealTimeAdherence: function (buid) {
			return window.baseLocation() + "#realtimeadherencesites/" + buid ;
		},
		UrlForTeamScheduleForDate: function (buid, date) {
			return window.baseLocation() + '#teamschedule/' + buid + '/' + toDateString(date);
		},
		UrlForPersonScheduleAddFullDayAbsence:function(buid,teamId,personId,date) {
			return  window.baseLocation() + '#personschedule/' + buid + '/' + teamId + '/' + personId + '/' + toDateString(date) + '/addfulldayabsence';
		},
		UrlForPersonScheduleAddIntradayAbsence:function(buid,teamId,personId,date) {
			return  window.baseLocation() + '#personschedule/' + buid + '/' + teamId + '/' + personId + '/' + toDateString(date) + '/addintradayabsence';
		},
		UrlForPersonScheduleAddActivity: function (buid, groupid, personid, date) {
			return window.baseLocation() + '#personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date) + '/addactivity';
		},
		UrlForPersonSchedule: function (buid, groupid, personid, date) {
			return window.baseLocation() + '#personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date);
		},
		UrlForPersonScheduleMoveActivity: function (buid, groupid, personid, date, startTime) {
			return window.baseLocation() + '#personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date) + '/moveactivity/' + startTime;
		}*/
	};
});
