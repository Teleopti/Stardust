Teleopti.MyTimeWeb.OvertimeRequestsLicense = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var getLicenseAvailabilityPromise = ajax.Ajax({
		url: 'OvertimeRequests/GetLicenseAvailability',
		dataType: "json",
		type: 'GET'
	});

	return {
		GetLicenseAvailability: function (action) {
			getLicenseAvailabilityPromise.done(function (data) {
				action(data);
			});
			return getLicenseAvailabilityPromise;
		}
	};
})(jQuery);
