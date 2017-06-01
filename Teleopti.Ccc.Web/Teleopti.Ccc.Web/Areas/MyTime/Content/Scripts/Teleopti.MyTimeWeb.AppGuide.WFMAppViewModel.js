Teleopti.MyTimeWeb.AppGuide.WFMAppViewModel = function () {
	var self = this;

	self.myTimeWebBaseUrl = ko.observable("");
	self.androidAppLink = ko.observable("https://play.google.com/store/apps/details?id=com.teleopti.mobile");
	self.iOSAppLink = ko.observable("https://itunes.apple.com/us/app/teleopti-wfm-mytime/id1196833421#");
	self.appQRCodeUrl = ko.observable(window.location.origin + Teleopti.MyTimeWeb.AjaxSettings.baseUrl + "Static/appdownload.html");
	self.myTimeCustomUrl = ko.observable("");
	self.myTimeQRCode = ko.observable("");
	self.appQRCode = ko.observable("");

	self.init = function (url) {
		self.myTimeCustomUrl(url);
		self.generateMyTimeQRCode();
		self.generateAppLinkQRCode();
	}

	self.generateMyTimeQRCode = function () {
		var customUrl = self.myTimeCustomUrl();
		var codeUrl = window.location.origin + Teleopti.MyTimeWeb.AjaxSettings.baseUrl;
		if (!!customUrl) {
			codeUrl = customUrl;
		}
		self.myTimeWebBaseUrl(codeUrl);
		self.myTimeQRCode(drawQrcode(codeUrl));
	};

	self.generateAppLinkQRCode = function () {
		self.appQRCode(drawQrcode(self.appQRCodeUrl()));
	};

	function drawQrcode(url) {
		if (!!url) {
			var qr = qrcode(7, 'M');
			qr.addData(url);
			qr.make();
			return qr.createImgTag(3, 0);
		}
	}
};