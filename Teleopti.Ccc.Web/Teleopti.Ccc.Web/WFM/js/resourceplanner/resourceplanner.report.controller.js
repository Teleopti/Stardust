(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerReportCtrl', [
			'$scope','$state', '$stateParams', function ($scope,$state, $stateParams) {
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
                $scope.dayBoxes = $stateParams.interResult._skillResultList;
                var CDayOff = {};
                CDayOff.a = 6;
                CDayOff.b = 0;
                CDayOff.start = 1;
                var parseWeekends = function(daysOff){
                    $stateParams.interResult._skillResultList.forEach(function(node){
                        node.SkillDetails.forEach(function(subnode){
                            var day = new Date(subnode.Date.Date).getDay();
                            if(day == daysOff.a || day == daysOff.b){
                                subnode.weekend = true;
                            }
                            if(day == daysOff.start){
                                subnode.weekstart = true;
                            }

                        })
                    })
                }(CDayOff);

			}
		]);
})();