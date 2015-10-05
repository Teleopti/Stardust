﻿(function () {
	'use strict';

	angular.module('wfm.rta').controller('RtaTeamsCtrl', [
         '$scope', '$state', '$stateParams','$interval', '$filter', 'RtaOrganizationService', 'RtaService', '$location',
          function ($scope, $state, $stateParams, $interval, $filter, RtaOrganizationService, RtaService, $location) {

						var siteId = $stateParams.siteId;

          	$scope.selectedTeams = [];
          	$scope.teams = RtaOrganizationService.getTeams(siteId);
						$scope.siteName = '';
           	RtaOrganizationService.getSiteName(siteId).then(function(name){
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

          	$scope.onTeamSelect = function (team) {
          		$state.go('rta-agents', { siteId: siteId, teamId: team.Id});
          	};

          	$scope.goBack = function () {
          		$state.go('rta-sites');
          	};

              var displayAdherence = function (data) { // FIXME get adherence from the server with this first call
                  data.forEach(function (team) {
                      team.OutOfAdherence = 0;
                  });
                  $scope.teams = data;
                  RtaService.getAdherenceForTeamsOnSite.query({ siteId: siteId }).$promise.then(updateAdherence);

                  $interval(function () {
                      RtaService.getAdherenceForTeamsOnSite.query({ siteId: siteId }).$promise.then(updateAdherence);
                  }, 5000);
              };

              var updateAdherence = function (data) {
                  data.forEach( function(dataTeam) {
                      var filteredTeam = $filter('filter')($scope.teams, { Id: dataTeam.Id });
                      filteredTeam[0].OutOfAdherence = dataTeam.OutOfAdherence ? dataTeam.OutOfAdherence : 0;
                  })
              };

              RtaService.getTeams.query({ siteId: siteId }).$promise.then(displayAdherence);
          }]);
})();
