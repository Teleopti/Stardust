$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Request", {
		setup: function () {
			$('body').append('<div id="Requests-body-inner">"#Requests-body-inner" For testing</div>');
			setUpFunctionsBeforeRun();
		},
		teardown: function () {
			$('#Requests-body-inner').remove();
			restoreFuntionsAfterRun();
		}
	});

	var overtimeRequestsToggle = false,
		fakeLicenseAvailabilityData = {
			IsLicenseAvailable: false,
			HasPermissionForOvertimeRequests: false
		};

	var tempToggleFn,
		tempList,
		tempAjax,
		tempAddShiftTradeRequest;

	test('should not show overtime request tab when has no license availability', function () {
		Teleopti.MyTimeWeb.Request.RequestPartialInit(null, null);

		var target = Teleopti.MyTimeWeb.Request.RequestNavigationViewModel();

		equal(target.overtimeRequestsLicenseAvailable(), false);
	});

	test('should not show overtime request tab when has permission but no license availability', function () {
		fakeLicenseAvailabilityData.IsLicenseAvailable = true;
		fakeLicenseAvailabilityData.HasPermissionForOvertimeRequests = false;

		Teleopti.MyTimeWeb.Request.RequestPartialInit(null, null);
		var target = Teleopti.MyTimeWeb.Request.RequestNavigationViewModel();

		equal(target.overtimeRequestsLicenseAvailable(), false);
	});

	test('should show overtime request tab when has license but no permission', function () {
		fakeLicenseAvailabilityData.IsLicenseAvailable = true;
		fakeLicenseAvailabilityData.HasPermissionForOvertimeRequests = false;

		Teleopti.MyTimeWeb.Request.RequestPartialInit(null, null);
		var target = Teleopti.MyTimeWeb.Request.RequestNavigationViewModel();

		equal(target.overtimeRequestsLicenseAvailable(), false);
	});

	test('should show overtime request tab when has license availability and permission', function () {
		fakeLicenseAvailabilityData.IsLicenseAvailable = true;
		fakeLicenseAvailabilityData.HasPermissionForOvertimeRequests = true;

		Teleopti.MyTimeWeb.Request.RequestPartialInit(null, null);
		var target = Teleopti.MyTimeWeb.Request.RequestNavigationViewModel();

		equal(target.overtimeRequestsLicenseAvailable(), true);
	});

	function setUpFunctionsBeforeRun() {
		tempToggleFn = Teleopti.MyTimeWeb.Common.IsToggleEnabled;
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (toggle) {
			if (toggle === 'MyTimeWeb_OvertimeRequest_44558') {
				return overtimeRequestsToggle;
			}
		};

		tempList = Teleopti.MyTimeWeb.Request.List;
		Teleopti.MyTimeWeb.Request.List = {
			Init: function () { }
		};

		tempAddShiftTradeRequest = Teleopti.MyTimeWeb.Request.AddShiftTradeRequest;
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = {
			Init: function () { }
		};

		tempAjax = Teleopti.MyTimeWeb.Ajax;
		Teleopti.MyTimeWeb.Ajax = function () {
			return {
				Ajax: function (option) {
					if (option.url === 'OvertimeRequests/GetLicenseAvailability') {
						return option.success(fakeLicenseAvailabilityData);
					}
				}
			};
		};
	}

	function restoreFuntionsAfterRun() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = tempToggleFn;
		Teleopti.MyTimeWeb.Request.List = tempList;
		Teleopti.MyTimeWeb.Ajax = tempAjax;
		Teleopti.MyTimeWeb.Request.tempAddShiftTradeRequest = tempAddShiftTradeRequest;
	}
});