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

			this.StartDateTimeMoment = moment(data.StartTime);
			this.EndDateTimeMoment = moment(data.EndTime);

			this.StartDateTime = self.StartDateTimeMoment.format(resources.DateTimeFormatForMoment);
			this.EndDateTime = self.EndDateTimeMoment.format(resources.DateTimeFormatForMoment);
			this.EndDate = ko.observable(self.EndDateTimeMoment.format('YYYY-MM-DD'));
			this.EndTime = ko.observable(self.EndDateTimeMoment.format(resources.TimeFormatForMoment));

			this.UpdatedEndDateTime = ko.computed(function () {
				return moment(self.EndDate() +' '+ self.EndTime(), 'YYYY-MM-DD' +' '+ resources.TimeFormatForMoment);
			});

			
			this.Name = ko.observable(data.Name);
			this.BackgroundColor = ko.observable(data.Color);
			
			this.AboutToRemove = ko.observable(false);
			this.Removing = ko.observable(false);

			this.IsBackToWorkFormVisible = ko.observable(false);
			this.IsBackToWorkFormFeatureEnabled = ko.observable(resources.MyTeam_AbsenceBackToWork_31478);

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

			this.convertTimeToOtherTimeZone = function (datetime) {
				if (self.ianaTimeZoneOther()) {
					var userTime = moment.tz(datetime, timezoneCurrent.IanaTimeZone());
					var otherTime = userTime.clone().tz(self.ianaTimeZoneOther());
					return otherTime.format(resources.DateTimeFormatForMoment);
				}
				return undefined;
			}
			
			this.StartTimeOtherTimeZone = ko.computed(function () {
				return self.convertTimeToOtherTimeZone(data.StartTime);
			});

			this.EndTimeOtherTimeZone = ko.computed(function () {
				return self.convertTimeToOtherTimeZone(data.EndTime);
			});
			
			this.ModifiedEndTimeOtherTimeZone = ko.computed(function () {
				return self.convertTimeToOtherTimeZone(self.UpdatedEndDateTime().format("YYYY-MM-DD HH:mm"));
			});

			this.ValidateEndTimeIsAfterStartTime = function () {
				return !(self.UpdatedEndDateTime().isBefore(self.StartDateTimeMoment));
			}
			this.ValidateEndTimeIsBeforeOriginalEndTime = function() {
				return !(self.UpdatedEndDateTime().isAfter(self.EndDateTimeMoment));
			}

			this.ErrorMessage = ko.computed(function () {
				if (!self.ValidateEndTimeIsAfterStartTime())
					return resources.DateFromGreaterThanDateTo;
				if (!self.ValidateEndTimeIsBeforeOriginalEndTime())
					return resources.BackToWorkCannotBeGreaterThanAbsenceEnd;
				return undefined;
			});

			this.IsAbsenceValid = ko.computed(function () {
				return !(self.ErrorMessage());
			});

			this.BackToWorkTextPrompt = ko.observable(data.PersonName+" "+resources.BackToWorkTextPrompt);

			this.Remove = function() {
				self.AboutToRemove(true);
			};

			this.Save = function () {

				if (!self.IsAbsenceValid()) {
					return;
				}

				var trackId = guidgenerator.newGuid();
				var requestData = JSON.stringify({
					StartTime: self.StartDateTime,
					EndTime: self.UpdatedEndDateTime(),
					PersonAbsenceId: data.Id,
					PersonId: data.PersonId,
					TrackedCommandInfo: { TrackId: trackId }
				});
				ajax.ajax({
					url: 'PersonScheduleCommand/ModifyPersonAbsence',
					type: 'POST',
					headers: { 'X-Business-Unit-Filter': data.BusinessUnitId },
					data: requestData,
					success: function (responseData, textStatus, jqXHR) {
						navigation.GoToTeamSchedule(data.BusinessUnitId, data.GroupId, data.Date);
					},
					statusCode500: function (jqXHR, textStatus, errorThrown) {
						notificationsViewModel.UpdateNotification(trackId, 3);
					}
				}
				);
				notificationsViewModel.AddNotification(trackId, resources.UpdatingAbsence + " " + personName + "... ");
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

			this.ShowBackToWorkSection = function() {
				this.IsBackToWorkFormVisible(!this.IsBackToWorkFormVisible());
			}

			this.Cancel = function () {
				navigation.GoToTeamSchedule(data.BusinessUnitId, data.GroupId, data.Date);
			};
		};
	});
