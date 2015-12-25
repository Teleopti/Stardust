'use strict';

(function () {
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('scheduleTableCtrl', scheduleTableController);


	function scheduleTableDirective() {

		return {
			scope: {
				scheduleVm: '=',
				personSelection: '=',
				selectMode: '='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'scheduleTableCtrl',
			templateUrl: "js/teamSchedule/html/scheduletable.html"
		};
	};

	function scheduleTableController() {
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