define([
		'knockout',
		'moment',
		'navigation',
		'resources',
		'ajax',
		'guidgenerator',
		'notifications'
	], function(
		ko,
		moment,
		navigation,
		resources,
		ajax,
		guidgenerator,
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
			this.ianaTimeZone = ko.observable(data.IanaTimeZoneLoggedOnUser);
			this.ianaTimeZoneOther = ko.observable(data.IanaTimeZoneOther);

			var getTimeZoneNameShort = function (timeZoneName) {
				if (!timeZoneName) return undefined;
				var end = timeZoneName.indexOf(')');
				return timeZoneName.substring(1, end);
			};

			this.TimeZoneNameShort = ko.observable(getTimeZoneNameShort(data.TimeZoneName));
			
			this.IsOtherTimezone = ko.computed(function () {
				if (self.StartTime() && self.ianaTimeZone() && self.ianaTimeZoneOther()) {
					var userTime = moment.tz(self.ianaTimeZone());
					var otherTime = userTime.clone().tz(self.ianaTimeZoneOther());
					return otherTime.format('HH:mm') != userTime.format('HH:mm');
				}
				return false;
			});

			this.StartTimeOtherTimeZone = ko.computed(function () {
				if (self.ianaTimeZone() && self.ianaTimeZoneOther()) {
					var userTime = moment.tz(data.StartTime, self.ianaTimeZone());
					var otherTime = userTime.clone().tz(self.ianaTimeZoneOther());
					return otherTime.format(resources.DateTimeFormatForMoment);
				}
				return undefined;
			});

			this.EndTimeOtherTimeZone = ko.computed(function () {
				if (self.ianaTimeZone() && self.ianaTimeZoneOther()) {
					var userTime = moment.tz(data.EndTime, self.ianaTimeZone());
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
						data: JSON.stringify({
							PersonAbsenceId: data.Id,
							TrackedCommandInfo: { TrackId: trackId }
						}),
						success: function (responseData, textStatus, jqXHR) {
							navigation.GoToTeamSchedule(data.GroupId, data.Date);
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
