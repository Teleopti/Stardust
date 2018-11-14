
Teleopti.SSO.Authentication.NavigationConstructor = function () {
	this.GotoChangePassword = function () {
		window.location.hash = 'changepassword';
	};
	this.GotoMustChangePassword = function () {
		window.location.hash = 'mustchangepassword';
	};
	this.GotoForgotPassword = function () {
		window.location.hash = 'forgotpassword';
	}
	this.GotoReturnUrl = function(returnUrl, pendingRequest, rememberMe) {
		var form = $('<form></form>');

		form.attr("method", "post");
		form.attr("action", returnUrl);

		var requestField = $('<input></input>');
		requestField.attr("type", "hidden");
		requestField.attr("name", "pendingRequest");
		requestField.attr("value", pendingRequest);
		form.append(requestField);

		var isPersistentField = $('<input></input>');
		isPersistentField.attr("type", "hidden");
		isPersistentField.attr("name", "isPersistent");
		isPersistentField.attr("value", rememberMe);
		form.append(isPersistentField);

		$(document.body).append(form);
		form.submit();
	};
	this.GotoSignIn = function () {
		window.location.hash = '';
	};
};
Teleopti.SSO.Authentication.Navigation = new Teleopti.SSO.Authentication.NavigationConstructor();
