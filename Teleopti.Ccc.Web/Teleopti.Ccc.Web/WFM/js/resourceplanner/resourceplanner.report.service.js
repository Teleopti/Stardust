(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.service('ResourcePlannerReportSrvc', [
		'$resource', function ($resource) {


            this.parseWeek = function(period){
				var culturalDaysOff = {};
				culturalDaysOff.a = 6; //saturday
				culturalDaysOff.b = 0; //sunday
				culturalDaysOff.start = 1;
				period.forEach(function(node){
                    node.SkillDetails.forEach(function(subnode){
                        var day = new Date(subnode.Date).getDay();
                        if(day == culturalDaysOff.a || day == culturalDaysOff.b){
                        	return subnode.weekend = true;
                        }
                        if(day == culturalDaysOff.start){
                            return subnode.weekstart = true;
                        }
                    })
                })
			};
			this.parseRelDif = function(period){
				period.forEach(function(node){
						node.SkillDetails.forEach(function(subnode){
							var parseDif = (subnode.RelativeDifference * 100).toFixed(1)
							return subnode.parseDif = parseDif;
						})
					})
			};


		}
])
})();
