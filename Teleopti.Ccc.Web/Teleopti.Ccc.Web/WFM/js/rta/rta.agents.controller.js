(function() {
	'use strict';

	angular.module('wfm.rta').controller('RtaAgentsCtrl', [
		'$scope', '$filter', '$state', '$stateParams', '$interval', 'RtaOrganizationService', 'RtaService',
		function($scope, $filter, $state, $stateParams, $interval, RtaOrganizationService, RtaService) {

			var siteId = $stateParams.siteId;
			var teamId = $stateParams.teamId;
			var siteIds = $stateParams.siteIds;
			var teamIds = $stateParams.teamIds;

			var setStatesInAgents = function(states) {
				$scope.states = states;
				$scope.agents.forEach(function(agent) {
					var state = $filter('filter')(states, {
						PersonId: agent.PersonId
					});
					agent.State = state[0].State;
					agent.StateStart = state[0].StateStart;
					agent.Activity = state[0].Activity;
					agent.NextActivity = state[0].NextActivity;
					agent.NextActivityStartTime = state[0].NextActivityStartTime;
					agent.Alarm = state[0].Alarm;
					agent.AlarmStart = state[0].AlarmStart;
					agent.AlarmColor = state[0].AlarmColor;
					agent.TimeInState = state[0].TimeInState;
				});
			};

			var updateStates = function() {
				RtaService.getStates.query({
						teamId: teamId
					}).$promise
					.then(function(states) {
						setStatesInAgents(states);
					});
			};

			var updateStatesForSites = function() {
				RtaService.getStatesForSites.query({
					siteIds: siteIds
				}).$promise.then(function(states) {
					setStatesInAgents(states);
				})
			};

			var updateStatesForTeams = function() {
				RtaService.getStatesForTeams.query({
					teamIds: teamIds
				}).$promise.then(function(states) {
					setStatesInAgents(states);
				})
			};

			if (teamId) {
				RtaService.getAgents.query({
						teamId: teamId
					}).$promise
					.then(function(agents) {
						$scope.agents = agents;
						$scope.siteName = agents[0].SiteName;
						$scope.teamName = agents[0].TeamName;
					}).then(updateStates);

				$interval(function() {
					updateStates();
				}, 5000);
			}

			if (siteIds) {
				RtaService.getAgentsForSites.query({
						siteIds: siteIds
					}).$promise
					.then(function(agents) {
						$scope.agents = agents;
					}).then(updateStatesForSites);

				$interval(function() {
					updateStatesForSites();
				}, 5000);
			}

			if (teamIds) {
				RtaService.getAgentsForTeams.query({
						teamIds: teamIds
					}).$promise
					.then(function(agents) {
						$scope.agents = agents;
					}).then(updateStatesForTeams);

				$interval(function() {
					updateStatesForTeams();
				}, 5000);
			}

			$scope.goBackToRoot = function() {
				$state.go('rta-sites');
			};

			$scope.goBack = function() {
				$state.go('rta-teams', siteId);
			};

			// 	$scope.gridOptions = {
			// 		columnDefs: [
			// { name: 'Name', field: 'Name', enableColumnMenu: false },
			// { name: 'TeamName', field: 'TeamName', enableColumnMenu: false},
			// { name: 'State', field: 'State', enableColumnMenu: false },
			// { name: 'Activity', field: 'Activity', enableColumnMenu: false },
			// { name: 'Next Activity', field: 'Next Activity', enableColumnMenu: false },
			// { name: 'Next Activity Start Time', field: 'Next Activity Start Time', enableColumnMenu: false },
			// { name: 'Alarm', field: 'Alarm', enableColumnMenu: false },
			// { name: 'Time in Alarm', field: 'Time in Alarm', enableColumnMenu: false }
			// 		],
			// 		data: $scope.filteredAgents
			// 	};

			  $scope.filterData = function() {
			    $scope.filteredAgents = $filter('agentFilter')( $scope.agents, $scope.filterText, undefined);
			  };
		}
	]);
})();
