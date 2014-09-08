define([
	'moment',
	'momentTimezoneData',
	'resources'
], function(
	moment,
	momentTimezoneData,
	resources
) {
	var fromTimeInput = function(time, timeZone, optionalDate) {
		var momentInput = moment(time, resources.TimeFormatForMoment);
		if (!optionalDate() || !timeZone)
			return moment().add('h', momentInput.hours()).add('m', momentInput.minutes());
		return optionalDate().clone().add('h', momentInput.hours()).add('m', momentInput.minutes());
	};
	return {
		self: this,
		FromTimeInput: fromTimeInput,
		FromDate: function(date, timeZone) {
			return momentTimezoneData.tz(date.format('YYYY-MM-DD'), timeZone);
		},
		IsOtherTimeZone: function(timeZone1, timeZone2, startTime, optionalDate) {
			if (startTime && timeZone1 && timeZone2) {
				var userTime = fromTimeInput(startTime, timeZone1, function() {
					return optionalDate;
				});
				var otherTime = userTime.clone().tz(timeZone2);
				return otherTime.format('HH:mm') != userTime.format('HH:mm');
			}
			return false;
		}
	};
});
