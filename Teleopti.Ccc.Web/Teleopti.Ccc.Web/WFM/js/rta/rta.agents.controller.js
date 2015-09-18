(function () {
   'use strict';

    angular.module('wfm.rta').controller('RtaAgentsCtrl', [
         '$scope', '$state', '$stateParams', 'RtaOrganizationService', 'RtaAgentsService', 'RtaService',
          function ($scope, $state, $stateParams, RtaOrganizationService, RtaAgentsService, RtaService) {

          	var teamId = $stateParams.teamId;
          	var siteId = $stateParams.siteId;

          	if (teamId) {
          		$scope.agents = RtaAgentsService.getAgents($stateParams.teamId);
          	} else {
          		$scope.sites = RtaOrganizationService.getSites();
          	}

          	$scope.siteName = RtaOrganizationService.getSiteName(siteId);
          	$scope.teamName = RtaOrganizationService.getTeamName(teamId);

          	$scope.gridOptions = {
          		columnDefs: [
					{ name: 'Agent', field: 'Name', enableColumnMenu: false },
					{ name: 'Team', field: 'Team', enableColumnMenu: false},
					{ name: 'State', field: 'State', enableColumnMenu: false },
					{ name: 'Id', field: 'Id', enableColumnMenu: false }
          		],
          		data: $scope.agents
          	};

          	$scope.goBackToRoot = function () {
          		$state.go('rta-sites');
          	};

          	$scope.goBack = function () {
          		$state.go('rta-teams', { siteName: $scope.siteName, siteId: $stateParams.siteId });
          	};

          }]);
})();