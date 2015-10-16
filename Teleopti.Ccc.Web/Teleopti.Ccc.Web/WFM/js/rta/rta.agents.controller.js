(function() {
	'use strict';

	angular.module('wfm.rta').controller('RtaAgentsCtrl', [
		'$scope', '$filter', '$state', '$stateParams', '$interval', 'RtaOrganizationService', 'RtaService',
		function($scope, $filter, $state, $stateParams, $interval, RtaOrganizationService, RtaService) {

			var siteId = $stateParams.siteId;
			var teamId = $stateParams.teamId;
			var siteIds = $stateParams.siteIds;
			var teamIds = $stateParams.teamIds;

			var updateStates = function() {
				RtaService.getStates.query({
						teamId: teamId
					}).$promise
					.then(function(states) {
						$scope.states = states;
					});
			};

			var updateStatesForSites = function() {
				RtaService.getStatesForSites.query({
					siteIds: siteIds
				}).$promise.then(function(states) {
					$scope.states = states;
				})
			};

			var updateStatesForTeams = function() {
				RtaService.getStatesForTeams.query({
					teamIds: teamIds
				}).$promise.then(function(states){
					$scope.states = states;
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
				.then(function(agents){
					$scope.agents = agents;
				}).then(updateStatesForTeams);

				$interval(function() {
					updateStatesForTeams();
				}, 5000);
			}

			$scope.format = function(time) {
				return moment.utc(time).format('YYYY-MM-DD HH:mm:ss');
			};

			$scope.formatDuration = function(duration) {
				var durationInSeconds = moment.duration(duration, 'seconds');
				return (Math.floor(durationInSeconds.asHours()) + moment(durationInSeconds.asMilliseconds()).format(':mm:ss'));
			};

			$scope.goBackToRoot = function () {
				$state.go('rta-sites');
			};

			$scope.goBack = function () {
				$state.go('rta-teams', siteId);
			};
		}
	]);
})();
