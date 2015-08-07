(function () {
	'use strict';
	var start = angular.module('wfm.start', []);

	start.controller("FeedCtrl", [
	'$scope', 'FeedService', function ($scope, Feed) {
		$scope.feedSrc = 'http://blog.teleopti.com/feed/';
			$scope.loadFeed = function(e) {
				Feed.parseFeed($scope.feedSrc).then(function(res) {
					$scope.feeds = res.data.responseData.feed.entries;

				});
			};
		}
	]);

	start.factory('FeedService', [
		'$http', function ($http) {
			return {
				parseFeed: function(url) {
					return $http.jsonp('//ajax.googleapis.com/ajax/services/feed/load?v=1.0&num=50&callback=JSON_CALLBACK&q=' + encodeURIComponent(url));
				}
			};
		}
	]);

})();