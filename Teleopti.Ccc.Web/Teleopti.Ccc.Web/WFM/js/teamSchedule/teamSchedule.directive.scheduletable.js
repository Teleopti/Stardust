'use strict';

(function () {
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('scheduleTableCtrl', ['Toggle', 'PersonSelection', '$scope','ScheduleManagement', scheduleTableController]);

	function scheduleTableDirective() {
		return {
			scope: {
				selectMode: '='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'scheduleTableCtrl',
			templateUrl: "js/teamSchedule/html/scheduletable.html",
		};
	};
	function scheduleTableController(toggleSvc, personSelectionSvc, $scope, ScheduleMgmt) {
		var vm = this;
		vm.updateAllSelectionInCurrentPage = function (isAllSelected) {
			vm.scheduleVm.Schedules.forEach(function (personSchedule) {
				personSchedule.IsSelected = isAllSelected;
				$scope.$evalAsync(function () {
					vm.updatePersonSelection(personSchedule);
				});
			});

		};
		vm.init = init;

		$scope.$watch(function () {
			return ScheduleMgmt.groupScheduleVm.Schedules
		}, function (newVal) {
			if (newVal)
				vm.init();
		});

		$scope.$watch(function(){
			return isAllInCurrentPageSelected();
		}, function(newVal){
			vm.toggleAllInCurrentPage = newVal;
		});

		vm.totalSelectedProjections = function() {
			return personSelectionSvc.getTotalSelectedPersonAndProjectionCount().checkedPersonCount +
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().selectedActivityInfo.ActivityCount +
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().selectedAbsenceInfo.AbsenceCount;
		};

		vm.updatePersonSelection = function (personSchedule) {
			personSelectionSvc.updatePersonSelection(personSchedule);
			personSelectionSvc.toggleAllPersonProjections(personSchedule);
		};

		vm.ToggleProjectionSelection = function (currentProjection, personSchedule, shiftDateMoment) {
			if (!toggleSvc.WfmTeamSchedule_RemoveAbsence_36705 && !toggleSvc.WfmTeamSchedule_RemoveActivity_37743)
				return;

			var isSameDay = shiftDateMoment.isSame(personSchedule.Date, 'day');
			if (!isSameDay || currentProjection.IsOvertime) {
				return;
			}

			currentProjection.ToggleSelection();

			personSelectionSvc.updatePersonProjectionSelection(currentProjection, personSchedule);
		};

		function isAllInCurrentPageSelected() {
			var isAllSelected = true;
			var selectedPeople = personSelectionSvc.personInfo;
			if (!vm.scheduleVm || !vm.scheduleVm.Schedules) {
				return false;
			}
			for (var i = 0; i < vm.scheduleVm.Schedules.length; i++) {
				var personSchedule = vm.scheduleVm.Schedules[i];
				if (!selectedPeople[personSchedule.PersonId]) {
					isAllSelected = false;
					break;
				}
			}

			return isAllSelected;
		}

		function init() {
			vm.toggleAllInCurrentPage = isAllInCurrentPageSelected();
			vm.scheduleVm = ScheduleMgmt.groupScheduleVm;
		}
	};
} ());