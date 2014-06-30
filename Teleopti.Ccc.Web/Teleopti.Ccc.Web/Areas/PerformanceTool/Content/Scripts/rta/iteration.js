
define([
], function(
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
				timestamp: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
				platformTypeId: data.PlatformTypeId,
				sourceId: data.SourceId,
				isSnapshot: 'false'
			};
			$.ajax({
				url: 'Rta/Service/SaveExternalUserState',
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

		var successed = false;

		this.IncomingActualAgentState = function (state) {
			if (successed)
				return;
			if (!data.IsEndingIteration)
				return;
			if (data.ExpectedEndingStateGroup !== state.State)
				return;
			if (data.Person.PersonId.toUpperCase() === state.PersonId.toUpperCase()) {
				successed = true;
				data.Success();
			}
		};

	};
});
