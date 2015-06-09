(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerReportCtrl', [
			'$scope', '$stateParams', function ($scope, $stateParams) {
			$scope.issues = $stateParams.result.BusinessRulesValidationResults;
			$scope.hasIssues = $scope.issues.length > 0;

			$scope.scheduledAgents = $stateParams.result.ScheduledAgentsCount;
			$scope.gridOptions = {
				columnDefs: [
					{ name: 'Agent', field: 'Name', enableColumnMenu: false},
					{ name: 'Detail', field: 'Message', enableColumnMenu: false },
					{ name: 'Issue-type', field: 'BusinessRuleCategoryText', enableColumnMenu: false }
				],
				data: $scope.issues
			};
			}
		]);
})();