
define([
	], function (
	) {

		var dateToUrl = function(date) {
			return date.replace( /-/g , '');
		};
		
		return {
			GotoAgentSchedule: function (id, date) {
				if (date)
					window.location.hash = 'agentschedule/' + id + '/' + dateToUrl(date);
				else
					window.location.hash = 'agentschedule/' + id;
			},
			GotoResources: function (date) {
				if (date) {
					window.location.hash = 'resources/' + date.replace(/-/g, '');
				} else {
					window.location.hash = 'resources';
				}
			},
			GotoAgentScheduleEditLayer: function (id, date, layerId) {
				if (date)
					window.location.hash = 'agentschedule/' + id + '/' + dateToUrl(date) + '/editlayer/' + layerId;
				else
					window.location.hash = 'agentschedule/' + id + '/editlayer/' + layerId;
			}
		};
	});
