(function () {
	"use strict";
	angular.module('wfm.reports').service('LeaderBoardService', LeaderBoardService);

	LeaderBoardService.$inject = ['$http','$q'];

	function LeaderBoardService($http, $q) {
		var defaultUrl = '../api/Reports/SearchLeaderboard';
		var searchByPeriodUrl = '../api/Reports/SearchLeaderboard';

		this.getLeaderBoardDefaultData = getLeaderBoardDefaultData;
		this.getLeaderBoardDataByPeriod = getLeaderBoardDataByPeriod;

		function getLeaderBoardDefaultData(keyword) {
			var deferred = $q.defer();
			$http.get(defaultUrl,
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
			$http.get(searchByPeriodUrl,
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