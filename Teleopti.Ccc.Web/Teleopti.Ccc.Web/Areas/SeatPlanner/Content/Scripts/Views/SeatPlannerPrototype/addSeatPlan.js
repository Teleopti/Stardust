define([
		'knockout',
		'moment',
		'navigation',
		'ajax',
		'resources',
		'guidgenerator',
		'notifications'
], function (
		ko,
		moment,
		navigation,
		ajax,
		resources,
		guidgenerator,
		notificationsViewModel
	) {

	return function () {

		var self = this;
		var businessUnitId;
		var trackId = guidgenerator.newGuid();
		this.data = {};


		this.SetData = function (data) {
			this.data =
			{
				StartDate: data.StartDate,
				EndDate: data.EndDate,
				Locations: self.getTreeNodeGuids(data.Locations),
				Teams: self.getTreeNodeGuids(data.Teams),
				TrackedCommandInfo: { TrackId: trackId }
			}

			businessUnitId = data.BusinessUnitId;
		};

		this.getTreeNodeGuids = function (treeNodes) {
			var guids = [];
			for (var i = 0; i < treeNodes.length; i++) {
				guids.push(treeNodes[i].id());
			}
			return guids;
		}

		this.Apply = function () {
			
			var requestData = JSON.stringify(self.data);
			ajax.ajax({
				url: 'SeatPlanCommand/AddSeatPlan',
				type: 'POST',
				headers: { 'X-Business-Unit-Filter': businessUnitId },
				data: requestData,
				success: function (data, textStatus, jqXHR) {
					//navigation.GoToTeamSchedule(businessUnitId, groupId, self.StartDate());
					notificationsViewModel.UpdateNotification(trackId, 6);
				},
				statusCode500: function (jqXHR, textStatus, errorThrown) {
					notificationsViewModel.UpdateNotification(trackId, 3);
				}
			});
			notificationsViewModel.AddSimpleNotification(trackId, "Adding New Seat Plan...");

		};
	};
});