(function () {
	'use strict';

	angular.module('wfm.rta').controller('RtaTeamsCtrl', [
         '$scope', '$state', '$stateParams','$interval', '$filter', 'RtaOrganizationService', 'RtaService', '$location',
          function ($scope, $state, $stateParams, $interval, $filter, RtaOrganizationService, RtaService, $location) {

						//var siteId = $stateParams.siteId;
						var siteIds = $stateParams.siteIds;

          	$scope.selectedTeams = [];
          	$scope.teams = RtaOrganizationService.getTeams(siteIds);
						$scope.siteName = '';

					 	RtaOrganizationService.getSiteName(siteIds).then(function(name){
						$scope.siteName = name;
						});

          	$scope.toggleSelection = function (teamId) {
          		var index = $scope.selectedTeams.indexOf(teamId);

          		if (index > -1) {
          			$scope.selectedTeams.splice(index, 1);
          		} else {
          			$scope.selectedTeams.push(teamId);
          		}
          	};

          	$scope.openSelectedTeams = function (selectedTeams) {
          		RtaOrganizationService.getAgentsForSelectedTeams(selectedTeams);
          	};

          	// $scope.onTeamSelect = function (team) {
          	// 	$state.go('rta-agents', { siteId: siteIds, teamId: team.Id});
          	// };

          	$scope.goBack = function () {
          		$state.go('rta-sites');
          	};

              var displayAdherence = function (data) { // FIXME get adherence from the server with this first call
                  data.forEach(function (team) {
                      team.OutOfAdherence = 0;
                  });
                  $scope.teams = data;

									siteIds.forEach(function (id) {
										  RtaService.getAdherenceForTeamsOnSite.query({ siteId: id }).$promise.then(updateAdherence);
									});

                  $interval(function () {
										siteIds.forEach(function(id) {
												RtaService.getAdherenceForTeamsOnSite.query({ siteId: id }).$promise.then(updateAdherence);
										});
                  }, 5000);
              };

              var updateAdherence = function (data) {
                  data.forEach( function(dataTeam) {
                      var filteredTeam = $filter('filter')($scope.teams, { Id: dataTeam.Id });
                      filteredTeam[0].OutOfAdherence = dataTeam.OutOfAdherence ? dataTeam.OutOfAdherence : 0;
                  })
              };

              RtaService.getTeamsForSelectedSites.query({ siteIds: siteIds }).$promise.then(displayAdherence);
          }]);
})();
