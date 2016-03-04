Teleopti.MyTimeWeb.Request.AbsenceRequestDetailViewModel = function (requestViewModel, ajax) {
	var self = this;
	self.requestViewModel = requestViewModel;
	self.requestViewModel.WaitlistPosition = ko.observable();

	if (requestViewModel != null) {
		var id = requestViewModel.EntityId();
		if (id != null) {
			ajax.Ajax({
				url: "Requests/AbsenceRequestDetail",
				dataType: "json",
				type: "GET",
				data: {
					Id: id
				},
				success: function(absenceRequestDetailViewModel) {
					self.requestViewModel.WaitlistPosition(absenceRequestDetailViewModel.WaitlistPosition);
				}
			});
		}
	}
}
