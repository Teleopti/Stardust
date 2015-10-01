(function () {
   'use strict';

    angular.module('wfm.rta').controller('RtaAgentsCtrl', [
         '$scope', '$filter', '$state', '$stateParams', 'RtaOrganizationService', 'RtaService',
          function ($scope, $filter, $state, $stateParams, RtaOrganizationService) {

          	var teamId = $stateParams.teamId;
          	var siteId = $stateParams.siteId;

          	$scope.agents = RtaOrganizationService.getAgents($stateParams.teamId);
          	$scope.siteName = RtaOrganizationService.getSiteName(siteId);
          	$scope.teamName = RtaOrganizationService.getTeamName(teamId);

          	$scope.gridOptions = {
          		columnDefs: [
					{ name: 'Name', field: 'Name', enableColumnMenu: false },
					{ name: 'TeamName', field: 'TeamName', enableColumnMenu: false},
					{ name: 'State', field: 'State', enableColumnMenu: false },
					{ name: 'Activity', field: 'Activity', enableColumnMenu: false },
					{ name: 'Next Activity', field: 'Next Activity', enableColumnMenu: false },
					{ name: 'Next Activity Start Time', field: 'Next Activity Start Time', enableColumnMenu: false },
					{ name: 'Alarm', field: 'Alarm', enableColumnMenu: false },
					{ name: 'Time in Alarm', field: 'Time in Alarm', enableColumnMenu: false }
          		],
          		data: $scope.agents
          	};

            $scope.refreshData = function() {
              $scope.gridOptions.data = $filter('filter')( $scope.agents, $scope.filterText, undefined);
            };

          	$scope.goBackToRoot = function () {
          		$state.go('rta-sites');
          	};

          	$scope.goBack = function () {
				      $state.go('rta-teams', { siteId: siteId });
          	};
		  }]);
})();
