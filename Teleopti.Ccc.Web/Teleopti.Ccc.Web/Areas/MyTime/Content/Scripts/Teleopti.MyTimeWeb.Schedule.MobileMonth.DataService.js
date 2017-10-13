Teleopti.MyTimeWeb.Schedule.MobileMonth.DataService = function(ajax) {
	var self = this;
	self.fetchData = function (date, successCallback) {
		ajax.Ajax({
			url: "../api/Schedule/FetchMobileMonthData",
			dataType: "json",
			type: "GET",
			data: {
				date: date
			},
			success: function(data) {
				successCallback && successCallback(data);
			}
		});
	};
}