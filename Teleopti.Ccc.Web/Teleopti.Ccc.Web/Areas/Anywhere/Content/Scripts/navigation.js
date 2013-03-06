
define([
	], function (
	) {
		return {
			GotoPersonSchedule: function (id, date) {
				if (date)
					window.location.hash = 'personschedule/' + id + '/' + date;
				else
					window.location.hash = 'personschedule/' + id;
			}
		};
	});
