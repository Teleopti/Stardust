$(document).ready(function() {

	module("Teleopti.MyTimeWeb.AppGuide.WFMAppViewModel");
	
	test("Should generate QR Code for app link correctly",
		function() {
			var target = new Teleopti.MyTimeWeb.AppGuide.WFMAppViewModel();
			target.generateAppLinkQRCode();
			equal(!!target.appQRCode(), true);
		});	

	test("Should generate QR Code for MyTimeWeb configuration Url correctly",
		function() {
			var target = new Teleopti.MyTimeWeb.AppGuide.WFMAppViewModel();
			target.generateMyTimeQRCode();

			var expectedUrl = window.location.origin + Teleopti.MyTimeWeb.AjaxSettings.baseUrl;
			equal(!!target.myTimeQRCode(), true);
			equal(target.myTimeWebBaseUrl(), expectedUrl);
		});
});