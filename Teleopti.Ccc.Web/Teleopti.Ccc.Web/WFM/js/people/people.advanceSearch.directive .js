'use strict';

(function () {
	var directive = function () {
		return {
			templateUrl: "js/people/template/advancedPeopleSearch.html"
		};
	};
	angular.module('wfm.people').directive('advanceSearch', directive);
}());