
define([
		'helpers'
	], function (
		helpers
	) {
		return {
			GotoAgentSchedule: function (id, date) {
				if (date)
					window.location.hash = 'agentschedule/' + id + '/' + helpers.DateToUrl(date);
				else
					window.location.hash = 'agentschedule/' + id;
			},
			GotoResources: function (date) {
				if (date) {
					window.location.hash = 'resources/' + helpers.DateToUrl(date);
				} else {
					window.location.hash = 'resources';
				}
			},
			GotoAgentScheduleEditLayer: function (id, date, layerId) {
				if (date)
					window.location.hash = 'agentschedule/' + id + '/' + helpers.DateToUrl(date) + '/editlayer/' + layerId;
				else
					window.location.hash = 'agentschedule/' + id + '/editlayer/' + layerId;
			}
		};
	});
