(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("ShiftEditorService", ShiftEditorService);

	ShiftEditorService.$inject = ["$http", "$q"];

	function ShiftEditorService($http, $q) {
		var self = this;
		var changeActivityTypeUrl = '../api/TeamScheduleCommand/ChangeActivityType';

		self.changeActivityType = changeActivityType;

		function changeActivityType(date, personId, layers) {
			var deferred = $q.defer();
			var inputData = {
				Date: date,
				PersonId: personId,
				Layers: layers
			}
			$http.post(changeActivityTypeUrl, inputData).then(function (response) {
				deferred.resolve(response.data);
			}, function (error) {
				deferred.reject(error);
			});
			return deferred.promise;
		}
	}
})();