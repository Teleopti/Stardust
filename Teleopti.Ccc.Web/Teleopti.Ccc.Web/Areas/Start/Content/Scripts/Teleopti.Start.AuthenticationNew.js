/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/Start/Content/Scripts/Teleopti.Start.Common.js" />
/// <reference path="~/Areas/Start/Content/Scripts/Teleopti.Start.LoginViewModel.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.Start) === 'undefined') {
		Teleopti.Start = {};
	}
}

Teleopti.Start.Authentication = function () {

	var defaultView = "signin";

	var getTemplate = function(view) {
		var template = $('#' + view);
		var html = template.html();
		template.remove();
		return html;
	};
	
	var views = {
		signin: new Teleopti.Start.Authentication.SignInView(getTemplate("signin")),
		businessunit: new Teleopti.Start.Authentication.BusinessUnitSelectionView(getTemplate("businessunit")),
		menu: new Teleopti.Start.Authentication.MenuView(getTemplate("menu")),
		changepassword: new Teleopti.Start.Authentication.ChangePasswordView(getTemplate("changepassword")),
	};
	
	function _displayView(viewData) {
		viewData.render = function(html) {
			$('#view').html(html);
		};
		viewData.element = $('#view');
		views[viewData.view].Display(viewData);
	}

	function _initRoutes() {
		var viewRegex = '[a-z]+';
		var authenticationTypeRegex = 'windows|application';
		var dataSourceNameRegex = '.*';
		crossroads.addRoute(
				new RegExp('^(' + viewRegex + ')/(' + authenticationTypeRegex + ')/(' + dataSourceNameRegex + ')$', "i"),
				function (view, authenticationType, dataSourceName) {
					_displayView({
						 view: view,
						 authenticationType: authenticationType,
						 dataSourceName: dataSourceName
					});
				});
		crossroads.addRoute(
				new RegExp('^(' + viewRegex + ')$', "i"),
				function (view) {
					_displayView({ view: view });
				});
		crossroads.addRoute('', function () {
			_displayView({ view: defaultView });
		});
	}

	function _initHasher() {
		var parseHash = function (newHash, oldHash) {
			crossroads.parse(newHash);
		};
		hasher.initialized.add(parseHash);
		hasher.changed.add(parseHash);
		hasher.init();
	}

	_initRoutes();
	_initHasher();

};

Teleopti.Start.Authentication.NavigationConstructor = function() {
	this.GotoBusinessUnits = function(authenticationType, dataSourceName) {
		window.location.hash = 'businessunit/' + authenticationType + '/' + dataSourceName;
	};
};
Teleopti.Start.Authentication.Navigation = new Teleopti.Start.Authentication.NavigationConstructor();

Teleopti.Start.Authentication.Settings = { baseUrl: '' };

Teleopti.Start.Authentication.DataSourceViewModel = function (data) {
	var self = this;
	this.Name = "";
	this.Type = "";
	this.Selected = ko.observable(false);
	this.Select = function() {
		data.events.notifySubscribers(self, 'dataSourceSelected');
	};
};

Teleopti.Start.Authentication.SignInViewModel = function(data) {
	var self = this;
	
	this.DataSources = ko.observableArray();
	this.UserName = ko.observable();
	this.Password = ko.observable();
	this.ErrorMessage = ko.observable();
	
	this.SelectedDataSource = ko.computed(function () {
		return ko.utils.arrayFirst(self.DataSources(), function(d) {
			return d.Selected();
		});
	});
	
	this.DisplayUserNameAndPasswordBoxes = ko.computed(function() {
		var selected = self.SelectedDataSource();
		if (selected)
			return selected.Type == "application";
		return false;
	});

	data.events.subscribe(function (dataSource) {
		ko.utils.arrayForEach(self.DataSources(), function(d) {
			d.Selected(d === dataSource);
		});
	}, null, "dataSourceSelected");
	
	function _buildAuthenticationModel() {
		if (self.SelectedDataSource().Type === "windows") {
			return {
				type: "windows",
				datasource: self.SelectedDataSource().Name
			};
		}
		if (self.SelectedDataSource().Type === "application") {
			return {
				type: "application",
				datasource: self.SelectedDataSource().Name,
				username: self.UserName,
				password: self.Password
			};
		}
		return null;
	}

	this.LoadDataSources = function() {
		var events = data.events;
		$.ajax({
			url: data.baseUrl + "Start/AuthenticationApi/DataSources",
			dataType: "json",
			type: 'GET',
			success: function(data, textStatus, jqXHR) {
				self.DataSources.removeAll();
				for (var i = 0; i < data.length; i++) {
					var dataSource = new Teleopti.Start.Authentication.DataSourceViewModel({
						events: events
					});
					dataSource.Selected(i == 0);
					dataSource.Name = data[i].Name;
					dataSource.Type = data[i].Type;
					self.DataSources.push(dataSource);
				}
			}
		});
	};

	this.SignIn = function() {
		$.ajax({
			url: data.baseUrl + "Start/AuthenticationApi/BusinessUnits",
			dataType: "json",
			type: 'GET',
			data: _buildAuthenticationModel(),
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.ErrorMessage(data.Errors);
				}
			},
			success: function(data, textStatus, jqXHR) {
				Teleopti.Start.Authentication.Navigation.GotoBusinessUnits(self.SelectedDataSource().Type, self.SelectedDataSource().Name);
			}
		});
	};
	
};

Teleopti.Start.Authentication.SignInView = function (html) {
	
	var events = new ko.subscribable();
	
	this.Display = function (data) {
		var viewModel = new Teleopti.Start.Authentication.SignInViewModel({
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl,
			events: events
		});
		data.render(html);
		ko.applyBindings(viewModel, data.element[0]);
		viewModel.LoadDataSources();
	};

};

Teleopti.Start.Authentication.BusinessUnitSelectionViewModel = function(data) {
	var self = this;
	this.BusinessUnits = ko.observableArray();
	
	this.LoadBusinessUnits = function ()
	{
		$.ajax({
			url: baseUrl + "Start/AuthenticationApi/BusinessUnits",
			dataType: 'json',
			data: _buildAuthenticationModel(),
			success: function (data, textStatus, jqXHR) {
				for (var i = 0; i < data.length; i++) {
					var businessUnit = new Teleopti.Start.BusinessUnitViewModel();
					businessUnit.Selected(false);
					businessUnit.Name = data[i].Name;
					businessUnit.Id = data[i].Id;
					self.AvailableBusinessUnits.push(businessUnit);
				}
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.ErrorMessage(data.Errors);
				}
			}
		});
	}
};

Teleopti.Start.Authentication.BusinessUnitSelectionView = function (html) {
	this.Display = function (data) {
		data.render(html);
		var viewModel = new Teleopti.Start.Authentication.BusinessUnitSelectionView({
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl
		});
		data.render(html);
		ko.applyBindings(viewModel, data.element[0]);
		viewModel.LoadBusinessUnits();
	};
};

Teleopti.Start.Authentication.MenuView = function (html) {
	this.Display = function (data) {

	};
};

Teleopti.Start.Authentication.ChangePasswordView = function (html) {
	this.Display = function (data) {

	};
};

