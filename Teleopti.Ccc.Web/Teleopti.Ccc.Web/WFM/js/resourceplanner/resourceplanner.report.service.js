(function () {
	'use strict';
	angular.module('restPlanningPeriodService', ['ngResource'])
		.service('ResourcePlannerReportSrvc', [
		'$resource', function ($resource) {

            this.parseWeek = function(){
                console.log('hello from the service')
                return
            }


		}
])
})();
