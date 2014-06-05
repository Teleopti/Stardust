
define([
		'addfulldayabsence/scenariobase'
], function (
		scenariobase
	) {
	return function () {

		var readModelName = "PersonScheduleDayReadModel";

		var isNotificationApplicable = function(notification) {
			if (this.PersonId.toUpperCase() == notification.DomainReferenceId.toUpperCase()) {
				return true;
			}
			return false;
		};

		var text = 
		"\
		A few known things that will make this fail:<br /> \
		- Broker and backplane configuration is not correctly set up.<br /> \
		- Service bus is not configured correctly or not running.<br /> \
		- Persons that does not have a Person period for every date in the date range.<br /> \
		";

		return new scenariobase(readModelName, isNotificationApplicable, text);

	};
});