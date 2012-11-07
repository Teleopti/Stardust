/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />

var SignInViewModel = function () {
	var self = this;

	this.AvailableDataSources = ko.observableArray();
	this.AvailableBusinessUnits = ko.observableArray();
	this.AvailableApplications = ko.observableArray();

	this.SelectedSource = ko.observable();
	this.SelectedBusinessUnit = ko.observable();
	this.SelectedApplication = ko.observable();

	this.DataSourceSelectionActive = ko.observable(true);
	this.BusinessUnitSelectionActive = ko.observable(false);
	this.ApplicationSelectionActive = ko.observable(false);
	this.WarningActive = ko.observable(false);
	this.MustChangePasswordActive = ko.observable(false);
	this.ChangingPasswordActive = ko.observable(false);

	this.UserName = ko.observable();
	this.Password = ko.observable();

	this.SelectDataSource = function () {
		if (self.SelectedSource())
			self.SelectedSource().Selected(false);
		this.Selected(true);
		self.SelectedSource(this);
	};

	this.SelectBusinessUnit = function () {
		if (self.SelectedBusinessUnit())
			self.SelectedBusinessUnit().Selected(false);
		this.Selected(true);
		self.SelectedBusinessUnit(this);


		var app1 = new ApplicationViewModel();
		app1.ApplicationName = "Team Leader Tool";
		var app2 = new ApplicationViewModel();
		app2.ApplicationName = "Mytime Web";
		var app3 = new ApplicationViewModel();
		app3.ApplicationName = "Anywhere Report";
		var apps = [app1, app2, app3];
		self.AvailableApplications(apps);
		apps[0].Selected(true);
		self.SelectedApplication(apps[0]);
		self.ApplicationSelectionActive(true);
		self.BusinessUnitSelectionActive(false);
	};

	this.SelectApplication = function () {
		if (self.SelectedApplication())
			self.SelectedApplication().Selected(false);
		this.Selected(true);
		self.SelectedApplication(this);
	};

	this.ToggleChangingPassword = function () {
		self.ChangingPasswordActive(!self.ChangingPasswordActive());
	};
};

var DataSourceViewModel = function () {
	this.DataSourceName = "";
	this.Selected = ko.observable(false);
	this.ApplicationAuthentication = ko.observable(false);
};

var BusinessUnitViewModel = function () {
	this.BusinessUnitName = "";
	this.Selected = ko.observable(false);
};

var ApplicationViewModel = function () {
	this.ApplicationName = "";
	this.Selected = ko.observable(false);
	this.ApplicationUrl = "";
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

var signinViewModel = new SignInViewModel();
var datas = [data1, data2, data3, data4];
signinViewModel.AvailableDataSources(datas);
datas[0].Selected(true);
signinViewModel.SelectedSource(datas[0]);
ko.applyBindings(signinViewModel, $('#Login-container')[0]);



$('#Login-button').click(function () {
	if ($('#Username-input').val() == "1") {
		signinViewModel.WarningActive(true);
		signinViewModel.ChangingPasswordActive(false);
	}
	if ($('#Username-input').val() == "2") {
		signinViewModel.MustChangePasswordActive(true);
		signinViewModel.ChangingPasswordActive(true);
		signinViewModel.DataSourceSelectionActive(false);
		return;
	}
	signinViewModel.BusinessUnitSelectionActive(true);
	signinViewModel.DataSourceSelectionActive(false);
	var bu1 = new BusinessUnitViewModel();
	bu1.BusinessUnitName = "BU1";
	var bu2 = new BusinessUnitViewModel();
	bu2.BusinessUnitName = "BU2";
	var bu3 = new BusinessUnitViewModel();
	bu3.BusinessUnitName = "BU3";
	var bus = [bu1, bu2, bu3];
	signinViewModel.AvailableBusinessUnits(bus);
	bus[0].Selected(true);
	signinViewModel.SelectedBusinessUnit(bus[0]);

});

$('#Change-password-button').click(function () {
	signinViewModel.DataSourceSelectionActive(true);
	signinViewModel.BusinessUnitSelectionActive(false);
	signinViewModel.MustChangePasswordActive(false);
	signinViewModel.WarningActive(false);
});

//$('#Businessunit-cancel-button').click(function () {
//	signinViewModel.BusinessUnitSelectionActive(false);
//	signinViewModel.DataSourceSelectionActive(true);
//	signinViewModel.WarningActive(false);
//});

//$('#Application-cancel-button').click(function () {
//	signinViewModel.ApplicationSelectionActive(false);
//	signinViewModel.BusinessUnitSelectionActive(true);
//});