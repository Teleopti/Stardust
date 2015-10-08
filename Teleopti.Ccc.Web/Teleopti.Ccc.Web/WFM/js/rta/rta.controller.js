(function () {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', 'RtaService', 'RtaOrganizationService', function ($scope, $filter, $state, $stateParams, $interval, RtaService, RtaOrganizationService) {

        $scope.selectedSites = [];
				$scope.sites = RtaOrganizationService.getSites();

				//$scope.onSiteSelect = function (site) {
				//	$state.go('rta-teams', {siteId: site.Id});
				//};

        $scope.toggleSelection = function (siteId) {
          var index = $scope.selectedSites.indexOf(siteId);

          if(index > -1){
            $scope.selectedSites.splice(index, 1);
          }else{
            $scope.selectedSites.push(siteId);
               }
          };

          $scope.openSelectedSites = function (selectedSites){
            //RtaOrganizationService.getTeamsForSelectedSites(selectedSites);
						console.log(selectedSites);
						RtaOrganizationService.getTeams(selectedSites);
						$state.go('rta-teams', {siteIds: $scope.selectedSites});
          };

					$scope.openSelectedSite = function (siteId){
						$scope.toggleSelection(siteId);
						RtaOrganizationService.getTeams($scope.selectedSites);
							console.log($scope.selectedSites);
						$state.go('rta-teams', {siteIds: $scope.selectedSites});
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
