define(
[
	'knockout',
	'resources'
], function (
	ko,
	resources
) {
	var self = this;

	var notificationViewModel = function (trackId, message) {
		this.Succeed = false;
		this.OriginalMessage = message;
		this.TrackId = trackId;
		this.Message = ko.observable(message);
		this.Css = ko.observable("alert alert-info alert-dismissible");
	};

	this.Notifications = ko.observableArray();

	var removeNotification = function(trackId) {
		self.Notifications.remove(function(item) { return item.TrackId === trackId; });
	};

	var updateNotification = function(trackId, status) {
		var match = ko.utils.arrayFirst(self.Notifications(), function(item) {
			return trackId === item.TrackId;
		});
		if (!match || match.Succeed)
			return;
		var msg = match.OriginalMessage + " " + "Unknown status";
		var cssClazz = "alert alert-danger alert-dismissible";
		if (status === 1) {
			msg = match.OriginalMessage + " " + resources.SuccessWithExclamation;
			cssClazz = "alert alert-success alert-dismissible";
			match.Succeed = true;
			setTimeout(function() {
				removeNotification(trackId);
			}, 10000);
		}
		if (status === 2) {
			msg = match.OriginalMessage + " " + resources.FailedWithExclamation + " " + "Failed on Service bus!";
			cssClazz = "alert alert-danger alert-dismissible";
		}
		if (status === 3) {
			msg = match.OriginalMessage + " " + resources.FailedWithExclamation + " " + resources.PleaseTryAgainWithExclamation;
			cssClazz = "alert alert-danger alert-dismissible";
		}
		if (status === 4) {
			msg = match.OriginalMessage + " " + "...";
			cssClazz = "alert alert-warning alert-dismissible";
			setTimeout(function () {
				updateNotification(trackId, 5);
			}, 15000);
		}
		if (status === 5) {
			msg = match.OriginalMessage + " " + resources.FailedWithExclamation + " " + resources.PleaseRefreshThePageWithExclamation;
			cssClazz = "alert alert-danger alert-dismissible";
		}
		match.Message(msg);
		match.Css(cssClazz);
	};


	return {
		UpdateNotification: updateNotification,

		AddNotification: function (trackId, message) {
			self.Notifications.push(new notificationViewModel(trackId, message));
			setTimeout(function () {
				updateNotification(trackId, 4);
			}, 10000);
		},

		RemoveNotification: removeNotification
	};
});
