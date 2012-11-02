/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />

var SignInViewModel = function (availableDataSources) {
	this.self = this;

	this.AvailableDataSources = ko.observableArray(availableDataSources);
	this.AvailableBusinessUnits = ko.observableArray();
	this.DataSourceSelectionActive = ko.observable(true);
	this.BusinessUnitSelectionActive = ko.observable(false);
	this.ApplicationSelectionActive = ko.observable(false);
	this.UserName = ko.observable();
	this.Password = ko.observable();
	this.DataSource = ko.observable();
	this.ApplicationAuthentication = ko.observable(false);
};



var signinViewModel = new SignInViewModel(["Teleopti CCC Main", "TestData", "372 Data"]);
ko.applyBindings(signinViewModel, $('#login')[0]);

$('#windowsLogon').click(function() {
	signinViewModel.ApplicationAuthentication(false);
});

$('#applicationLogon').click(function () {
	signinViewModel.ApplicationAuthentication(true);
});

$('#logonButton').click(function () {
	signinViewModel.BusinessUnitSelectionActive(true);
	signinViewModel.DataSourceSelectionActive(false);
	signinViewModel.AvailableBusinessUnits(["BU1", "BU2", "BU3"]);
});

$('#buSelect').click(function () {
	signinViewModel.ApplicationSelectionActive(true);
	signinViewModel.BusinessUnitSelectionActive(false);
});

$('#buCancel').click(function () {
	signinViewModel.BusinessUnitSelectionActive(false);
	signinViewModel.DataSourceSelectionActive(true);
});