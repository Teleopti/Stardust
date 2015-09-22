(function () {
   'use strict';

    angular.module('wfm.rta').controller('RtaAgentsCtrl', [
         '$scope', '$filter', '$state', '$stateParams', 'RtaOrganizationService', 'RtaAgentsService', 'RtaService',
          function ($scope, $filter, $state, $stateParams, RtaOrganizationService, RtaAgentsService, RtaService) {

          	var teamId = $stateParams.teamId;
          	var siteId = $stateParams.siteId;

          	$scope.agents = RtaAgentsService.getAgents($stateParams.teamId);
          	$scope.siteName = RtaOrganizationService.getSiteName(siteId);
          	$scope.teamName = RtaOrganizationService.getTeamName(teamId);

          	$scope.gridOptions = {
          		columnDefs: [
					{ name: 'Name', field: 'Name', enableColumnMenu: false },
					{ name: 'Team', field: 'Team', enableColumnMenu: false},
					{ name: 'State', field: 'State', enableColumnMenu: false },
					{ name: 'Activity', field: 'Activity', enableColumnMenu: false },
					{ name: 'Next Activity', field: 'Next Activity', enableColumnMenu: false },
					{ name: 'Next Activity Start Time', field: 'Next Activity Start Time', enableColumnMenu: false },
					{ name: 'Alarm', field: 'Alarm', enableColumnMenu: false },
					{ name: 'Time in Alarm', field: 'Time in Alarm', enableColumnMenu: false }
          		],
          		data: $scope.agents
          	};

          	$scope.goBackToRoot = function () {
          		$state.go('rta-sites');
          	};

          	$scope.goBack = function () {
				$state.go('rta-teams', { siteId: siteId });
          	};

          }]);
})();