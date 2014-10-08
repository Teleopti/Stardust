define(['buster', 'navigation'], function (buster, navigation) {
	return function () {

		buster.testCase("test case for navigation urls", {
			"should get correct home url": function () {
				var homeUrl = navigation.UrlForHome("guid");
				assert.equals(homeUrl, window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port: '') + window.location.pathname+ "#teamschedule/guid");
			},
		});
	};
});