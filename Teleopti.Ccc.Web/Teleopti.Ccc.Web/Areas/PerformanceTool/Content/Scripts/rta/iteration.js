
define([
], function(
) {

	return function(data) {

		var self = this;

		this.Number = data.Number;

		this.Start = function() {
			self.AllCommandsCompletedPromise = $.Deferred();
			self.SendRtaExternalState();
		};

		this.SendRtaExternalState = function() {
			$.ajax({
				url: data.Url,
				dataType: 'json',
				contentType: 'application/json',
				data: JSON.stringify({
					authenticationKey: "!#¤atAbgT%",
					userCode: data.ExternalLogOn,
					stateCode: data.StateCode,
					isLoggedOn: "true",
					timestamp: moment.utc(),
					platformTypeId: data.PlatformTypeId,
					sourceId: data.SourceId,
					isSnapshot: "false"
				}),
				error: function() {
					data.Failure();
				},
				complete: function (jqXHR, text) {
					if (text === "success")
						data.Success();
				}
			});
		};
	};
});
