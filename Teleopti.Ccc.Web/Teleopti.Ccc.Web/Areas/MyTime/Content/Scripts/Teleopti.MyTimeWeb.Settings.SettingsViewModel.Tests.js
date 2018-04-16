
$(document).ready(function() {

	module("Teleopti.MyTimeWeb.Settings.SettingsViewModel");
	
	test("should enable set agent description", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (toggleName) {
			return toggleName == 'Settings_SetAgentDescription_23257';
		};

		var viewModel = new Teleopti.MyTimeWeb.Settings.SettingsViewModel();
		viewModel.featureCheck();

		equal(viewModel.isSetAgentDescriptionEnabled(), true);
	});

	test("should disable set agent description", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (toggleName) {
			return toggleName != 'Settings_SetAgentDescription_23257';
		};

		var viewModel = new Teleopti.MyTimeWeb.Settings.SettingsViewModel();
		viewModel.featureCheck();

		equal(viewModel.isSetAgentDescriptionEnabled(), false);
	});

	test("should load cultures", function () {
		var cultures = [{ id: "1", text: "Culture A" },
			{ id: "2", text: "Culture B" },
			{ id: "3", text: "Culture C" }];
		var nameFormats = [{ id: "0", text: "[FirstName] [LastName]" }];
		var ajax = {
			Ajax: function (options) {
				if (options.url == "Settings/GetSettings") {
					options.success(
						{
							Cultures: cultures,
							ChoosenUiCulture: { id: 2 },
							ChoosenCulture: { id: 1 },
							ChosenNameFormat: { id: 0 },
							NameFormats: nameFormats
						}
					);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Settings.SettingsViewModel(ajax);

		viewModel.loadSettings();

		equal(viewModel.cultures().length, 3);
		equal(viewModel.cultures()[0].id, 1);
		equal(viewModel.cultures()[0].text, "Culture A");
		equal(viewModel.selectedUiCulture(), 2);
		equal(viewModel.selectedCulture(), 1);
		equal(viewModel.selectedNameFormat(), 0);
		equal(viewModel.nameFormats().length, 1);
		equal(viewModel.settingsLoaded(), true);
		equal(viewModel.avoidReload, false);
	});

	test("should get correct url for QRcode", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (toggleName) {
			return toggleName == 'QRCodeForMobileApps_42695';
		};

		var fakeQRcodeContainer = document.createElement('div');
		fakeQRcodeContainer.setAttribute('id', 'QRCodePlaceHolder');
		document.body.appendChild(fakeQRcodeContainer);

		var fakeAndroidAppContainer = document.createElement('div');
		fakeAndroidAppContainer.setAttribute('id', 'AndroidApp');
		document.body.appendChild(fakeAndroidAppContainer);
		
		var fakeiOSAppContainer = document.createElement('div');
		fakeiOSAppContainer.setAttribute('id', 'iOSApp');
		document.body.appendChild(fakeiOSAppContainer);

		var viewModel = new Teleopti.MyTimeWeb.Settings.SettingsViewModel();
		viewModel.hasPermissionToViewQRCode(true);
		viewModel.featureCheck();
		//let agent has view QRCode permission

		equal(viewModel.isQRCodeForMobileAppsEnabled(), true);
		equal(viewModel.customMobileAppBaseUrl(), false);

		viewModel.generateQRCode();
		var expectedUrl = window.location.origin + Teleopti.MyTimeWeb.AjaxSettings.baseUrl;
		equal(viewModel.myTimeWebBaseUrl(), expectedUrl);

		document.body.removeChild(fakeQRcodeContainer);
		document.body.removeChild(fakeAndroidAppContainer);
		document.body.removeChild(fakeiOSAppContainer);
	});
});
