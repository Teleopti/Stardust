'use strict';

(function () {
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('scheduleTableCtrl', ['Toggle', scheduleTableController]);

	function scheduleTableDirective() {
		return {
			scope: {
				scheduleVm: '=',
				personSelection: '=',
				selectedPersonAbsences: '=',
				selectMode: '='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'scheduleTableCtrl',
			templateUrl: "js/teamSchedule/html/scheduletable.html"
		};
	};

	function scheduleTableController(toggleSvc) {
		var vm = this;

		vm.toggleAllSelectionInCurrentPage = function () {
			var isAllSelected = vm.isAllInCurrentPageSelected();

			vm.scheduleVm.Schedules.forEach(function (personSchedule) {
				vm.personSelection[personSchedule.PersonId].isSelected = !isAllSelected;
			});
		};

		vm.updatePersonIdSelection = function (person) {
			vm.personSelection[person.PersonId].isSelected = !vm.personSelection[person.PersonId].isSelected;
		};

		var getSelectedPersonAbsences = function(personId) {
			for(var i = 0; i < vm.selectedPersonAbsences.length; i++) {
				var personAndAbsencePair = vm.selectedPersonAbsences[i];
				if (personAndAbsencePair.PersonId === personId) {
					return i;
				}
			};
			return -1;
		}

		vm.ToggleProjectionSelection = function (currentProjection, personSchedule) {
			if (!toggleSvc.WfmTeamSchedule_RemoveAbsence_36705
				|| currentProjection.ParentPersonAbsence == undefined || currentProjection.ParentPersonAbsence == null) {
				return;
			}

			currentProjection.ToggleSelection();
			var selected = currentProjection.Selected;
			var allSelectedProjections = [];
			angular.forEach(personSchedule.Shifts, function (shift) {
				angular.forEach(shift.Projections, function(projection) {
					if (projection.ParentPersonAbsence === currentProjection.ParentPersonAbsence) {
						projection.Selected = selected;
					}
					if (projection.Selected && allSelectedProjections.indexOf(projection.ParentPersonAbsence) === -1) {
						allSelectedProjections.push(projection.ParentPersonAbsence);
					}
				});
			});

			var personId = personSchedule.PersonId;
			var personAbsenceIndex = getSelectedPersonAbsences(personId);
			if (allSelectedProjections.length === 0) {
				if (personAbsenceIndex > -1) {
					vm.selectedPersonAbsences.splice(personAbsenceIndex, 1);
				}
			} else {
				var data = {
					PersonId: personId,
					SelectedPersonAbsences: allSelectedProjections
				};
				if (personAbsenceIndex > -1) {
					vm.selectedPersonAbsences[personAbsenceIndex] = data;
				} else {
					vm.selectedPersonAbsences.push(data);
				}
			}
		}

		vm.isAllInCurrentPageSelected = function() {
			return vm.scheduleVm.Schedules.every(function(personSchedule) {
				return vm.personSelection[personSchedule.PersonId]&&vm.personSelection[personSchedule.PersonId].isSelected;
			});
		};

		vm.isPersonSelected = function (personSchedule) {
			return vm.personSelection[personSchedule.PersonId] && vm.personSelection[personSchedule.PersonId].isSelected;
		}
	};
}());