(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('noteEditor', noteEditorDirective);

	noteEditorCtrl.$inject = ['$scope', 'ScheduleNoteManagementService'];

	function noteEditorCtrl($scope, ScheduleNoteMgmt) {
		var vm = this;
		vm.label = 'InternalNotes';
		vm.submit = function () {
			ScheduleNoteMgmt.setInternalNoteForPerson(vm.noteInputOption.personId, vm.internalNotes);
			vm.noteInputOption.showEditor = false;
		};
		
		vm.cancel = function () {
			vm.noteInputOption.showEditor = false;
		};

		$scope.$watch(function () {
			return vm.noteInputOption;
			},
			function (newValue) {
				if (newValue) {
					vm.internalNotes = ScheduleNoteMgmt.getInternalNoteForPerson(vm.noteInputOption.personId);
				}
			}
		);
	}

	function noteEditorDirective() {
		return {
			restrict: 'E',
			scope: {
				noteInputOption: '=?'
			},
			controller: noteEditorCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/noteEditor.html'
		}
	}
})();
