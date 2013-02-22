
define([
	], function (
	) {
		return {
			GotoAgentSchedule: function (id, date) {
				if (date)
					window.location.hash = 'agentschedule/' + id + '/' + date;
				else
					window.location.hash = 'agentschedule/' + id;
			}
		};
	});
