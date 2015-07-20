'use strict';

(function () {
	var directive = function () {
		return {
			templateUrl: "js/people/template/importPeople.html"
		};
	};
	angular.module('wfm.people').directive('importPeople', directive);
}());