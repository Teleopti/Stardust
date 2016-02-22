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

		vm.ToggleProjectionSelection = function (projection, allProjections) {
			if (!toggleSvc.WfmTeamSchedule_RemovePlannedAbsence_36705
				|| projection.ParentPersonAbsence == undefined || projection.ParentPersonAbsence == null) {
				return;
			}

			projection.ToggleSelection();

			var personAbsenceIndex = vm.selectedPersonAbsences.indexOf(projection.ParentPersonAbsence);
			var personAbsenceInSelectionList = personAbsenceIndex > -1;
			if (projection.Selected && !personAbsenceInSelectionList) {
				vm.selectedPersonAbsences.push(projection.ParentPersonAbsence);
			} else if (!projection.Selected && personAbsenceInSelectionList) {
				vm.selectedPersonAbsences.splice(personAbsenceIndex, 1);
			}

			allProjections.forEach(function (otherProjection) {
				if (otherProjection.ParentPersonAbsence === projection.ParentPersonAbsence) {
					otherProjection.Selected = projection.Selected;
				}
			});
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