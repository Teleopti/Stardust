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

		var sendingBatchWithRandomStatesTrigger = null;

		vm.gridOptions = {
			rowHeight: 60,
			enableGridMenu: true
		};

		vm.gridOptions.onRegisterApi = function (gridApi) {
			vm.gridApi = gridApi.grid.api;
		};

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
					vm.gridOptions = {
						columnDefs: buildColumnDefinitionsFromStateCodes(vm.stateCodes),
						rowHeight: 60,
						enableGridMenu: true
					};

					vm.gridOptions.data = vm.agents;
				});
		})();

		function buildColumnDefinitionsFromStateCodes(stateCodes) {
			return [{
				field: 'Name',
				displayName: 'Name',
				sort: {
					direction: 'asc'
				},
				width: '10%'
			}, {
				field: 'UserCode',
				displayName: 'User Code',
				cellTemplate: '<div style="margin: 0 auto; text-align: center">{{row.entity.DataSource}}_{{row.entity.UserCode}}</div>',
				width: '8%'
			}].concat(stateCodes
				.map(function (s) {
					return {
						field: s.Code,
						displayName: s.Name,
						cellTemplate: 'app/rtaTool/rtaTool.statesCellTemplate.html',
						enableSorting: false
					}
				}));
		}

		vm.sendBatches = function (stateCode) {
			sendBatch(stateCode);
		}

		vm.sendRandom = function () {
			var selectedAgents = vm.gridApi.selection.getSelectedRows();
			selectedAgents = selectedAgents.length > 0 ? selectedAgents : vm.agents
			var randomAgent = selectedAgents[Math.floor(Math.random() * selectedAgents.length)];
			var stateName = vm.stateCodes[Math.floor(Math.random() * vm.stateCodes.length)].Name;
			vm.sendState(randomAgent.UserCode, randomAgent.DataSource, stateName);
		}

		function sendBatch(stateCode) {
			var selectedAgents = vm.gridApi.selection.getSelectedRows();
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
