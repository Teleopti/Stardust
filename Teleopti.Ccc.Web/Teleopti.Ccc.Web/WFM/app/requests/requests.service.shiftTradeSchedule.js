(function () {
	"use strict";

	angular.module("wfm.requests")
		.service("shiftTradeScheduleService", ["$q", '$http', ShiftTradeScheduleService]);

	function ShiftTradeScheduleService($q, $http) {

		var service = this;

		service.getSchedules = function (date, personFromId, personToId) {
			var deferred = $q.defer();
			$http.post("../api/Requests/shiftTradeRequestSchedules", {
				PersonFromId: personFromId,
				PersonToId: personToId,
				RequestDate: date
			}).success(function (data) {
				deferred.resolve(data);
			}).error(function (e) {
				deferred.reject(e);
			});
			return deferred.promise;
		}

	}
})();