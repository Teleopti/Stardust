/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js"/>

Teleopti.MyTimeWeb.Settings.SettingsViewModel = function (ajax) {
	var self = this;
	
	self.isSetAgentDescriptionEnabled = ko.observable(false);
	self.isQRCodeForMobileAppsEnabled = ko.observable(false);
	self.customMobileAppBaseUrl = ko.observable(false);
	self.customMobileAppBaseUrlError = ko.observable(false);
	self.myTimeWebBaseUrl = ko.observable();
	self.androidAppLink = ko.observable("https://play.google.com/store/apps/details?id=com.teleopti.mobile");
	self.iOSAppLink = ko.observable("https://itunes.apple.com/us/app/teleopti-wfm-mytime/id1196833421#");
	self.settingsLoaded = ko.observable(false);
	self.avoidReload = false;
	self.cultures = ko.observableArray();
	self.selectedUiCulture = ko.observable();
	self.selectedCulture = ko.observable();
	self.nameFormats = ko.observableArray();
	self.selectedNameFormat = ko.observable();
	self.hasPermissionToViewQRCode = ko.observable(false);

	self.CalendarSharingActive = ko.observable(false);
	self.CalendarUrl = ko.observable();
	self.ActivateCalendarSharing = function () {
		self.setCalendarLinkStatus(true);
	};
	self.DeactivateCalendarSharing = function () {
		self.setCalendarLinkStatus(false);
	};

	self.IsNotificationSupported = ko.computed(function () {
		if (window.webkitNotifications)
			return true;
		return false;
	});
	
	self.RequestNotificationPermission = function () {
		if (window.webkitNotifications && window.webkitNotifications.checkPermission() != 0) {
			window.webkitNotifications.requestPermission();
		}
	};

	self.selectedUiCulture.subscribe(function (newValue) {
		if (!self.avoidReload)
			self.selectorChanged(newValue, "Settings/UpdateUiCulture");
	});

	self.selectedCulture.subscribe(function (newValue) {
		if (!self.avoidReload)
			self.selectorChanged(newValue, "Settings/UpdateCulture");
	});

	self.selectedNameFormat.subscribe(function (newValue)
	{
		if (!self.avoidReload) self.nameFormatChanged(newValue);
	});

	self.loadSettings = function() {
		return ajax.Ajax({
			url: "Settings/GetSettings",
			dataType: "json",
			type: "GET",
			global: false,
			cache: false,
			success: function(data, textStatus, jqXHR) {
				self.cultures(data.Cultures);
				self.nameFormats(data.NameFormats);
				self.avoidReload = true;
				self.selectedUiCulture(data.ChoosenUiCulture.id);
				self.selectedCulture(data.ChoosenCulture.id);
				self.selectedNameFormat(data.ChosenNameFormat.id);
				self.avoidReload = false;
				self.settingsLoaded(true);
				self.hasPermissionToViewQRCode(data.HasPermissionToViewQRCode);
			}
		});
	};
	
	self.selectorChanged = function(value, url) {
		var data = { LCID: value };
		ajax.Ajax({
			url: url,
			contentType: 'application/json; charset=utf-8',
			type: "PUT",
			data: JSON.stringify(data),
			success: function (data, textStatus, jqXHR) {
				ajax.CallWhenAllAjaxCompleted(function () {
					location.reload();
				});
			},
			error: function (jqXHR, textStatus, errorThrown) {
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	};

	self.nameFormatChanged = function (id) {
		var data = { NameFormatId: id };
		ajax.Ajax({
			url: 'Settings/UpdateNameFormat',
			contentType: 'application/json; charset=utf-8',
			type: "PUT",
			data: JSON.stringify(data),
			success: function (data, textStatus, jqXHR) {
				ajax.CallWhenAllAjaxCompleted(function () {
					location.reload();
				});
			},
			error: function (jqXHR, textStatus, errorThrown) {
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	};

	self.setCalendarLinkStatus = function(isActive) {
		ajax.Ajax({
			url: "Settings/SetCalendarLinkStatus",
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			type: "POST",
			data: JSON.stringify({ IsActive: isActive }),
			success: function(data, textStatus, jqXHR) {
				self.CalendarSharingActive(data.IsActive);
				self.CalendarUrl(data.Url);
			},
			error: function(jqXHR, textStatus, errorThrown) {
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	};
	
	self.getCalendarLinkStatus = function() {
		if ($(".share-my-calendar").length == 0)
			return null;
		return ajax.Ajax({
			url: "Settings/CalendarLinkStatus",
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			type: "GET",
			success: function (data, textStatus, jqXHR) {
				self.CalendarSharingActive(data.IsActive);
				self.CalendarUrl(data.Url);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	};

	self.featureCheck = function () {
		self.isSetAgentDescriptionEnabled(Teleopti.MyTimeWeb.Common.IsToggleEnabled("Settings_SetAgentDescription_23257"));
		self.isQRCodeForMobileAppsEnabled(Teleopti.MyTimeWeb.Common.IsToggleEnabled("QRCodeForMobileApps_42695") && self.hasPermissionToViewQRCode());
		self.customMobileAppBaseUrl(self.hasPermissionToViewQRCode() && Teleopti.MyTimeWeb.Common.IsToggleEnabled("ConfigQRCodeURLForMobileApps_43224"));
	};

	self.generateQRCode = function () {
		var defaultUrl = window.location.origin + Teleopti.MyTimeWeb.AjaxSettings.baseUrl + Teleopti.MyTimeWeb.AjaxSettings.defaultNavigation;
		if (self.customMobileAppBaseUrl()) {
			ajax.Ajax({
				url: "Settings/MobileQRCodeUrl",
				contentType: 'application/json; charset=utf-8',
				type: "GET",
				success: function (data, textStatus, jqXHR) {
					if (data == '') {
						self.myTimeWebBaseUrl(defaultUrl);
					} else {
						self.myTimeWebBaseUrl(data);
					}
					drawQrcode();
				},
				error: function (jqXHR, textStatus, errorThrown) {
					Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
					self.customMobileAppBaseUrlError(true);
				}
			});
		} else {
			self.myTimeWebBaseUrl(defaultUrl);
			drawQrcode();
		}
	};

	function drawQrcode() {
		if (self.myTimeWebBaseUrl() && self.myTimeWebBaseUrl().length > 0) {
			var typeNumber = 7;
			var errorCorrectionLevel = 'M';
			var qr = qrcode(typeNumber, errorCorrectionLevel);
			qr.addData(self.myTimeWebBaseUrl());
			qr.make();
			document.getElementById('#QRCodePlaceHolder').innerHTML = qr.createImgTag(5);
			self.generateAppLinkQRCode();
			self.setupMouseEnterLeaveEventForAppsQRCode();
		}
	}

	self.generateAppLinkQRCode = function(){
		var typeNumber = 7;
		var errorCorrectionLevel = 'M';
		var qrAndroid = qrcode(typeNumber, errorCorrectionLevel);
		qrAndroid.addData(self.androidAppLink());
		qrAndroid.make();
		document.getElementById('#AndroidApp').innerHTML = qrAndroid.createImgTag(5);

		var qriOS = qrcode(typeNumber, errorCorrectionLevel);
		qriOS.addData(self.iOSAppLink());
		qriOS.make();
		document.getElementById('#iOSApp').innerHTML = qriOS.createImgTag(5);
	};

	self.setupMouseEnterLeaveEventForAppsQRCode = function(){
		$('.android-app-link').mouseenter(function(event) {
			$('.android-app-link div').show();
		});

		$('.android-app-link').mouseleave(function(event) {
			$('.android-app-link div').hide();
		});

		$('.ios-app-link').mouseenter(function(event) {
			$('.ios-app-link div').show();
		});

		$('.ios-app-link').mouseleave(function(event) {
			$('.ios-app-link div').hide();
		});
	};
};