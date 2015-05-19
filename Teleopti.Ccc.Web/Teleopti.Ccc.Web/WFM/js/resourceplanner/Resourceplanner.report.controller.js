(function () {
	'use strict';
	angular.module('wfm.resourceplanner.report', [])
		.controller('ResourceplannerReportCtrl', [
			'$scope', '$stateParams', function ($scope, $stateParams) {
				$scope.issues = $stateParams;
				$scope.issuePreview = $scope.issues.result.BusinessRulesValidationResults;
				$scope.scheduledAgents = $stateParams.result.ScheduledAgentsCount;

				$scope.gridOptions = {
					selectionRowHeaderWidth: 35,
					columnDefs: [
						{ name: 'Agent', field: 'Name' },
						{ name: 'Detail', field: 'Message' },
						{ name: 'Issue-type', field: 'BusinessRuleCategory' }
					],

					data: $scope.issues.result.BusinessRulesValidationResults
				};

			}
		]);
})();