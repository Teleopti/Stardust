define(
	[
	], function () {

		return {
			Minutes: {
				FromHours: function (hours) {
					return hours * 60;
				},

				StartOfHour: function (minutes) {
					return minutes - (minutes % 60);
				},

				EndOfHour: function (minutes) {
					var reminder = (minutes % 60);
					if (reminder == 0)
						return minutes;
					return minutes + (60 - reminder);
				},

				AddHours: function (minutes, hours) {
					return minutes + 60 * hours;
				}
			},

			Async: {
				RunAndSetInterval: function (code, interval) {
					code();
					return setInterval(code, interval);
				}
			},

			Guid: {
				Create: function () {
					return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace( /[xy]/g , function(c) {
						var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
						return v.toString(16);
					});
				}
			}
		};

	});
