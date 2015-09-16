(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerReportCtrl', [
			'$scope','$state', '$stateParams', function ($scope,$state, $stateParams) {
			$scope.issues = $stateParams.result.BusinessRulesValidationResults;
			$scope.hasIssues = $scope.issues.length > 0;
			$scope.scheduledAgents = $stateParams.result.ScheduledAgentsCount;
            var scheduleResult = $stateParams.interResult._skillResultList;
            $scope.dayNodes = scheduleResult;
			$scope.gridOptions = {
				columnDefs: [
					{ name: 'Agent', field: 'Name', enableColumnMenu: false},
					{ name: 'Detail', field: 'Message', enableColumnMenu: false },
					{ name: 'Issue-type', field: 'BusinessRuleCategoryText', enableColumnMenu: false }
				],
				data: $scope.issues
			};
            $scope.getdaysuntilnextmonday = function (day) {
                if(day.getDay() === 1) return 7;
                var dayobj = moment(day);
                var nbdays = 1;
                while(dayobj.day() !== 1){
                    dayobj.day(1);
                    nbdays++ ;
                }
                return nbdays;
            };
                var CDayOff = {};
                CDayOff.a = 6;
                CDayOff.b = 0;
                CDayOff.start = 1;
                var parseWeekends = function(daysOff){
                    scheduleResult.forEach(function(node){
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
                var parseDate = function(){
                    $scope.dates = [];
                    scheduleResult[0].SkillDetails.forEach(function(subnode){
                        var day = new Date(subnode.Date.Date).getDate();
                        var month = new Date(subnode.Date.Date).getMonth();
                        var year = new Date(subnode.Date.Date).getFullYear();
                        $scope.dates.push({day:day,month:month+1,year:year})
                    })
                }();
			}
		]);
})();