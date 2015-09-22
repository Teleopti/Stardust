(function () {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', 'RtaService', 'RtaOrganizationService', function ($scope, $filter, $state, $stateParams, $interval, RtaService, RtaOrganizationService) {

				$scope.sites = RtaOrganizationService.getSites();

				$scope.onSiteSelect = function (site) {
					$state.go('rta-teams', {siteId: site.Id});
				};

			    var displayAdherence = function (data) { // FIXME get adherence from the server with this first call
			        data.forEach(function (site) {  
			            site.OutOfAdherence = 0;
			        });
			        $scope.sites = data;
			        RtaService.getAdherenceForAllSites.query().$promise.then(updateAdherence);

                    $interval(function () {
                    	RtaService.getAdherenceForAllSites.query().$promise.then(updateAdherence);
			        }, 5000);
			    };

			    var updateAdherence = function (data) {
			        data.forEach( function(dataSite) {
			            var filteredSite = $filter('filter')($scope.sites, { Id: dataSite.Id });
			            filteredSite[0].OutOfAdherence = dataSite.OutOfAdherence ? dataSite.OutOfAdherence : 0;
			        })
			    };

			    RtaService.getSites.query({ id: $stateParams.id }).$promise.then(displayAdherence);

        }]);
})();