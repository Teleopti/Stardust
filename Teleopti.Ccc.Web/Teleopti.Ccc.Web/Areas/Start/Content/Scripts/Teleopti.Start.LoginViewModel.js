/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

Teleopti.Start.SignInViewModel = function () {
	var self = this;
	var ajax = new Teleopti.MyTimeWeb.Ajax();

	this.AvailableDataSources = ko.observableArray();
	this.AvailableBusinessUnits = ko.observableArray();
	this.AvailableApplications = ko.observableArray();

	this.SelectedSource = ko.observable();
	this.SelectedBusinessUnit = ko.observable();
	this.SelectedApplication = ko.observable();

	this.DataSourceSelectionActive = ko.observable(true);
	this.BusinessUnitSelectionActive = ko.observable(false);
	this.ApplicationSelectionActive = ko.observable(false);
	this.MustChangePasswordActive = ko.observable(false);

	this.WarningActive = ko.observable(false);
	this.ChangingPasswordActive = ko.observable(false);
	this.IsApplicationLogon = ko.computed(function () {
		if (self.SelectedSource()) {
			return self.SelectedSource().ApplicationAuthentication();
		} else {
			return true;
		}
	});

	this.UserName = ko.observable();
	this.Password = ko.observable();
	this.ErrorMessage = ko.observable('');

	this.PersonId = '';
	this.AuthenticationType = 0;

	this.DisplayBusinessUnitSelection = function () {
		self.BusinessUnitSelectionActive(true);
		self.DataSourceSelectionActive(false);
		self.ApplicationSelectionActive(false);
		self.MustChangePasswordActive(false);
	};

	this.SelectDataSource = function () {
		if (self.SelectedSource())
			self.SelectedSource().Selected(false);
		this.Selected(true);
		self.SelectedSource(this);
	};

	this.LoadDataSources = function () {
		var a = this.alskdjaskd;
		ajax.Ajax({
			url: "/Start/AuthenticationNew/DataSources",
			dataType: "json",
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				//var a = self.alskdjaskd;
				for (var i = 0; i < data.length; i++) {
					var dataSource = new Teleopti.Start.DataSourceViewModel();
					dataSource.Selected(false);
					dataSource.Name = data[i].Name;
					dataSource.Type = data[i].Type;
					dataSource.ApplicationAuthentication(data[i].Type == "application");
					if (i == 0) {
						dataSource.Selected(true);
						self.SelectedSource(dataSource);
					}
					self.AvailableDataSources.push(dataSource);
				}

			}
		});
	};

	function _prefixModel(prefix, model) {
		var prefixedModel = {};
		$.each(model, function (key, val) {
			prefixedModel[prefix + key] = val;
		});
		return prefixedModel;
	}

	function _buildAuthenticationModel() {
		if (self.SelectedSource().Type === "windows") {
			return {
				type: "windows",
				datasource: self.SelectedSource().Name
			};
		}
		if (self.SelectedSource().Type === "application") {
			return {
				type: "application",
				datasource: self.SelectedSource().Name,
				username: self.UserName,
				password: self.Password
			};
		}
		return null;
	}

	this.Logon = function () {
		//var model;
		//var url;
		//		if (self.IsApplicationLogon()) {
		//			model = { "DataSourceName": self.SelectedSource().Name, "UserName": self.UserName(), "Password": self.Password() };
		//			url = "/Start/Authentication/Application";
		//		} else {
		//			model = { "DataSourceName": self.SelectedSource().Name };
		//			url = "/Start/Authentication/Windows";
		//		}
		var url = "/Start/AuthenticationNew/BusinessUnits";
		ajax.Ajax({
			url: url,
			dataType: 'json',
			//type: 'POST',
			//data: _prefixModel('SignIn.', model),
			data: _buildAuthenticationModel(),
			success: function (data, textStatus, jqXHR) {

				self.DisplayBusinessUnitSelection();
				for (var i = 0; i < data.length; i++) {
					var businessUnit = new Teleopti.Start.BusinessUnitViewModel();
					businessUnit.Selected(false);
					businessUnit.Name = data[i].Name;
					businessUnit.Id = data[i].Id;
					self.AvailableBusinessUnits.push(businessUnit);
				}

				//				
				//				if (data.BusinessUnits && data.BusinessUnits.length > 1) {
				//					self.BusinessUnitSelectionActive(true);
				//					self.DataSourceSelectionActive(false);
				//					for (var i = 0; i < data.BusinessUnits.length; i++) {
				//						var businessUnit = new Teleopti.Start.BusinessUnitViewModel();
				//						businessUnit.Selected(false);
				//						businessUnit.BusinessUnitName = data.BusinessUnits[i].Name;
				//						businessUnit.BusinessUnitId = data.BusinessUnits[i].Id;
				//						self.AvailableBusinessUnits.push(businessUnit);
				//					}
				//					self.PersonId = data.SignIn.PersonId;
				//					self.AuthenticationType = data.SignIn.AuthenticationType;
				//				}
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.ErrorMessage(data.Errors);
				}
			}
		});
	};

	this.SelectBusinessUnit = function () {
		if (self.SelectedBusinessUnit())
			self.SelectedBusinessUnit().Selected(false);

		this.Selected(true);
		self.SelectedBusinessUnit(this);

//		var model = {
//			BusinessUnitId: this.Id,
//			AuthenticationType: self.AuthenticationType,
//			PersonId: self.PersonId,
//			DataSourceName: self.SelectedSource().Name
//		};

		var model = _buildAuthenticationModel();
		model.businessUnitId = self.SelectedBusinessUnit().Id;

		ajax.Ajax({
			url: "/Start/AuthenticationNew/Logon",
			//data: _prefixModel('SignIn.', model),
			data: model,
			type: 'POST',
			success: function (data, textStatus, jqXHR) {
				alert("signed in!");
			}
		});

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

Teleopti.Start.DataSourceViewModel = function () {
	this.Name = "";
	this.Type = "";
	this.Selected = ko.observable(false);
	this.ApplicationAuthentication = ko.observable(false);
};

Teleopti.Start.BusinessUnitViewModel = function () {
	this.Id = "";
	this.Name = "";
	this.Selected = ko.observable(false);
};

Teleopti.Start.ApplicationViewModel = function () {
	this.ApplicationName = "";
	this.Selected = ko.observable(false);
	this.ApplicationUrl = "";
};

