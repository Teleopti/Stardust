
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

		this.SendRtaExternalState = function () {
			var externalState = JSON.stringify({
				authenticationKey: "!#¤atAbgT%",
				userCode: data.ExternalLogOn,
				stateCode: data.StateCode,
				isLoggedOn: 'true',
				timestamp: moment.utc().toDate(),
				platformTypeId: data.PlatformTypeId,
				sourceId: data.SourceId,
				isSnapshot: 'false'
			});
			$.ajax({
				url: data.Url + '/SaveExternalUserState/',
				dataType: 'json',
				type: "POST",
				contentType: "application/json",
				data: externalState,
				error: function (jqXHR, textStatus, errorThrown) {
					console.log(arguments);
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
