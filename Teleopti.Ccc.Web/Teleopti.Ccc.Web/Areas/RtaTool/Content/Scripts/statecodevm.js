define([
	'knockout',
	'rta',
	'jquery'
], function (
	ko,
	rta,
	$
) {

	return function (data) {
		var self = this;

		self.code = ko.observable(data.code);

		self.sendState = function () {
			$.ajax({
				url: 'Rta/State/Change',
				type: 'POST',
				data: JSON.stringify({
					AuthenticationKey: data.authenticationKey(),
					UserCode: data.usercode,
					StateCode: data.code,
					StateDescription: data.code,
					IsLoggedOn: true,
					SecondsInState: 0,
					TimeStamp: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
					PlatformTypeId: '00000000-0000-0000-0000-000000000000',
					SourceId: 1,
					BatchId: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
					IsSnapshot: false
				}),
				error: function (jqXHR, textStatus, errorThrown) {
					data.error("ERROR ERROR: " + textStatus + " " + errorThrown);
				},
				contentType: "application/json"
			});
		};

	};
});

