/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

AddExtendedPreferenceFormViewModel = function () {
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

	this.EnableActivityTimeEditing = ko.computed(function () {
		var result = this.ActivityPreferenceId();
		return result != undefined && result != '';
	}, this);
};
