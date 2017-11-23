Teleopti.MyTimeWeb.Schedule.MobileStartDay.DataService = function(ajax) {
	var self = this;
	self.fetchData = function (date, staffingPossiblityType, successCallback) {
		ajax.Ajax({
			url: "../api/Schedule/FetchDayData",
			dataType: "json",
			type: "GET",
			data: {
				date: date,
				staffingPossiblityType: staffingPossiblityType
			},
			success: function(data) {
				successCallback && successCallback(data);
			}
		});
	};

	self.fetchProbabilityData = function(date, staffingPossiblityType, successCallback) {
		ajax.Ajax({
			url: "../api/ScheduleStaffingPossibility",
			dataType: "json",
			type: "GET",
			data: {
				date: date,
				staffingPossiblityType: staffingPossiblityType,
				returnOneWeekData: false
			},
			success: function (data) {
				successCallback && successCallback(data);
			}
		});
	};

	self.fetchMessageCount = function(callback) {
		ajax.Ajax({
			url: "../api/Schedule/GetUnreadMessageCount",
			dataType: "json",
			type: "GET",
			success: function(data) {
				callback && callback(data);
			}
		});
	};
}