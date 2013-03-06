/// <reference path="~/Content/Scripts/jquery-1.8.3-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.3.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.9.1.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />


Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel = function (ajax) {
	var self = this;

	this.AvailableTemplates = ko.observableArray();
	this.SelectedTemplate = ko.observable();

	this.PreferenceId = ko.observable();
	this.EarliestStartTime = ko.observable();
	this.LatestStartTime = ko.observable();
	this.EarliestEndTime = ko.observable();
	this.EarliestEndTimeNextDay = ko.observable();
	this.LatestEndTime = ko.observable();
	this.LatestEndTimeNextDay = ko.observable();
	this.MinimumWorkTime = ko.observable();
	this.MaximumWorkTime = ko.observable();
	this.ActivityEarliestStartTime = ko.observable();
	this.ActivityLatestStartTime = ko.observable();
	this.ActivityEarliestEndTime = ko.observable();
	this.ActivityLatestEndTime = ko.observable();
	this.ActivityPreferenceId = ko.observable();
	this.ActivityMinimumTime = ko.observable();
	this.ActivityMaximumTime = ko.observable();

	this.IsSaveAsNewTemplate = ko.observable();
	this.NewTemplateName = ko.observable('');

	this.IsTimeInputEnabled = ko.observable();

	this.EnableActivityTimeEditing = ko.computed(function () {
		var result = self.ActivityPreferenceId();
		return result != undefined && result != '';
	});

	this.SelectedTemplate.subscribe(function (newValue) {
		if (newValue) {
			if (!newValue.PreferenceId)
				self.PreferenceId('');
			else
				self.PreferenceId(newValue.PreferenceId);
			self.EarliestStartTime(newValue.EarliestStartTime);
			self.LatestStartTime(newValue.LatestStartTime);
			self.EarliestEndTime(newValue.EarliestEndTime);
			self.LatestEndTime(newValue.LatestEndTime);
			self.EarliestEndTimeNextDay(newValue.EarliestEndTimeNextDay);
			self.LatestEndTimeNextDay(newValue.LatestEndTimeNextDay);
			self.MinimumWorkTime(newValue.MinimumWorkTime);
			self.MaximumWorkTime(newValue.MaximumWorkTime);
			if (!newValue.ActivityPreferenceId)
				self.ActivityPreferenceId('');
			else
				self.ActivityPreferenceId(newValue.ActivityPreferenceId);
			self.ActivityEarliestStartTime(newValue.ActivityEarliestStartTime);
			self.ActivityLatestStartTime(newValue.ActivityLatestStartTime);
			self.ActivityEarliestEndTime(newValue.ActivityEarliestEndTime);
			self.ActivityLatestEndTime(newValue.ActivityLatestEndTime);
			self.ActivityMinimumTime(newValue.ActivityMinimumTime);
			self.ActivityMaximumTime(newValue.ActivityMaximumTime);
		}
	});

	this.IsTimeInputEnabled.subscribe(function (newValue) {
		if (!newValue) {
			self.ActivityPreferenceId('');
			self.EarliestStartTime(undefined);
			self.LatestStartTime(undefined);
			self.EarliestEndTime(undefined);
			self.LatestEndTime(undefined);
			self.EarliestEndTimeNextDay(false);
			self.LatestEndTimeNextDay(false);
			self.MinimumWorkTime(undefined);
			self.MaximumWorkTime(undefined);
		}
	});

	this.EnableActivityTimeEditing.subscribe(function (newValue) {
		if (!newValue) {
			self.ActivityEarliestStartTime(undefined);
			self.ActivityLatestStartTime(undefined);
			self.ActivityEarliestEndTime(undefined);
			self.ActivityLatestEndTime(undefined);
			self.ActivityMinimumTime(undefined);
			self.ActivityMaximumTime(undefined);
		}
	});

	this.ValidationError = ko.observable();
};