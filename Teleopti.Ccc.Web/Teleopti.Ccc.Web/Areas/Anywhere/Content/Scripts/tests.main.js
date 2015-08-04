require.config(requireconfiguration);

define('resources', {
	TimeFormatForMoment: "HH:mm",
	FixedDateTimeFormatForMoment: "YYYY-MM-DD HH:mm",
	DateTimeFormatForMoment: "YYYY-MM-DD HH:mm",
	FixedTimeFormatForMoment: "HH:mm",
	TimeZoneOffsetMinutes: 0
});

define('window', {
	setLocationHash: function () { },
	locationReplace: function () { }
});

var testCases = [
	'tests',
	'views/personschedule/vm_test',
	'views/personschedule/absencelistitem_test',
	'views/teamschedule/vm_test',
	'views/realtimeadherencesites/vm_test',
	'views/realtimeadherenceteams/vm_test',
	'views/realtimeadherenceagents/vm_test',
	'views/realtimeadherenceteams/team_test',
	'views/manageadherence/vm_test',
	'helpers_test',
	'navigation_test'
];

var parseQueryString = function (queryString) {
	var params = {}, queries, temp, i, l;
	queries = queryString.split("&");
	for (i = 0, l = queries.length; i < l; i++) {
		temp = queries[i].split('=');
		params[temp[0]] = temp[1];
	}
	return params;
};

require(testCases, function () {
	var query = parseQueryString(location.search.substring(1));
	for (var i = 0, j = arguments.length; i < j; i++) {
		if (!query.q || testCases[i].indexOf(query.q) > 0)
			arguments[i]();
	}
});
