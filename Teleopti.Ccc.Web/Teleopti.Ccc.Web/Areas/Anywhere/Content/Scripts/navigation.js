
define([
        'moment',
        'window'
], function (
        moment,
		windowWrap
    ) {

	var toDateString = function (date) {
		if (moment.isMoment(date))
			date = date.format("YYYYMMDD");
		return date;
	};

    var baseLocation = function() {
        if (!window.location.origin) {
            window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
        }
        return window.location.origin + window.location.pathname;
    };

	return {
		GoToTeamSchedule: function (buid, id, date) {
			windowWrap.setLocationHash('teamschedule/' + buid + '/' + id + '/' + toDateString(date));
		},
		GoToTeamScheduleWithPreselectedParameter: function (buid, id, date, selectedPersonId, selectedStartMinutes) {
			windowWrap.setLocationHash('teamschedule/' + buid + '/' + id + '/' + toDateString(date) 
				+ '/#/' + selectedPersonId + (!isNaN(selectedStartMinutes) ? '/' + selectedStartMinutes : '/'));
		},
		GotoRealTimeAdherenceTeams: function (buid, siteId) {
			windowWrap.setLocationHash('realtimeadherenceteams/' + buid + '/' + siteId);
		},

		GotoRealTimeAdherenceTeamDetails: function (buid, teamId) {
			windowWrap.setLocationHash('realtimeadherenceagents/' + buid + '/' + teamId);
		},
		GotoRealTimeAdherenceMultipleTeamDetails: function (buid) {
			windowWrap.setLocationHash('realtimeadherenceagents/' + buid + '/' + 'MultipleTeams');
		},
		GotoRealTimeAdherenceMultipleSiteDetails: function(buid) {
			windowWrap.setLocationHash('realtimeadherenceagents/' + buid + '/' + 'MultipleSites');
		},

		
		UrlForHome: function (buid, realTimeAdherenceAvailable, teamScheduleAvailable) {
			if(teamScheduleAvailable)
				return baseLocation() + "#teamschedule/" + buid;
			else if(realTimeAdherenceAvailable)
				return baseLocation() + "#realtimeadherencesites/" + buid;
			return baseLocation() + "#teamschedule/" + buid;
		},
		UrlForTeamScheduleToday: function(buid) {
			return baseLocation() + "#teamschedule/" + buid;
		},
		UrlForChangingSchedule : function(buid,teamId,personId) {
		    return baseLocation() + "#teamschedule/" + buid + "/" + teamId + "/" + personId;
		},
		UrlForRealTimeAdherence: function (buid) {
			return baseLocation() + "#realtimeadherencesites/" + buid ;
		},
		UrlForTeamScheduleForDate: function (buid, date) {
			return baseLocation() + '#teamschedule/' + buid + '/' + toDateString(date);
		},
		UrlForPersonScheduleAddFullDayAbsence:function(buid,teamId,personId,date) {
			return baseLocation() + '#personschedule/' + buid + '/' + teamId + '/' + personId + '/' + toDateString(date) + '/addfulldayabsence';
		},
		UrlForPersonScheduleAddIntradayAbsence:function(buid,teamId,personId,date) {
			return baseLocation() + '#personschedule/' + buid + '/' + teamId + '/' + personId + '/' + toDateString(date) + '/addintradayabsence';
		},
		UrlForPersonScheduleAddActivity: function (buid, groupid, personid, date) {
			return baseLocation() + '#personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date) + '/addactivity';
		},
		UrlForPersonSchedule: function (buid, groupid, personid, date) {
			return baseLocation() + '#personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date);
		},
		UrlForPersonScheduleMoveActivity: function (buid, groupid, personid, date, startTime) {
			return baseLocation() + '#personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date) + '/moveactivity/' + startTime;
		}
	};
});
