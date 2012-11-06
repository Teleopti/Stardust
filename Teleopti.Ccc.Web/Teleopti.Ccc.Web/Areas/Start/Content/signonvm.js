/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
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
	this.SelectedSource = ko.observable();
};

var DataSourceViewModel = function () {
	this.self = this;
	this.ApplicationAuthentication = ko.observable(false);
	this.DataSourceName = "";
};


var data1 = new DataSourceViewModel();
data1.DataSourceName = "Teleopti CCC Main";
data1.ApplicationAuthentication(true);

var data2 = new DataSourceViewModel();
data2.DataSourceName = "Teleopti CCC Main (Windows)";
data2.ApplicationAuthentication(false);

var data3 = new DataSourceViewModel();
data3.DataSourceName = "TestData";
data3.ApplicationAuthentication(true);

var data4 = new DataSourceViewModel();
data4.DataSourceName = "372 Data";
data4.ApplicationAuthentication(true);

var signinViewModel = new SignInViewModel([data1, data2, data3, data4]);
ko.applyBindings(signinViewModel, $('#login')[0]);

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