(function () {
	"use strict";
	angular.module('wfm.reports').service('LeaderBoardService', LeaderBoardService);

	LeaderBoardService.$inject = ['$http','$q'];

	function LeaderBoardService($http, $q) {
		var url = '../api/Reports/SearchLeaderboard';

		this.getLeaderBoardDefaultData = getLeaderBoardDefaultData;
		this.getLeaderBoardDataByPeriod = getLeaderBoardDataByPeriod;

		function getLeaderBoardDefaultData(keyword) {
			var deferred = $q.defer();
			$http.get(url,
				{
					params: {
						keyword: keyword
					}
				}).success(function (data) {
					deferred.resolve(data);
				});
			return deferred.promise;
		}

		function getLeaderBoardDataByPeriod(keyword, selectedPeriod) {
			var deferred = $q.defer();
			$http.get(url,
				{
					params: {
						keyword: keyword,
						startDate: selectedPeriod.startDate,
						endDate: selectedPeriod.endDate
					}
				}).success(function (data) {
					deferred.resolve(data);
				});
			return deferred.promise;
		}
	}
})();
