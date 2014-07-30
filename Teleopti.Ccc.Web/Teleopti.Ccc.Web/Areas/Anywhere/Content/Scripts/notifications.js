define(
[
	'knockout'
], function (
	ko
) {
	var self = this;

	var notificationViewModel = function (trackId, message) {
		this.TrackId = trackId;
		this.Message = ko.observable(message);
		this.Css = ko.observable("alert alert-info");
	};

	this.Notifications = ko.observableArray();


	return {
		AddNotification: function (trackId, message) {
			self.Notifications.push(new notificationViewModel(trackId, message));
		},

		RemoveNotification: function (trackId) {
			self.Notifications.remove(function (item) { return item.TrackId === trackId; });
		},

		UpdateNotification: function (trackId, status) {
			var match = ko.utils.arrayFirst(self.Notifications(), function (item) {
				return trackId === item.TrackId;
			});
			if (!match)
				return;
			match.Message(match.Message() + " " + (status === 0 ? "Success" : "Failed"));
			match.Css(status === 0 ? "alert alert-success" : "alert alert-danger");
		}
	};
});
