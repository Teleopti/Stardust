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
				SeatMapData: data.SeatMapData,
				Id: data.Id,
				ChildLocations: data.ChildLocations,
				Seats: data.Seats,
				TrackedCommandInfo: { TrackId: trackId }
			}

			businessUnitId = data.BusinessUnitId;
		};

		this.Apply = function (successCallback) {

			var requestData = JSON.stringify(self.data);
			ajax.ajax({
				url: 'SeatMapCommand/AddSeatMap',
				type: 'POST',
				headers: { 'X-Business-Unit-Filter': businessUnitId },
				data: requestData,
				success: function (data, textStatus, jqXHR) {
					notificationsViewModel.UpdateNotification(trackId, 6);
					successCallback.call();
				},
				statusCode500: function (jqXHR, textStatus, errorThrown) {
					notificationsViewModel.UpdateNotification(trackId, 3);
				}
			});
			notificationsViewModel.AddSimpleNotification(trackId, "Saving Seat Map...");

		};
	};
});