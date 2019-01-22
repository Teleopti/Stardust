Teleopti.MyTimeWeb.Preference.PreferencesAndSchedulesViewModel = function (ajax, dayViewModels) {
	var self = this;

	this.ShowCalendar = Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_Preference_Indicate_BankHoliday_79900');

	this.DayViewModels = dayViewModels;

	this.LoadPreferencesAndSchedules = function (from, to) {
		var deferred = $.Deferred();
		if (!from || !to) {
			deferred.reject();
		} else {
			ajax.Ajax({
				url: 'Preference/PreferencesAndSchedules',
				dataType: 'json',
				data: {
					From: from,
					To: to
				},
				type: 'GET',
				success: function (data, textStatus, jqXHR) {
					data = data || [];
					$.each(data, function (index, element) {
						var dayViewModel = self.DayViewModels[element.Date];
						if (element.Preference) dayViewModel.ReadPreference(element.Preference);
						if (element.DayOff) dayViewModel.ReadDayOff(element.DayOff);
						if (element.Absence) dayViewModel.ReadAbsence(element.Absence);
						if (element.PersonAssignment) dayViewModel.ReadPersonAssignment(element.PersonAssignment);
						dayViewModel.Feedback(element.Feedback);
						if (element.StyleClassName) dayViewModel.StyleClassName(element.StyleClassName);
						if (element.Meetings) {
							dayViewModel.Meetings(element.Meetings);
						}
						if (element.PersonalShifts) {
							dayViewModel.PersonalShifts(element.PersonalShifts);
						}
						if (self.ShowCalendar && element.BankHolidayCalendar) dayViewModel.ReadBankHolidayCalendar(element.BankHolidayCalendar);

					});
					deferred.resolve();
				}
			});
		}
		return deferred.promise();
	};
};
