'use strict';

(function () {
	var directive = function () {
		return {
			controller: 'ImportPeopleCtrl',
			controllerAs: 'vm',
			bindToController: true,
			require: ['importPeople', '^people'],
			templateUrl: "js/people/html/importPeople.html",
			linkFunction: linkFunction
		};
	};
	angular.module('wfm.people')
		.directive('importPeople', directive);

	function linkFunction(scope, element, attributes, controllers) {
		var vm = controllers[0];
		var parentVm = controllers[1];
		vm.parentVm = parentVm;
	};
}());