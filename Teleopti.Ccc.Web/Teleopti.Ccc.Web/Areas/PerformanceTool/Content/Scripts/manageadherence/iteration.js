
define(["knockout"
], function(ko
) {

	return function(data) {

		var self = this;

		this.Start = function () {
			self.StateSentCompletedPromise = $.Deferred();
			self.SendRtaExternalState();
		};

		this.Data = data;
		this.SendRtaExternalState = function () {
			var externalState = {
				authenticationKey: '!#¤atAbgT%',
				userCode: data.Person.ExternalLogOn,
				stateCode: data.StateCode,
				isLoggedOn: 'true',
				timestamp: data.Timestamp || moment.utc().format('YYYY-MM-DD HH:mm:ss'),
				platformTypeId: data.PlatformTypeId,
				sourceId: data.SourceId,
				isSnapshot: 'false'
			};
			$.ajax({
				url: 'Rta/State/Change',
				type: 'POST',
				contentType: 'application/json',
				cache: false,
				data: JSON.stringify(externalState),
				error: function () {
				},
				complete: function () {
					self.StateSentCompletedPromise.resolve();
				}
			});
		};
	};
});
