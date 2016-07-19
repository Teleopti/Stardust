(function() {
	"use strict";

	angular.module('wfm.teamSchedule').service('ShiftCategoryService', shiftCategoryServiceCtrl);

	shiftCategoryServiceCtrl.$inject = ['$http', '$q'];

	function shiftCategoryServiceCtrl($http, $q) {
		var fetchShiftCategoriesUrl = "../api/TeamScheduleData/FetchShiftCategories";	
		var modifyShiftCategoriesUrl = "../api/TeamScheduleData/ModifyShiftCategories";

		this.fetchShiftCategories = fetchShiftCategories;
		this.modifyShiftCategories = modifyShiftCategories;

		function fetchShiftCategories(){
			var deferred = $q.defer();

			$http.get(fetchShiftCategoriesUrl).then(function(data){
				deferred.resolve(data);
			}, function(error){
				deferred.reject(error);
			});

			return deferred.promise;
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