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
		'$filter'];

	function RtaToolController(rtaToolService, $scope, $q, $interval, $filter) {
		var vm = this;

		vm.pause = true;
		vm.sendInterval = { time: 5 };
		vm.stateCodes = [];
		vm.snapshot = false;
		vm.authKey = "!#¤atAbgT%";
		vm.selectedAgentsArray = [];

		var sendingBatchWithRandomStatesTrigger = null;

		vm.tableData = [];

		(function init() {
			rtaToolService.getStateCodes()
				.then(function (states) {
					vm.stateCodes = states;
				})
				.then(rtaToolService.getAgents)
				.then(function (agents) {
					vm.agents = agents;

					vm.agents.forEach(function (a) {
						vm.stateCodes.forEach(function (s) {
							a[s.Code] = s.Code;
						})
					});
				})
				.then(function () {
					vm.tableData = vm.agents;
				});
		})();

		vm.sendBatches = function (stateCode) {
			sendBatch(stateCode);
		}
		var toggledAgents = false;
		vm.toggleAllAgents = function () {
			if (!toggledAgents || vm.selectedAgentsArray.length == 0) {
				toggledAgents = true;
				vm.gridOptions.data.forEach(function (data) {
					data.selectedRow = true;
					vm.selectedAgentsArray.push(data);
				});
			} else {
				vm.gridOptions.data.forEach(function (data) {
					data.selectedRow = false;
				});
				toggledAgents = false;
				vm.selectedAgentsArray = [];
			}
		}

		vm.selectAgent = function (agent) {
			if (agent.selectedRow) {
				agent.selectedRow = false;
				var agentToRemove = vm.selectedAgentsArray.find(function (a) {
					return agent.UserCode === a.UserCode;
				});

				var index = vm.selectedAgentsArray.indexOf(agentToRemove);

				vm.selectedAgentsArray.splice(index, 1);
			} else {
				agent.selectedRow = true;

				vm.selectedAgentsArray.push(agent);
			}
		}

		vm.sendRandom = function () {
			var selectedAgents = vm.selectedAgentsArray;
			selectedAgents = selectedAgents.length > 0 ? selectedAgents : vm.agents
			var randomAgent = selectedAgents[Math.floor(Math.random() * selectedAgents.length)];
			var stateName = vm.stateCodes[Math.floor(Math.random() * vm.stateCodes.length)].Name;
			vm.sendState(randomAgent.UserCode, randomAgent.DataSource, stateName);
		}

		function sendBatch(stateCode) {
			var selectedAgents = vm.selectedAgentsArray;
			selectedAgents = selectedAgents.length > 0 ? selectedAgents : vm.agents

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
						return s.DataSource == d
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

				rtaToolService.sendBatch(batch);
			});
		}

		vm.sendState = function (userCode, dataSource, displayName) {
			var stateCode = vm.stateCodes
				.filter(function (s) { return s.Name == displayName; })
				.map(function (s) { return s.Code; })[0];

			var batch = {
				AuthenticationKey: vm.authKey,
				SourceId: dataSource,
				IsSnapshot: vm.snapshot,
				States: [
					{
						UserCode: userCode,
						StateCode: stateCode
					}
				]
			};
			rtaToolService.sendBatch(batch);
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

		$scope.$watch(function () {
			return vm.pause;
		}, function () {
			if (vm.pause)
				stopSendingBatchWithRandomStates();
			else
				startSendingBatchWithRandomStates();
		});

		vm.togglePause = function () {
			vm.pause = !vm.pause;
		}
	};
})();
