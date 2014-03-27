
Teleopti.SSO.Authentication.NavigationConstructor = function () {
	this.GotoChangePassword = function (dataSourceName) {
		window.location.hash = 'changepassword/' + encodeURIComponent(dataSourceName);
	};
	this.GotoMustChangePassword = function (dataSourceName) {
		window.location.hash = 'mustchangepassword/' + encodeURIComponent(dataSourceName);
	};
	this.GotoReturnUrl = function(returnUrl, pendingRequest) {
		var form = $('<form></form>');

		form.attr("method", "post");
		form.attr("action", returnUrl);

		var field = $('<input></input>');

		field.attr("type", "hidden");
		field.attr("name", "pendingRequest");
		field.attr("value", pendingRequest);

		form.append(field);

		// The form needs to be a part of the document in
		// order for us to be able to submit it.
		$(document.body).append(form);
		form.submit();
	};
	this.GotoSignIn = function () {
		window.location.hash = '';
	};
};
Teleopti.SSO.Authentication.Navigation = new Teleopti.SSO.Authentication.NavigationConstructor();
