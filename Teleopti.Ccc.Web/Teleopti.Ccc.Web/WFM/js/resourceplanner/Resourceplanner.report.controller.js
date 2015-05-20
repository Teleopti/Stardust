(function () {
	'use strict';
	angular.module('wfm.resourceplanner.report', [])
		.controller('ResourceplannerReportCtrl', [
			'$scope', '$stateParams', function ($scope, $stateParams) {
				$scope.issues = $stateParams.result.BusinessRulesValidationResults;
				$scope.scheduledAgents = $stateParams.result.ScheduledAgentsCount;
				$scope.gridOptions = {
					columnDefs: [
						{ name: 'Agent', field: 'Name', enableColumnMenu: false},
						{ name: 'Detail', field: 'Message', enableColumnMenu: false },
						{ name: 'Issue-type', field: 'BusinessRuleCategory', enableColumnMenu: false }
					],
					data: $scope.issues
				};
			}
		]);
})();