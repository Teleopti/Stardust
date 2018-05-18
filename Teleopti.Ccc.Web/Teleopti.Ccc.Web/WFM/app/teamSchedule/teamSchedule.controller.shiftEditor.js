(function () {
	'use strict';

	angular.module("wfm.teamSchedule").controller("ShiftEditorController", ShiftEditorController);

	ShiftEditorController.$inject = ['$stateParams', 'serviceDateFormatHelper'];

	function ShiftEditorController($stateParams, serviceDateFormatHelper) {
		var vm = this;
		vm.personSchedule = $stateParams.personSchedule;
		vm.date = serviceDateFormatHelper.getDateOnly($stateParams.date);
	}

})();