(function () {
	'use strict';
	angular.module('wfm.rta', [])
		.controller('RtaCtrl', [
			'$scope', '$filter', '$interval', 'RtaSvrc', function ($scope, $filter, $interval, RtaSvrc) {

			    var displayAdherence = function (data) { // FIXME get adherence from the server with this first call
			        data.forEach(function (site) {  
			            site.OutOfAdherence = 0;
			        });

			        $scope.sites = data;
                    RtaSvrc.getAdherenceForAllSites.query().$promise.then(updateAdherence);

                    $interval(function () {
			            RtaSvrc.getAdherenceForAllSites.query().$promise.then(updateAdherence);
			        }, 5000);
                
			    }; 
			    var updateAdherence = function (data) {
			        data.forEach( function(dataSite) {
			            var filteredSite = $filter('filter')($scope.sites, { Id: dataSite.Id });
			            filteredSite[0].OutOfAdherence = dataSite.OutOfAdherence ? dataSite.OutOfAdherence : 0;
			        })
			    };
			    RtaSvrc.getSites.query().$promise.then(displayAdherence);
        }  
		]);
})();