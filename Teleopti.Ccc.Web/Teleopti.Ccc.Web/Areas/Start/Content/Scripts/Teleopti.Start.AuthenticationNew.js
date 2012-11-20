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

	var authenticationState = new Teleopti.Start.Authentication.AuthenticationState({
		baseUrl: Teleopti.Start.Authentication.Settings.baseUrl
	});
	
	var views = {
		signin: new Teleopti.Start.Authentication.SignInView({
			html: getTemplate("signin"),
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl,
			authenticationState: authenticationState
		}),
		businessunit: new Teleopti.Start.Authentication.BusinessUnitSelectionView({
			html: getTemplate("businessunit"),
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl,
			authenticationState: authenticationState
		}),
		menu: new Teleopti.Start.Authentication.MenuView({
			html: getTemplate("menu"),
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl,
			authenticationState: authenticationState
		}),
		changepassword: new Teleopti.Start.Authentication.ChangePasswordView({
			html: getTemplate("changepassword"),
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl,
			authenticationState: authenticationState
		}),
	};
	
	function _displayView(viewData) {
		viewData.render = function(html) {
			$('#view').html(html);
		};
		viewData.element = $('#view');
		viewData.authenticationState = authenticationState;
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

Teleopti.Start.Authentication.AuthenticationState = function(data) {
	
	var authenticationModel;
	var businessUnits;

	var loadBusinessUnits = function(options) {
		var error = function(jqXHR, textStatus, errorThrown) {
			options.error(jqXHR, textStatus, errorThrown);
		};
		
		var success = function(responseData, textStatus, jqXHR) {
			businessUnits = responseData;
			options.success(responseData, textStatus, jqXHR);
		};
		
		$.ajax({
			url: data.baseUrl + "Start/AuthenticationApi/BusinessUnits",
			dataType: "json",
			type: 'GET',
			data: authenticationModel,
			error: error,
			success: success
		});
	};
	
	this.ForceLoadBusinessUnits = function(options) {
		authenticationModel = options.data;
		loadBusinessUnits(options);
	};
	

	this.MightLoadBusinessUnits = function(options) {
		if (authenticationModel)
			$.extend(authenticationModel, options.data);
		else
			authenticationModel = options.data;
		
		if (businessUnits) {
			options.success(businessUnits);
			return;
		}

		loadBusinessUnits(options);
	};
	
};

