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
			self.personId = data.PersonId;
			self.CurrentDateMoment = ko.observable(moment(data.Date));
			self.CurrentDate = ko.observable(data.Date.format('YYYY-MM-DD'));
			self.EndTimeForAbsenceModify = ko.observable(moment(data.Date));
			self.Schedules = ko.observableArray();
			self.StartDateOnly = moment(data.StartTime, "YYYY-MM-DD");
			self.EndDateOnly = moment(data.EndTime, "YYYY-MM-DD");

			this.StartDateTimeMoment = moment(data.StartTime);
			this.EndDateTimeMoment = moment(data.EndTime);

			this.StartDateTime = self.StartDateTimeMoment.format(resources.DateTimeFormatForMoment);
			this.EndDateTime = self.EndDateTimeMoment.format(resources.DateTimeFormatForMoment);
			this.EndDate = ko.observable(self.EndDateTimeMoment.format('YYYY-MM-DD'));
			this.EndTime = ko.observable(self.EndDateTimeMoment.format(resources.TimeFormatForMoment));

			this.PreviousDate = ko.computed(function () {
				self.CurrentDateMoment(moment(self.CurrentDate()));
				self.EndTimeForAbsenceModify(moment(self.CurrentDate()));
				return moment(self.CurrentDate()).add('day', -1);
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
				return self.convertTimeToOtherTimeZone(self.EndTimeForAbsenceModify().format("YYYY-MM-DD HH:mm"));
			});

			this.ValidateEndTimeIsAfterStartTime = function () {
				return !(moment(self.CurrentDateMoment(), "YYYY-MM-DD").isBefore(self.StartDateOnly));
			}
			this.ValidateEndTimeIsBeforeOriginalEndTime = function() {
				return !(moment(self.CurrentDateMoment(), "YYYY-MM-DD").isAfter(self.EndDateOnly));
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

			this.LoadAbsenceTimeForEmptyDay = function() {

			};

			this.Save = function () {
				var trackId = guidgenerator.newGuid();
				ajax.ajax({
					url: 'PersonScheduleCommand/GetPersonSchedule',
					dataType: "json",
					type: 'GET',
					data: { personId: self.personId, date: self.PreviousDate().format("YYYY-MM-DD") },
					success: function (schedule) {
						var endTime = self.CurrentDateMoment();
						if (schedule.EndTime != null) {
							endTime = moment(schedule.EndTime);
						}
						else if (schedule.IsDayOff) {
							endTime = self.CurrentDateMoment();
						}

						if (self.CurrentDate() == self.StartDateOnly.format("YYYY-MM-DD")) {
							self.ConfirmRemoval();
						} else {
							self.EndTimeForAbsenceModify(endTime);
							self.UpdateAbsence();
						}
					},
					statusCode500: function (jqXHR, textStatus, errorThrown) {
						notificationsViewModel.UpdateNotification(trackId, 3);
					}
				});
			};

			this.UpdateAbsence = function () {

				if (!self.IsAbsenceValid()) {
					return;
				}

				var trackId = guidgenerator.newGuid();
				var requestData = JSON.stringify({
					StartTime: self.StartDateTime,
					EndTime: self.EndTimeForAbsenceModify().format('YYYY-MM-DD' + ' ' + resources.TimeFormatForMoment),
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
