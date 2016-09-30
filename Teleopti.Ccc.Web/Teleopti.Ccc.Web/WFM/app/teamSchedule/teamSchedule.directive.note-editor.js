(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('noteEditor', noteEditorDirective);

	noteEditorCtrl.$inject = ['$scope', 'ScheduleNoteManagementService', 'NoticeService'];

	function noteEditorCtrl($scope, ScheduleNoteMgmt, NoticeService) {
		var vm = this;
		vm.label = 'InternalNote';
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
			templateUrl: 'app/teamSchedule/html/noteEditor.html',
			compile: function (tElement, tAttrs) {
				var tabindex = angular.isDefined(tAttrs.tabindex) ? tAttrs.tabindex : '0';
				function addTabindexTo() {
					angular.forEach(arguments, function (elements) {
						angular.forEach(elements, function (element) {
							element.setAttribute('tabIndex', tabindex);
						});
					});
				}
				addTabindexTo(
					tElement[0].querySelectorAll('textarea'),
					tElement[0].querySelectorAll('#submit-note'),
					tElement[0].querySelectorAll('#cancel-note-edit')
				);
				return linkFn;
			}
		}
	}

	function linkFn(scope, elem, attrs){
		scope.$watch(function () {
			return scope.vm.noteInputOption;
		},function(newValue){
			if(newValue){
				var focusTarget = elem[0].querySelector('textarea');
				setTimeout(function(){
					angular.element(focusTarget).focus();
				}, 0);
				elem.removeAttr('tabindex');
			}
		});
	}
})();
