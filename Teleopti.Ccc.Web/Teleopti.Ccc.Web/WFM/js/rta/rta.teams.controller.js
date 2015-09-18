(function () {
	'use strict';

	angular.module('wfm.rta').controller('RtaTeamsCtrl', [
         '$scope', '$state', '$stateParams','$interval', '$filter', 'RtaOrganizationService', 'RtaService',
          function ($scope, $state, $stateParams, $interval, $filter, RtaOrganizationService, RtaService) {

          	var siteId = $stateParams.siteId;
          	
          	if (siteId) {
          		$scope.teams = RtaOrganizationService.getTeams($stateParams.siteId);
          	} else {
          		$scope.sites = RtaOrganizationService.getSites();
          	}
         
          	$scope.siteName = RtaOrganizationService.getSiteName(siteId);

          	$scope.onTeamSelect = function (team) {
          		$state.go('rta-agents', { siteName: $scope.siteName, siteId: siteId, teamName: team.teamName, teamId: team.teamId});
          	};

          	$scope.goBack = function () {
          		$state.go('rta-sites');
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
          		data.forEach(function (dataSite) {
          			var filteredSite = $filter('filter')($scope.sites, { Id: dataSite.Id });
          			filteredSite[0].OutOfAdherence = dataSite.OutOfAdherence ? dataSite.OutOfAdherence : 0;
          		})
          	};

          	RtaService.getSites.query({ id: $stateParams.id }).$promise.then(displayAdherence);

          }]);
})();