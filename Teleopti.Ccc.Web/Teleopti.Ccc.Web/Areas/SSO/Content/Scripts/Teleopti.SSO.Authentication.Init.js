/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/SSO/Content/Scripts/Teleopti.SSO.Common.js" />
/// <reference path="~/Areas/SSO/Content/Scripts/Teleopti.SSO.Authentication.ChangePasswordView.js" />

Teleopti.SSO.Authentication.Init = function () {

	var defaultView = "signin";

	var getTemplate = function (view) {
		var template = $('#' + view);
		var html = template.html();
		template.remove();
		return html;
	};

	var authenticationState = new Teleopti.SSO.Authentication.AuthenticationState({
		baseUrl: Teleopti.SSO.Authentication.Settings.baseUrl
	});

	var views = {
		signin: new Teleopti.SSO.Authentication.SignInView({
			html: getTemplate("signin"),
			baseUrl: Teleopti.SSO.Authentication.Settings.baseUrl,
			authenticationState: authenticationState
		}),
		changepassword: new Teleopti.SSO.Authentication.ChangePasswordView({
			html: getTemplate("changepassword"),
			baseUrl: Teleopti.SSO.Authentication.Settings.baseUrl,
			authenticationState: authenticationState
		})
	};

	function _displayView(viewData) {
		viewData.render = function (html) {
			$('#view').html(html);
		};
		viewData.element = $('#view');
		viewData.authenticationState = authenticationState;
		views[viewData.view].Display(viewData);
	}

	function _initRoutes() {
		var viewRegex = 'signin';
		var authenticationTypeRegex = 'windows|application';
		var dataSourceNameRegex = '.*';
		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')/(' + authenticationTypeRegex + ')/(' + dataSourceNameRegex + ')$', "i"),
			function (view, authenticationType, dataSourceName) {
				_displayView({
					view: view,
					authenticationType: authenticationType,
					dataSourceName: decodeURIComponent(dataSourceName)
				});
			});
		crossroads.addRoute(
			new RegExp('^(changepassword)/(' + dataSourceNameRegex + ')$', "i"),
			function (view, dataSourceName) {
				_displayView({
					view: "changepassword",
					mustChangePassword: false,
					dataSourceName: decodeURIComponent(dataSourceName)
				});
			});
		crossroads.addRoute(
			new RegExp('^(mustchangepassword)/(' + dataSourceNameRegex + ')$', "i"),
			function (view, dataSourceName) {
				_displayView({
					view: "changepassword",
					mustChangePassword: true,
					dataSourceName: decodeURIComponent(dataSourceName)
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
		crossroads.bypassed.add(function () {
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
