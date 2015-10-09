'use strict';

(function () {
	var directive = function () {
		return {
			templateUrl: "js/people/html/advancedPeopleSearch.html"
		};
	};
	angular.module('wfm.people').directive('advanceSearch', directive);
}());