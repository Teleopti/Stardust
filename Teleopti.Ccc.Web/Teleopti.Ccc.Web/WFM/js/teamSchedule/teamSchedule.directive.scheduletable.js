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
				selectedPersonProjections: '=',
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

		var getSelectedPersonIndex = function(personId) {
			for (var i = 0; i < vm.selectedPersonProjections.length; i++) {
				if (vm.selectedPersonProjections[i].PersonId === personId) {
					return i;
				}
			};
			return -1;
		};

		vm.ToggleProjectionSelection = function (currentProjection, personSchedule, shiftDate) {
			if (!toggleSvc.WfmTeamSchedule_RemoveAbsence_36705 && !toggleSvc.WfmTeamSchedule_RemoveActivity_37743)
				return;

			var isSameDay = moment(shiftDate).isSame(personSchedule.Date, 'day');
			if (!isSameDay || currentProjection.IsOvertime) {
				return;
			}

			currentProjection.ToggleSelection();

			var selected = currentProjection.Selected;
			var selectedPersonAbsencesLocal = [],
				selectedPersonActivitiesLocal = [];
			angular.forEach(personSchedule.Shifts, function(shift) {
				angular.forEach(shift.Projections, function (projection) {
					
					var isPersonAbsenceValid = projection.ParentPersonAbsence != undefined && currentProjection.ParentPersonAbsence != undefined;
					var isPersonActivityValid = projection.ActivityId != undefined && currentProjection.ActivityId != undefined;
					if ((isPersonAbsenceValid && projection.ParentPersonAbsence === currentProjection.ParentPersonAbsence) || (isPersonActivityValid && projection.ActivityId === currentProjection.ActivityId)) {
						projection.Selected = selected;
					}
					if (projection.Selected && projection.ParentPersonAbsence != undefined && selectedPersonAbsencesLocal.indexOf(projection.ParentPersonAbsence) === -1)
						selectedPersonAbsencesLocal.push(projection.ParentPersonAbsence);

					if (projection.Selected && projection.ActivityId != undefined && selectedPersonActivitiesLocal.indexOf(projection.ActivityId) === -1)
						selectedPersonActivitiesLocal.push(projection.ActivityId);
				});
			});

			var personId = personSchedule.PersonId;
			var personIndex = getSelectedPersonIndex(personId);
			var data;
			if (selectedPersonAbsencesLocal.length === 0 && selectedPersonActivitiesLocal.length === 0) {
					vm.selectedPersonProjections.splice(personIndex, 1);
			}
			else {
				data = {
					PersonId: personId,
					SelectedPersonAbsences: selectedPersonAbsencesLocal,
					SelectedPersonActivities: selectedPersonActivitiesLocal
				};
				if (personIndex > -1) {
					vm.selectedPersonProjections[personIndex] = data;
				} else {
					vm.selectedPersonProjections.push(data);
				}
			}
		};

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