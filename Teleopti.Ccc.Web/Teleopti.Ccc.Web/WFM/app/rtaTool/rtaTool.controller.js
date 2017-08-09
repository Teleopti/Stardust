(function () {
	'use strict';
	angular
		.module('wfm.rtaTool')
		.controller('RtaToolController', RtaToolController);

	RtaToolController.$inject = [
		'rtaToolService',
		'$scope',
		'$q',
		'$interval',
		'$filter',
		'NoticeService',
		'organization'];

	function RtaToolController(rtaToolService, $scope, $q, $interval, $filter, NoticeService, organization) {
		var vm = this;

		vm.pause = true;
		vm.sendInterval = { time: 5 };
		vm.snapshot = false;
		vm.authKey = "!#¤atAbgT%";
		vm.filteredAgents = [];
		vm.stateCodes = [];
		vm.filteredAgentsShown = 100;
		vm.selectSites = selectSites;
		vm.selectTeams = selectTeams;
		vm.openSitePicker = false;
		vm.openTeamPicker = false;
		vm.organization = organization;
		vm.toggleDropdown = toggleDropdown;

		var sendingBatchWithRandomStatesTrigger = null;

		(function init() {
			rtaToolService.getStateCodes()
				.then(function (states) {
					vm.stateCodes = states.map(function (state) {
						state.sendBatch = function () {
							sendBatch(state.Code);
						};
						return state;
					});
				})
				.then(rtaToolService.getAgents)
				.then(function (_agents) {
					vm.agents = _agents.map(function (agent) {
						agent.isSelected = false;
						agent.selectAgent = function () {
							agent.isSelected = !agent.isSelected;
						}
						agent.StateCodes = vm.stateCodes;
						agent.sendState = function (state) {
							sendState(agent, state);
						};
						return agent;
					});

					vm.filteredAgents = vm.agents.slice(0);
					showMoreAgents();
				});

			vm.organization.Sites.forEach(function (site) {
				site.isChecked = false;
				site.toggle = function () {
					site.isChecked = !site.isChecked;
				}
			});

			vm.organization.Teams.forEach(function (team) {
				team.isChecked = false;
				team.toggle = function () {
					team.isChecked = !team.isChecked;
				}
			});
		})();


		vm.filterAgents = function () {
			vm.filteredAgents = $filter('filter')(vm.agents, vm.filterText)
		}

		vm.toggleAgents = function () {
			var selectedAgents = findSelectedAgents();
			var shouldSelectAll = selectedAgents.length != vm.filteredAgents.length;

			vm.filteredAgents.forEach(function (agent) {
				agent.isSelected = shouldSelectAll;
			});
		}

		vm.sendRandom = function () {
			var selectedAgents = findSelectedAgents();
			selectedAgents = selectedAgents.length > 0 ? selectedAgents : vm.filteredAgents;

			var randomAgent = selectedAgents[Math.floor(Math.random() * selectedAgents.length)];
			var state = vm.stateCodes[Math.floor(Math.random() * vm.stateCodes.length)];
			randomAgent.sendState(state);
		}

		function findSelectedAgents() {
			return vm.filteredAgents.filter(function (a) {
				return a.isSelected;
			});
		}

		function sendBatch(stateCode) {
			var selectedAgents = findSelectedAgents();
			selectedAgents = selectedAgents.length > 0 ? selectedAgents : vm.filteredAgents;
			var snapshotId = now();
			var distinctDatasources = selectedAgents
				.map(function (s) {
					return s.DataSource
				})
				.reduce(function (groups, s) {
					if (groups.indexOf(s) == -1)
						groups.push(s)
					return groups;
				}, []);

			distinctDatasources.forEach(function (d) {

				var states = selectedAgents
					.filter(function (s) {
						return s.DataSource == d;
					})
					.reduce(function (groups, s) {
						if (!(groups.find(function (g) { return g.DataSource == s.DataSource && g.UserCode == s.UserCode; }))) {
							groups.push(s);
						}
						return groups;
					}, [])
					.map(function (s) {
						return {
							UserCode: s.UserCode,
							StateCode: stateCode
						};
					});

				var batch = {
					AuthenticationKey: vm.authKey,
					SourceId: d,
					IsSnapshot: vm.snapshot,
					States: states
				};

				rtaToolService.sendBatch(batch).then(function () {
					NoticeService.info('Done!', 5000, true);
				});
			});
		}

		function sendState(agent, state) {
			rtaToolService.sendBatch({
				AuthenticationKey: vm.authKey,
				SourceId: agent.DataSource,
				IsSnapshot: vm.snapshot,
				States: [
					{
						UserCode: agent.UserCode,
						StateCode: state.Code
					}
				]
			});
		}

		function now() {
			return moment.utc().format('YYYY-MM-DD HH:mm:ss');
		}

		function startSendingBatchWithRandomStates() {
			sendingBatchWithRandomStatesTrigger = $interval(function () {
				vm.sendRandom();
			}, parseInt(vm.sendInterval.time) * 1000);
		}

		function stopSendingBatchWithRandomStates() {
			if (sendingBatchWithRandomStatesTrigger != null)
				$interval.cancel(sendingBatchWithRandomStatesTrigger);
		}

		vm.togglePause = function () {
			vm.pause = !vm.pause;
			if (vm.pause) {
				stopSendingBatchWithRandomStates();
			} else {
				startSendingBatchWithRandomStates();
			}
		}

		vm.loadMore = function () {
			vm.filteredAgentsShown += 100;
			showMoreAgents();
		}

		function showMoreAgents() {
			vm.noMoreLoading = vm.filteredAgentsShown >= vm.filteredAgents.length;
			vm.loadingText = vm.noMoreLoading ? "No more agents to load" : "Load more";
		}

		function selectSites() {
			vm.openSitePicker = false;
			var selectedSiteIds = vm.organization.Sites.filter(function (site) {
				return site.isChecked;
			}).map(function (site) {
				return site.SiteId;
			});

			rtaToolService.getAgentsForSites(selectedSiteIds).then(function (result) {
				vm.filteredAgents = result;
			});

		}

		function selectTeams() {
			vm.openTeamPicker = false;
			var selectedTeamIds = vm.organization.Teams.filter(function (team) {
				return team.isChecked;
			}).map(function (team) {
				return team.TeamId;
			});

			rtaToolService.getAgentsForTeams(selectedTeamIds).then(function (result) {
				vm.filteredAgents = result;
			});
		}

		function toggleDropdown(key) {
			if (key === 1) {
				vm.openSitePicker = !vm.openSitePicker;
				if (vm.openTeamPicker) vm.openTeamPicker = false;
			}
			else {
				vm.openTeamPicker = !vm.openTeamPicker;
				if (vm.openSitePicker) vm.openSitePicker = false;
			}

		}
	};
})();
