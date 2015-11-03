(function() {﻿
	'use strict';﻿﻿

	angular﻿
		.module('wfm.rta')﻿
		.controller('RtaAgentsForTeamsCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', '$sessionStorage', 'RtaService', 'RtaGridService', 'RtaFormatService', 'RtaRouteService','FakeTimeService',
			function ($scope, $filter, $state, $stateParams, $interval, $sessionStorage, RtaService, RtaGridService, RtaFormatService, RtaRouteService, FakeTimeService) {

				var teamIds = $stateParams.teamIds;
				var propertiesForFiltering = ["Name", "TeamName", "State", "Activity", "NextActivity", "Alarm"];
				$scope.agents = [];
				$scope.gridOptions = RtaGridService.createAgentsGridOptions();

				$scope.goBackToRoot = function() {
					RtaRouteService.goToSites();
				};
				$scope.format = function(time) {
					return RtaFormatService.formatDateTime(time);
				};
				$scope.formatDuration = function(duration) {
					return RtaFormatService.formatDuration(duration);
				};
				$scope.hexToRgb = function(hex) {
					return RtaFormatService.formatHexToRgb(hex);
				};
				$scope.filterData = function() {
					$scope.gridOptions.data = $filter('agentFilter')($scope.agents, $scope.filterText, propertiesForFiltering);
				};
				$scope.changeScheduleUrl = function(teamId, personId) {
					return RtaRouteService.urlForChangingSchedule($sessionStorage.buid, teamId, personId);
				};

				var setStatesInAgents = function(states) {
					$scope.agents.forEach(function(agent) {
						var state = $filter('filter')(states, {
							PersonId: agent.PersonId
						});
						if (state.length > 0) {
							agent.State = state[0].State;
							agent.StateStart = state[0].StateStart;
							agent.Activity = state[0].Activity;
							agent.NextActivity = state[0].NextActivity;
							agent.NextActivityStartTime = state[0].NextActivityStartTime;
							agent.Alarm = state[0].Alarm;
							agent.AlarmStart = state[0].AlarmStart;
							agent.AlarmColor = state[0].AlarmColor;
							agent.TimeInState = state[0].TimeInState;
						}
					});
				};

				var updateStatesForTeams = function() {
					RtaService.getStatesForTeams.query({
						teamIds: teamIds
					}).$promise.then(function(states) {
						setStatesInAgents(states);
					})
				};

				var updateGrid = function() {
					if ($scope.filterText === undefined)
						$scope.gridOptions.data = $scope.agents;
					else
						$scope.filterData();
				};

				RtaService.getAgentsForTeams.query({
						teamIds: teamIds
					}).$promise
					.then(function(agents) {
						$scope.agents = agents;
						$scope.multipleTeamsName = "Multiple Teams";
					}).then(updateStatesForTeams)
					.then(updateGrid);

				var polling = $interval(function() {
					updateStatesForTeams();
					updateGrid();
				}, 5000);

				$scope.$on('$destroy', function(){
					$interval.cancel(polling);
				});

				$scope.$watch(
					function() {
						return $sessionStorage.buid;
					},
					function(newValue, oldValue) {
						if (oldValue !== undefined && newValue !== oldValue) {
							$scope.goBackToRoot();
						}
					}
				);
			}
		]);﻿
})();﻿
