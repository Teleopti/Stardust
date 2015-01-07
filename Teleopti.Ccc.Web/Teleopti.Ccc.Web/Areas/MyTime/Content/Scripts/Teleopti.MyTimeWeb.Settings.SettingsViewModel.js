/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js"/>

Teleopti.MyTimeWeb.Settings.SettingsViewModel = function (ajax) {
	var self = this;
	
	self.isSetAgentDescriptionEnabled = ko.observable(false);
	self.settingsLoaded = ko.observable(false);
	self.avoidReload = false;
	self.cultures = ko.observableArray();
	self.selectedUiCulture = ko.observable();
	self.selectedCulture = ko.observable();
	self.nameFormats = ko.observableArray();
	self.selectedNameFormat = ko.observable();

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
		var toggleEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled("Settings_SetAgentDescription_23257");
		self.isSetAgentDescriptionEnabled(toggleEnabled);
	};
};