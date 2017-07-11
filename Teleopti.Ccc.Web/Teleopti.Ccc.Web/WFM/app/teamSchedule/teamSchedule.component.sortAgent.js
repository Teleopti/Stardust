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
					key: "StartTime",
					isSelected: false
				},
				{
					key: "EndTime",
					isSelected: false
				}
			];
		};

		ctrl.select = function (item) {
			ctrl.availableOptions.forEach(function (item) {
				item.isSelected = false;
			});
			if (ctrl.selectedOption === item.key) {
				ctrl.selectedOption = undefined;
			} else {
				ctrl.selectedOption = item.key;
				item.isSelected = true;
			}
			$scope.$emit('teamSchedule.sortOption.update', { option: ctrl.selectedOption });
		}
		$scope.$on("teamSchedule.init.sortOption",
			function (e, d) {
				ctrl.availableOptions.forEach(function (item) {
					if (item.key === d.option) {
						item.isSelected = true;
						ctrl.selectedOption = d.option;
					} else {
						item.isSelected = false;
					}

				});
			});
	}
})(angular);
