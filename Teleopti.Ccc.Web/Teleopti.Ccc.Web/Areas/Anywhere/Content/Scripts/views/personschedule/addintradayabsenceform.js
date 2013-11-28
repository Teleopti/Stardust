define([
	'knockout',
	'moment',
	'navigation',
	'ajax',
	'resources!r',
	'timepicker'
], function (
	ko,
	moment,
	navigation,
	ajax,
	resources,
	timepicker
    ) {

	return function () {

		var self = this;
		
		this.Absence = ko.observable("");

		this.Date = ko.observable();
		this.StartTime = ko.observable();
		this.EndTime = ko.observable();
		this.AbsenceTypes = ko.observableArray();

		var groupid;
		var personId;

		this.SetData = function (data, groupId) {
			personId = data.PersonId;
			self.Date(data.Date);
			self.StartTime(data.DefaultIntradayAbsenceData.StartTime);
			self.EndTime(data.DefaultIntradayAbsenceData.EndTime);
			self.AbsenceTypes(data.Absences);
			groupid = groupId;
		};
		
		this.InvalidIntradayAbsenceTimes = ko.computed(function () {
			
			//if (!self.EndDate())
			//	return true;
			//if (self.StartDate() && self.StartDate().diff)
			//	return self.StartDate().diff(self.EndDate()) > 0;
			return false;
		});
		
		this.InvalidEndTime = ko.computed(function () {
			if (!self.EndTime())
				return true;
			if (self.StartTime() && self.StartTime().diff)
				return self.StartTime().diff(self.EndTime()) > 0;
			return false;
		});

		this.Apply = function() {
			var data = JSON.stringify({
				Date: self.Date().format('YYYY-MM-DD'),
				StartTime: self.StartTime(),
				EndTime: self.EndTime(),
				AbsenceId: self.Absence(),
				PersonId: personId
			});
			ajax.ajax({
					url: 'PersonScheduleCommand/AddIntradayAbsence',
					type: 'POST',
					data: data,
					success: function(data, textStatus, jqXHR) {
						navigation.GoToTeamSchedule(groupid, self.Date());
					},
					error: function(jqXHR, textStatus, errorThrown) {
						if (jqXHR.status == 400) {
							//var data = $.parseJSON(jqXHR.responseText);
							//self.ErrorMessage(data.Errors.join('</br>'));
							return;
						}
					}
				}
			);
		};

	};
});