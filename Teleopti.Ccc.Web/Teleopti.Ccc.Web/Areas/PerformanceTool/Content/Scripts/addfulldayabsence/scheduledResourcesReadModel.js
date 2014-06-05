
define([
		'addfulldayabsence/scenariobase'
], function (
		scenariobase
	) {
	return function () {

		var readModelName = "ScheduledResourcesReadModel";

		var isNotificationApplicable = function (notification) {
			return true;
		};

		var text =
		"\
		A few known things that will make this fail:<br /> \
		- EnableNewResourceCalculation setting is false.<br /> \
		";

		return new scenariobase(readModelName, isNotificationApplicable, text);

	};
});