(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerReportCtrl', [
			'$scope','$filter','$state', '$stateParams','PlanningPeriodSvrc','ResourcePlannerReportSrvc','growl', function ($scope,$filter,$state, $stateParams,PlanningPeriodSvrc,growl) {
			$scope.issues = $stateParams.result.BusinessRulesValidationResults;
			$scope.hasIssues = $scope.issues.length > 0;
			$scope.scheduledAgents = $stateParams.result.ScheduledAgentsCount;
            var scheduleResult = $stateParams.interResult._skillResultList;
			$scope.planningPeriod = $stateParams.planningperiod
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
				ResourcePlannerReportSrvc.parseWeek();
                if(day.getDay() === 1) return 7;
                var dayobj = moment(day);
                var nbdays = 1;
                while(dayobj.day() !== 1){
                    dayobj.day(1);
                    nbdays++ ;
                }
                return nbdays;
            };
			var parseRelDif = function() {
				scheduleResult.forEach(function(node){
						node.SkillDetails.forEach(function(subnode){

							var parseDif = Math.floor((subnode.RelativeDifference / 1) * 100)
							subnode.parseDif = parseDif;
						})
					})
			}();
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
				if (scheduleResult.length > 0) {
					scheduleResult[0].SkillDetails.forEach(function(subnode){
						var day = new Date(subnode.Date.Date).getDate();
						var month = new Date(subnode.Date.Date).getMonth();
						var year = new Date(subnode.Date.Date).getFullYear();
						$scope.dates.push({day:day,month:month+1,year:year})
					})
				}
            }();
				$scope.publishSchedule = function(){
					if ($scope.publishedClicked === true){
						growl.warning("<i class='mdi mdi-warning'></i> Scheduled agents have already been published", {
							ttl: 5000,
							disableCountDown: true
						});
						return
					}
					$scope.publishedClicked = true;
					PlanningPeriodSvrc.publishPeriod.query({id:$scope.planningPeriod.Id}).$promise.then(function() {
						growl.success("<i class='mdi mdi-thumb-up'></i> Scheduled agents have been successfully published", {
							ttl: 5000,
							disableCountDown: true
						});
					});
				};

			}
		]);
})();
