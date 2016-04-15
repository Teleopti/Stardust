'use strict';

(function () {
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('scheduleTableCtrl', ['Toggle','PersonSelection','$scope', scheduleTableController]);

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

	function scheduleTableController(toggleSvc, personSelectionSvc, $scope) {
		var vm = this;
		vm.updateAllSelectionInCurrentPage = function (isAllSelected) {
			vm.scheduleVm.Schedules.forEach(function (personSchedule) {
				personSchedule.IsSelected = isAllSelected;
				$scope.$evalAsync(function(){
					vm.updatePersonSelection(personSchedule);
				});
			});
			
		};
		
		vm.updatePersonSelection = function (personSchedule) {
			personSelectionSvc.updatePersonSelection(personSchedule);
			personSelectionSvc.toggleAllPersonProjections(personSchedule);
			vm.toggleAllInCurrentPage = isAllInCurrentPageSelected();
		};

		vm.ToggleProjectionSelection = function (currentProjection, personSchedule, shiftDate) {
			if (!toggleSvc.WfmTeamSchedule_RemoveAbsence_36705 && !toggleSvc.WfmTeamSchedule_RemoveActivity_37743)
				return;

			var isSameDay = moment(shiftDate).isSame(personSchedule.Date, 'day');
			if (!isSameDay || currentProjection.IsOvertime) {
				return;
			}

			currentProjection.ToggleSelection();

			personSelectionSvc.updatePersonProjectionSelection(currentProjection, personSchedule);
		};
		
		function isAllInCurrentPageSelected(){
			var isAllSelected = true;
			var selectedPeople = personSelectionSvc.personInfo;
			for(var i = 0; i < vm.scheduleVm.Schedules.length; i++){
				var personSchedule = vm.scheduleVm.Schedules[i];
				if(!selectedPeople[personSchedule.PersonId]){
					isAllSelected = false;
					break;
				}
			}

			return isAllSelected;
		}
	};
}());