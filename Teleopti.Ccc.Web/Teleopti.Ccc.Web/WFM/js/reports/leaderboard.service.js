(function () {
	"use strict";
	angular.module('wfm.reports').service('LeaderBoardService', LeaderBoardService);

	LeaderBoardService.$inject = ['$http','$q'];

	function LeaderBoardService($http, $q) {
		var url = '../api/Reports/SearchLeaderboard';
		this.getLeaderBoardData = getLeaderBoardData;

		function getLeaderBoardData(keyword) {
			var deferred = $q.defer();
			$http.get(url,
				{
					params: {
						keyword: keyword
					}
				}
				).success(function (data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}
	}
})();