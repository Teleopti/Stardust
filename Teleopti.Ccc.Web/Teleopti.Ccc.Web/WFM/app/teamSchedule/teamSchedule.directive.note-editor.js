(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('noteEditor', noteEditorDirective);

	noteEditorCtrl.$inject = ['$scope', 'ScheduleNoteManagementService', 'NoticeService'];

	function noteEditorCtrl($scope, ScheduleNoteMgmt, NoticeService) {
		var vm = this;
		vm.label = 'InternalNotes';
		vm.submit = function () {
			var result = ScheduleNoteMgmt.setInternalNoteForPerson(vm.noteInputOption.personId, vm.internalNotes, vm.noteInputOption.selectedDate);
			vm.noteInputOption.showEditor = false;
			if (result && result.length > 0) {
				var errorMsg = result[0].ErrorMessages.join(', ');
				NoticeService.error(errorMsg, null, true);
			}
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
