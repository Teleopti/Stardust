(function() {
	'use strict';
	angular.module('wfm.reports').service('ReportsService', reportsService);
	reportsService.$inject = ['$http', '$q', 'Toggle'];

	function reportsService($http, $q, ToggleSvc) {
		var urlForGetCategorizedReports = '../api/Reports/NavigationsCategorized';
		var urlForAuditTrailChangedByPerson = '../api/Reports/PersonsWhoChangedSchedules';

		this.getCategorizedReports = getCategorizedReports;
		this.getAuditTrailChangedByPerson = getAuditTrailChangedByPerson;

		function getCategorizedReports() {
			var deferred = $q.defer();
			$http.get(urlForGetCategorizedReports).success(function(data) {

				// this splice is a temp
				for( var i=data.length-1; i>=0; i--) {
					if( data[i].Url == "reports/leaderboard"){
						data.splice(i,1);
						data.push({
							Category:"Agent Performance",
							IsWebReport:true,
							Name:"Gamification Leaderboard",
							Url:"leaderboardreport"
						})
					}
				}

				// push in web reports
				if (ToggleSvc.WFM_AuditTrail_44006) {
					data.push({
						Category: "Schedule Analysis",
						IsWebReport: true,
						Name:"Schedule audit trail",
						Url:"auditTrail"
					})
				}

				deferred.resolve(data);
			});
			return deferred.promise;
		}

		function getAuditTrailChangedByPerson() {
			var deferred = $q.defer();
			$http.get(urlForAuditTrailChangedByPerson).success(function (data) {
				console.log(data);
				deferred.resolve(data);
			});
			return deferred.promise;
		}
	}
})();
