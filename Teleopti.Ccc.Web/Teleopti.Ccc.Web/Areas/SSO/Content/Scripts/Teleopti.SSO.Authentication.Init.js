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
		}),
		forgotpassword: new Teleopti.SSO.Authentication.ForgotPasswordView({
			html: getTemplate("forgotpassword"),
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

		crossroads.addRoute(
			new RegExp('^(' + viewRegex + ')$', "i"),
			function (view) {
				_displayView({
					view: view
				});
			});
		crossroads.addRoute(
			new RegExp('^(changepassword)$', "i"),
			function () {
				_displayView({
					view: "changepassword",
					mustChangePassword: false
				});
			});
		crossroads.addRoute(
			new RegExp('^(mustchangepassword)$', "i"),
			function () {
				_displayView({
					view: "changepassword",
					mustChangePassword: true
				});
			});
		crossroads.addRoute(new RegExp('^(forgotpassword)$', "i"),
			function () {
				_displayView({ view: "forgotpassword" });
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
