(function() {

	'use strict';

	angular.module('wfm.teamSchedule').directive('noteEditor', ['$timeout', noteEditorDirective]);

	function noteEditorDirective($timeout) {
		return {
			restrict: 'E',
			scope: {
				noteInputOption: '=?'
			},
			controller: noteEditorCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/noteEditor.html',
			compile: function(tElement, tAttrs) {
				var tabindex = angular.isDefined(tAttrs.tabindex) ? tAttrs.tabindex : '0';

				function addTabindexTo() {
					angular.forEach(arguments, function(elements) {
						angular.forEach(elements, function(element) {
							element.setAttribute('tabIndex', tabindex);
						});
					});
				}
				addTabindexTo(
					tElement[0].querySelectorAll('textarea'),
					tElement[0].querySelectorAll('#submit-note'),
					tElement[0].querySelectorAll('#cancel-note-edit')
				);

				return function(scope, elem, attrs) {
					if (scope.noteInputOption)
						scope.noteInputOption.showEditor = false;

					scope.$watch(function() {
						return scope.vm.noteInputOption;
					}, function(newValue) {
						if (newValue) {
							var focusTarget = elem[0].querySelector('textarea');
							$timeout(function() {
								angular.element(focusTarget).focus();
							}, 0);
							elem.removeAttr('tabindex');
						}
					});
				};
			}
		};
	}

	noteEditorCtrl.$inject = ['$scope', 'ScheduleNoteManagementService', 'NoticeService', 'teamsToggles'];

	function noteEditorCtrl($scope, ScheduleNoteMgmt, NoticeService, teamsToggles) {
		var vm = this;
		vm.internalNoteTab = 'InternalNote';
		vm.publicNoteTab = 'PublicNote';
		vm.WfmTeamSchedule_DisplayAndEditPublicNote_44783 = teamsToggles.all().WfmTeamSchedule_DisplayAndEditPublicNote_44783;
		vm.submit = function() {
			ScheduleNoteMgmt.submitInternalNoteForPerson(vm.noteInputOption.personId, vm.internalNotes, vm.noteInputOption.selectedDate).then(function(data) {
				vm.noteInputOption.showEditor = false;
				if (data && data.length > 0) {
					var errorMsg = data[0].ErrorMessages.join(', ');
					NoticeService.error(errorMsg, null, true);
				} else {
					ScheduleNoteMgmt.setInternalNoteForPerson(vm.noteInputOption.personId, vm.internalNotes);
				}
			});
		};

		vm.cancel = function() {
			vm.noteInputOption.showEditor = false;
		};

		$scope.$watch(function() {
				return vm.noteInputOption;
			},
			function(newValue) {
				if (newValue) {
					vm.internalNotes = ScheduleNoteMgmt.getNoteForPerson(vm.noteInputOption.personId).internalNotes;
					vm.publicNotes = ScheduleNoteMgmt.getNoteForPerson(vm.noteInputOption.personId).publicNotes;
				}
			}
		);
	};
})();