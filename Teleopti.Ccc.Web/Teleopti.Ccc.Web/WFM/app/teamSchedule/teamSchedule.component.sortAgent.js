(function (angular) {
	'use strict';
	angular.module('wfm.teamSchedule').component('sortAgent', {
		controller: SortAgentCtrl,
		templateUrl: 'app/teamSchedule/html/sortAgent.html',
		bindings: {
			ngModel:'='
		}
	});

	SortAgentCtrl.$inject = ['$scope'];
	function SortAgentCtrl($scope) {
		var ctrl = this;
		ctrl.selectedOption = ctrl.ngModel;
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
			if (!!ctrl.selectedOption) {
				ctrl.availableOptions.forEach(function (item) {
					if (item.key === ctrl.selectedOption) {
						item.isSelected = true;
					} else {
						item.isSelected = false;
					}
				});
			}
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
			$scope.$emit('teamSchedule.update.sortOption', { option: ctrl.selectedOption });
		}
	}
})(angular);
