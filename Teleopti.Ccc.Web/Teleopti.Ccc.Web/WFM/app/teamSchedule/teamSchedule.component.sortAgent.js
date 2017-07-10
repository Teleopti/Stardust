(function (angular) {
	'use strict';
	angular.module('wfm.teamSchedule').component('sortAgent', {
		controller: SortAgentCtrl,
		templateUrl: 'app/teamSchedule/html/sortAgent.html'
	});

	SortAgentCtrl.$inject = ['$scope'];
	function SortAgentCtrl($scope) {
		var ctrl = this;
		ctrl.selectedOption = "";
		ctrl.availableOptions = [];
		
		ctrl.$onInit = function () {
			ctrl.availableOptions = [
				{
					key: "FirstName",
					isSelected: false
				},
				{
					key: "LastName",
					isSelected: false
				},
				{
					key: "EmploymentNumber",
					isSelected: false
				},
				{
					key: "ShiftStart",
					isSelected: false
				},
				{
					key: "ShiftEnd",
					isSelected: false
				}
			];
		};

		ctrl.select = function (item) {
			ctrl.selectedOption = item.key;
			ctrl.availableOptions.forEach(function(item) {
				item.isSelected = false;
			});
			item.isSelected = true;
			$scope.$emit('teamSchedule.sortOption.update', { option: ctrl.selectedOption });
		}
	}
})(angular);
