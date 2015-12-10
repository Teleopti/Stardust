define([
        'knockout',
        'moment',
        'navigation',
        'ajax',
        'resources',
		'guidgenerator',
		'notifications'
    ], function(
        ko,
        moment,
        navigation,
        ajax,
        resources,
		guidgenerator,
		notificationsViewModel
    ) {

        return function() {

            var self = this;

            this.AbsenceType = ko.observable("");
            this.TimeZoneName = ko.observable();

            this.StartDate = ko.observable(moment());
            this.EndDate = ko.observable(moment());

            var personId;
            var personName;
            var groupId;
            var businessUnitId;
	        
            this.SetData = function (data) {
            	groupId = data.GroupId;
            	personId = data.PersonId;
            	businessUnitId = data.BusinessUnitId;
            	personName = data.PersonName;
            	self.StartDate(moment(data.Date.format('YYYY-MM-DD'),'YYYY-MM-DD'));
                self.EndDate(self.StartDate().clone());
                self.AbsenceTypes(data.Absences);
                self.TimeZoneName(data.TimeZoneName);
            };

            this.StartDateFormatted = ko.computed(function() {
                return self.StartDate().format(resources.DateFormatForMoment);
            });

            this.EndDateFormatted = ko.computed(function () {
                return self.EndDate().format(resources.DateFormatForMoment);
            });

            this.AbsenceTypes = ko.observableArray();

            this.InvalidEndDate = ko.computed(function() {
                if (!self.EndDate())
                    return true;
                if (self.StartDate() && self.StartDate().diff)
                    return self.StartDate().diff(self.EndDate()) > 0;
                return false;
            });

            this.Apply = function () {
            	var trackId = guidgenerator.newGuid();
                var data = JSON.stringify({
                	StartDate: self.StartDate().format('YYYY-MM-DD'),
                	EndDate: self.EndDate().format('YYYY-MM-DD'),
                    AbsenceId: self.AbsenceType(),
                    PersonId: personId,
                    TrackedCommandInfo: { TrackId: trackId }
                });
                ajax.ajax({
		                url: 'api/PersonScheduleCommand/AddFullDayAbsence',
		                type: 'POST',
		                headers: { 'X-Business-Unit-Filter': businessUnitId },
		                data: data,
		                success: function(data, textStatus, jqXHR) {
			                navigation.GoToTeamSchedule(businessUnitId, groupId, self.StartDate());
		                },
		                statusCode500: function (jqXHR, textStatus, errorThrown) {
			                notificationsViewModel.UpdateNotification(trackId, 3);
		                }
	                }
                );
                notificationsViewModel.AddNotification(trackId, resources.AddingFulldayAbsenceFor + " " + personName + "... ");
            };

        };
    });