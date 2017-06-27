Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel = function () {
	var self = this;


	self.Template = "add-overtime-request-template";
	self.IsPostingData = ko.observable(false);

	self.Subject = ko.observable();
	self.Message = ko.observable();

	self.DateFrom = ko.observable();
	self.DateTo = ko.observable();
	self.TimeFrom = ko.observable();
	self.TimeTo = ko.observable();
	self.DateFormat = ko.observable();

	self.weekStart = ko.observable(1);
	self.ShowMeridian = ko.observable(true);
	self.ShowError = ko.observable(false);
	self.ErrorMessage = ko.observable();

	self.checkMessageLength = function() {

	};

	self.AddRequest = function() {
		var data = { "Id": "6ad7665c-113f-4dd1-9d0d-a79f0078826b", "Link": { "rel": "self", "href": "/TeleoptiWFM/Web/MyTime/Requests/RequestDetail/6ad7665c-113f-4dd1-9d0d-a79f0078826b", "Methods": "GET" }, "Subject": "Overtime Request", "Text": "I want to work all night!!!", "Type": "Phone", "TypeEnum": 1, "DateTimeFrom": "2017-06-27T00:00:00.0000000", "DateTimeTo": "2017-06-27T23:59:00.0000000", "UpdatedOnDateTime": "2017-06-27T09:18:45.6898922", "Status": "Pending", "Payload": "Overtime paid", "PayloadId": "47d9292f-ead6-40b2-ac4f-9b5e015ab330", "IsCreatedByUser": false, "IsFullDay": true, "DenyReason": "Your request has been denied due to technical issues. Please try again later. ", "From": "", "To": "", "IsNew": false, "IsPending": false, "IsApproved": false, "IsDenied": false, "IsNextDay": false, "DateFromYear": 2017, "DateFromMonth": 6, "DateFromDayOfMonth": 27, "DateToYear": 2017, "DateToMonth": 6, "DateToDayOfMonth": 27, "IsReferred": false, "ExchangeOffer": null };
		Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data, true);
	};

}