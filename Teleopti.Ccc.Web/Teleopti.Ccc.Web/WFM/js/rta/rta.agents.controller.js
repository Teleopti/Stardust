(function() {
	'use strict';

	angular.module('wfm.rta').controller('RtaAgentsCtrl', [
		'$scope', '$filter', '$state', '$stateParams', '$interval', 'RtaOrganizationService', 'RtaService',
		function($scope, $filter, $state, $stateParams, $interval, RtaOrganizationService, RtaService) {

			var siteId = $stateParams.siteId;
			var teamId = $stateParams.teamId;
			var siteIds = $stateParams.siteIds;
			var teamIds = $stateParams.teamIds;
			$scope.agents = [];

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

			// var updateStatesForTeams = function() {
			// 	RtaService.getStatesForTeams.query({
			// 		teamIds: teamIds
			// 	}).$promise.then(function(states) {
			// 		setStatesInAgents(states);
			// 	})
			// };

			var updateStatesForTeams = function(teamId) {
				RtaService.getStates.query({
					teamId: teamId
				}).$promise.then(function(states) {
					setStatesInAgents(states);
				});
			};

			var updateGrid = function() {
				if ($scope.filterText === undefined)
					$scope.gridOptions.data = $scope.agents;
				else
					$scope.filterData();
			};

			if (teamId) {
				RtaService.getAgents.query({
						teamId: teamId
					}).$promise
					.then(function(agents) {
						$scope.agents = agents;
						$scope.siteName = agents[0].SiteName;
						$scope.teamName = agents[0].TeamName;
					})
					.then(updateStates)
					.then(updateGrid);

				$interval(function() {
					updateStates();
					updateGrid();
				}, 5000);
			}

			if (siteIds) {
				RtaService.getAgentsForSites.query({
						siteIds: siteIds
					}).$promise
					.then(function(agents) {
						$scope.agents = agents;
					})
					.then(updateStatesForSites)
					.then(updateGrid);

				$interval(function() {
					updateStatesForSites();
					updateGrid();
				}, 5000);
			}

			if (teamIds) {
				teamIds.forEach(function(teamId) {
					RtaService.getAgents.query({
							teamId: teamId
						}).$promise
						.then(function(agents) {
							agents.forEach(function(a) {
								$scope.agents.push(a)
							});
							return agents[0].TeamId;
						})
						.then(function(teamId) {
							updateStatesForTeams(teamId)
						})
						.then(updateGrid);
				});
				$interval(function() {
					teamIds.forEach(function(teamId) {
						updateStatesForTeams(teamId);
					});
					updateGrid();
				}, 5000);

			};

			// if (teamIds) {
			// 	RtaService.getAgentsForTeams.query({
			// 			teamIds: teamIds
			// 		}).$promise
			// 		.then(function(agents) {
			// 			$scope.agents = agents;
			// 		}).then(updateStatesForTeams)
			// 		.then(updateGrid);
			//
			// 	$interval(function() {
			// 		updateStatesForTeams();
			// 		updateGrid();
			// 	}, 5000);
			// }

			$scope.goBackToRoot = function() {
				$state.go('rta-sites');
			};

			$scope.goBack = function() {
				$state.go('rta-teams', {siteId: siteId});
			};

			$scope.format = function(time) {
				var momentTime = moment.utc(time);
				if (momentTime.format("YYYYMMDD") > moment().format("YYYYMMDD")) {
					return momentTime.format('YYYY-MM-DD HH:mm:ss');
				}
				return momentTime.format('HH:mm');
			};

			$scope.formatDuration = function(duration) {
				var durationInSeconds = moment.duration(duration, 'seconds');
				return (Math.floor(durationInSeconds.asHours()) + moment(durationInSeconds.asMilliseconds()).format(':mm:ss'));
			};

			$scope.hexToRgb = function(hex) {
				hex = hex ? hex.substring(1) : 'ffffff';
				var bigint = parseInt(hex, 16);
				var r = (bigint >> 16) & 255;
				var g = (bigint >> 8) & 255;
				var b = bigint & 255;
				return "rgba(" + r + ", " + g + ", " + b + ", 0.6)";
			}

			$scope.filterData = function() {
				$scope.gridOptions.data = $filter('agentFilter')($scope.agents, $scope.filterText);
			};

			var coloredCellTemplate = '<div class="ui-grid-cell-contents">{{COL_FIELD}}</div>';
			var coloredWithTimeCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.format(COL_FIELD)}}</div>';
			var coloredWithDurationCellTemplate = '<div class="ui-grid-cell-contents">{{grid.appScope.formatDuration(COL_FIELD)}}</div>';

			$scope.gridOptions = {
				rowTemplate: '<div style="background-color: {{grid.appScope.hexToRgb(row.entity.AlarmColor)}} !important;" ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.colDef.name" class="ui-grid-cell" ng-attr-agentid="{{row.entity.PersonId}}" ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader }" ui-grid-cell></div>',
				columnDefs: [{
					name: 'Name',
					field: 'Name',
					enableColumnMenu: false,
					cellTemplate: coloredCellTemplate,
				}, {
					name: 'TeamName',
					field: 'TeamName',
					enableColumnMenu: false,
					cellTemplate: coloredCellTemplate,
				}, {
					name: 'State',
					field: 'State',
					enableColumnMenu: false,
					cellTemplate: coloredCellTemplate,
				}, {
					name: 'Activity',
					field: 'Activity',
					enableColumnMenu: false,
					cellTemplate: coloredCellTemplate,
				}, {
					name: 'Next Activity',
					field: 'NextActivity',
					enableColumnMenu: false,
					cellTemplate: coloredCellTemplate,
				}, {
					name: 'Next Activity Start Time',
					field: 'NextActivityStartTime',
					enableColumnMenu: false,
					cellTemplate: coloredWithTimeCellTemplate
				}, {
					name: 'Alarm',
					field: 'Alarm',
					enableColumnMenu: false,
					cellTemplate: coloredCellTemplate
				}, {
					name: 'Time in Alarm',
					field: 'TimeInState',
					enableColumnMenu: false,
					cellTemplate: coloredWithDurationCellTemplate
				}],
				data: $scope.agents
			};

		}
	]);
})();
