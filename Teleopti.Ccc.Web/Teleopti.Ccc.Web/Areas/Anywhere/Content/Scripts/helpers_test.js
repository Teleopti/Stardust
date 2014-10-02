define(['buster', 'helpers'], function (buster, helpers) {
	return function () {

		buster.testCase("test case for common used functions in helpers", {
			"should format time for using as a url parameter": function () {
				
				var result = helpers.timeFormatForUrl(moment("13:00","HH:mm"));
				assert.equals(result, "1300");
			},
			"should format date for using as a url parameter" : function() {
				var result = helpers.dateFormatForUrl(moment("2014-01-01"));
				assert.equals(result, "20140101");
			}
		});
	};
});