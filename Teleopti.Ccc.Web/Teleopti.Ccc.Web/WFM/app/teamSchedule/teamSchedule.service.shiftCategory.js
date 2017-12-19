(function() {
	"use strict";

	angular.module('wfm.teamSchedule').service('ShiftCategoryService', shiftCategoryServiceCtrl);

	shiftCategoryServiceCtrl.$inject = ['$http', '$q'];

	function shiftCategoryServiceCtrl($http, $q) {
		var fetchShiftCategoriesUrl = "../api/TeamScheduleData/FetchShiftCategories";	
		var modifyShiftCategoriesUrl = "../api/TeamScheduleCommand/ChangeShiftCategory";

		this.fetchShiftCategories = fetchShiftCategories;
		this.modifyShiftCategories = modifyShiftCategories;

		var shiftCategories = null;
		function fetchShiftCategories() {
			return $q(function (resolve, reject) {
				if (!!shiftCategories) {
					resolve(shiftCategories);
					return;
				}
				$http.get(fetchShiftCategoriesUrl).then
					(function (data) {
						shiftCategories = data;
						resolve(shiftCategories);
					},
					function (err) {
						reject(err);
					});
			});
		}

		function modifyShiftCategories(requestData){
			var deferred = $q.defer();

			$http.post(modifyShiftCategoriesUrl, requestData).then(function(data){
				deferred.resolve(data);
			}, function(error){
				deferred.reject(error);
			});

			return deferred.promise;
		}
	}

})();