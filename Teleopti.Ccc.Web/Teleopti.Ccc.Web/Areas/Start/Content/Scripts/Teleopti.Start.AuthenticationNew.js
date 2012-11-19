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
