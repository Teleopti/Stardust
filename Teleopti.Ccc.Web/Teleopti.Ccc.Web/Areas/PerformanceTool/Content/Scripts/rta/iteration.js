
define([
], function(
) {

	return function(data) {

		var self = this;

		this.Start = function () {
			self.StateSentCompletedPromise = $.Deferred();
			self.SendRtaExternalState();
		};

		this.SendRtaExternalState = function () {
			var externalState = {
				authenticationKey: '!#¤atAbgT%',
				userCode: data.ExternalLogOn,
				stateCode: data.StateCode,
				isLoggedOn: 'true',
				timestamp: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
				platformTypeId: data.PlatformTypeId,
				sourceId: data.SourceId,
				isSnapshot: 'false'
			};
			$.ajax({
				url: data.Url ,
				type: 'POST',
				contentType: 'application/json',
				data: JSON.stringify(externalState),
				error: function () {
					data.Failure();
				},
				complete: function () {
					self.StateSentCompletedPromise.resolve();
				}
			});
		};
	};
});
