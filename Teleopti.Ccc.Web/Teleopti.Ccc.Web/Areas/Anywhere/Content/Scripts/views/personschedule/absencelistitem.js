define([
		'knockout',
		'moment',
		'navigation',
		'resources',
		'ajax',
		'guidgenerator',
		'shared/timezone-current',
		'notifications'
	], function(
		ko,
		moment,
		navigation,
		resources,
		ajax,
		guidgenerator,
		timezoneCurrent,
		notificationsViewModel
	) {

		return function(data) {

			var self = this;

			var personName = data.PersonName;

			this.StartTime = ko.observable(moment(data.StartTime).format(resources.DateTimeFormatForMoment));
			this.EndTime = ko.observable(moment(data.EndTime).format(resources.DateTimeFormatForMoment));
			this.Name = ko.observable(data.Name);
			this.BackgroundColor = ko.observable(data.Color);
			
			this.AboutToRemove = ko.observable(false);
			this.Removing = ko.observable(false);

			this.ScheduleDate = ko.observable(moment(data.StartTime).startOf('day'));
			this.ianaTimeZoneOther = ko.observable(data.IanaTimeZoneOther);

			this.IsOtherTimezone = ko.computed(function () {
				return timezoneCurrent.IanaTimeZone() !== self.ianaTimeZoneOther();
			});

			var getTimeZoneNameShort = function (timeZoneName) {
				if (self.IsOtherTimezone() && timeZoneName) {
					var end = timeZoneName.indexOf(')');
					return (end < 0) ? timeZoneName : timeZoneName.substring(1, end);
				}
				return undefined;
			};

			this.TimeZoneNameShort = ko.observable(getTimeZoneNameShort(data.TimeZoneName));
			
			this.StartTimeOtherTimeZone = ko.computed(function () {
				if (self.ianaTimeZoneOther()) {
					var userTime = moment.tz(data.StartTime, timezoneCurrent.IanaTimeZone());
					var otherTime = userTime.clone().tz(self.ianaTimeZoneOther());
					return otherTime.format(resources.DateTimeFormatForMoment);
				}
				return undefined;
			});

			this.EndTimeOtherTimeZone = ko.computed(function () {
				if (self.ianaTimeZoneOther()) {
					var userTime = moment.tz(data.EndTime, timezoneCurrent.IanaTimeZone());
					var otherTime = userTime.clone().tz(self.ianaTimeZoneOther());
					return otherTime.format(resources.DateTimeFormatForMoment);
				}
				return undefined;
			});

			this.Remove = function() {
				self.AboutToRemove(true);
			};

			this.ConfirmRemoval = function () {
				var trackId = guidgenerator.newGuid();
				self.Removing(true);
				ajax.ajax(
					{
						url: 'PersonScheduleCommand/RemovePersonAbsence',
						type: 'POST',
						headers: { 'X-Business-Unit-Filter': data.BusinessUnitId },
						data: JSON.stringify({
							PersonAbsenceId: data.Id,
							TrackedCommandInfo: { TrackId: trackId }
						}),
						success: function (responseData, textStatus, jqXHR) {
							navigation.GoToTeamSchedule(data.BusinessUnitId, data.GroupId, data.Date);
						},
						statusCode500: function (jqXHR, textStatus, errorThrown) {
							notificationsViewModel.UpdateNotification(trackId, 3);
						}
					}
				);
				notificationsViewModel.AddNotification(trackId, resources.RemovingAbsenceFor + " " + personName + "... ");
			};
		};
	});
