/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />

Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel = function (ajax, dayViewModels, date) {
	var self = this;

	this.LoadFeedback = function () {
		ajax.Ajax({
			url: "PreferenceFeedback/PeriodFeedback",
			dataType: "json",
			data: { Date: date },
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				self.TargetDaysOffLower(data.TargetDaysOff.Lower);
				self.TargetDaysOffUpper(data.TargetDaysOff.Upper);
				self.PossibleResultDaysOff(data.PossibleResultDaysOff);
				self.TargetContractTimeLower(data.TargetContractTime.Lower);
				self.TargetContractTimeUpper(data.TargetContractTime.Upper);
				self.MaxMustHave(data.MaxMustHave);
			}
		});
	};

	this.TargetDaysOffLower = ko.observable();
	this.TargetDaysOffUpper = ko.observable();
	this.PossibleResultDaysOff = ko.observable();

	this.TargetContractTimeLower = ko.observable();
	this.TargetContractTimeUpper = ko.observable();

	this.PossibleResultContractTimeMinutesLower = ko.computed(function () {
		var sum = 0;
		$.each(dayViewModels, function (index, day) {
			var value = day.PossibleContractTimeMinutesLower();
			if (value)
				sum += parseInt(value);
			sum += day.ContractTimeMinutes;
		});
		return sum;
	});

	this.PossibleResultContractTimeMinutesUpper = ko.computed(function () {
		var sum = 0;
		$.each(dayViewModels, function (index, day) {
			var value = day.PossibleContractTimeMinutesUpper();
			if (value)
				sum += parseInt(value);
			sum += day.ContractTimeMinutes;
		});
		return sum;
	});

	this.PossibleResultContractTimeLower = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultContractTimeMinutesLower());
	});

	this.PossibleResultContractTimeUpper = ko.computed(function () {
		return Teleopti.MyTimeWeb.Preference.formatTimeSpan(self.PossibleResultContractTimeMinutesUpper());
	});



	this.MaxMustHave = ko.observable(0);

	this.CurrentMustHave = ko.computed(function () {
		var total = 0;
		$.each(dayViewModels, function (index, day) {
			var value = day.MustHave();
			if (value)
				total += 1;
		});

		return total;
	});

	this.MustHaveText = ko.computed(function () {
		$('#Preference-must-have-numbers').text(self.CurrentMustHave() + "(" + self.MaxMustHave() + ")");
		if (self.CurrentMustHave() >= self.MaxMustHave()) {
			$('#Preference-must-have-button').addClass("grey-out");
		} else {
			$('#Preference-must-have-button').removeClass("grey-out");
		}
		return self.CurrentMustHave() + "(" + self.MaxMustHave() + ")";
	});
};
