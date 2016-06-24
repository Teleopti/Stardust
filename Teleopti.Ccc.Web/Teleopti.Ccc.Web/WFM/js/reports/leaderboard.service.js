(function () {
	"use strict";
	angular.module('wfm.reports').service('LeaderBoardService', LeaderBoardService);

	LeaderBoardService.$inject = ['$http','$q'];

	function LeaderBoardService($http, $q) {
		var url = '../api/Reports/GetLeaderBoardData';
		this.getLeaderBoardData = getLeaderBoardData;

		function getLeaderBoardData() {
			var deferred = $q.defer();
			$http.get(url).success(function (data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}
	}
})();