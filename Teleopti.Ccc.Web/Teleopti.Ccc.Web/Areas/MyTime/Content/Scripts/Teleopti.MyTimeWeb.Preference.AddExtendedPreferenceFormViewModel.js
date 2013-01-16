/// <reference path="~/Content/Scripts/jquery-1.8.3-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.3.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.9.1.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel = function () {
	var self = this;

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

	this.IsTimeInputEnabled = ko.observable();

	this.EnableActivityTimeEditing = ko.computed(function () {
		var result = self.ActivityPreferenceId();
		return result != undefined && result != '';
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

	this.reset = function () {
		self.PreferenceId('');
		self.EarliestStartTime(undefined);
		self.LatestStartTime(undefined);
		self.EarliestEndTime(undefined);
		self.EarliestEndTimeNextDay(undefined);
		self.LatestEndTime(undefined);
		self.LatestEndTimeNextDay(undefined);
		self.MinimumWorkTime(undefined);
		self.MaximumWorkTime(undefined);
		self.ActivityPreferenceId('');
		self.ValidationError(undefined);
	};
};

