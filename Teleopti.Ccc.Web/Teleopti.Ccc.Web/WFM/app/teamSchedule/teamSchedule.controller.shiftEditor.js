(function () {
	'use strict';

	angular.module("wfm.teamSchedule").controller("ShiftEditorController", ShiftEditorController);

	ShiftEditorController.$inject = ['$stateParams'];

	function ShiftEditorController($stateParams) {
		var vm = this;
		vm.personSchedule = angular.copy($stateParams.personSchedule);
	}

})();