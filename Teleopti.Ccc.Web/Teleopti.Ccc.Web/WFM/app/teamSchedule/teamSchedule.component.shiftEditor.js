(function () {
	'use strict';

	angular.module("wfm.teamSchedule").component("shiftEditor", {
		controller: ShiftEditorController,
		controllerAs: 'vm',
		templateUrl: 'app/teamSchedule/html/shiftEditor.html',
		bindings: {
			personSchedule: '<'
		}
	});

	ShiftEditorController.$inject = [];

	function ShiftEditorController() {
		var vm = this;
	}

})();