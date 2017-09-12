(function() {
	'use strict';
	angular.module('wfm.reports').service('ReportsService', reportsService);
	reportsService.$inject = ['$http', '$q', 'Toggle'];

	function reportsService($http, $q, ToggleSvc) {
		var urlForGetCategorizedReports = '../api/Reports/NavigationsCategorized';
		this.getCategorizedReports = getCategorizedReports;

		function getCategorizedReports() {
			var deferred = $q.defer();
			$http.get(urlForGetCategorizedReports).success(function(data) {

				// this splice is a temp
				for( var i=data.length-1; i>=0; i--) {
				    if( data[i].Url == "reports/leaderboard")
						data.splice(i,1);
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
				data.push({
					Category:"Agent Performance",
					IsWebReport:true,
					Name:"Gamification Leaderboard",
					Url:"leaderboardreport"
				})

				deferred.resolve(data);
			});
			return deferred.promise;
		}

	}
})();
