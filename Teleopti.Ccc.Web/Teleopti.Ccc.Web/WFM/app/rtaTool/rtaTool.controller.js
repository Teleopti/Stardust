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
			sendBatch(function () { return stateCode; });
		}

		vm.sendRandomBatch = function () {
			sendBatch(function () {
				return vm.stateCodes[Math.floor(Math.random() * vm.stateCodes.length)].Code;
			});
		}

		function sendBatch(stateCode) {
			var selectedAgents = vm.gridApi.selection.getSelectedRows();
			selectedAgents = selectedAgents.length > 0 ? selectedAgents : vm.agents
			var distinctAgents = [];
			var distinctDatasources = [];
			distinctDatasources =
				selectedAgents.map(function (s) {
					if (distinctDatasources.indexOf(s.DataSource) == -1) {
						distinctDatasources.push(s.DataSource);
						return s.DataSource;
					}
				}).filter(function (d) { return d != null });

			distinctDatasources.forEach(function (d) {
				rtaToolService.sendBatch(
					selectedAgents.filter(function (s) {
						return s.DataSource == d
					})
						.map(function (s) {
							var key = s.DataSource + "__" + s.UserCode;
							if (distinctAgents.indexOf(key) == -1) {
								distinctAgents.push(key);
								return createState(vm.authKey, s.UserCode, s.DataSource, stateCode(), now());
							}
						})
						.filter(function (s) { return s != null; }))
					.then(function () {
						if (vm.snapshot)
							closeSnapshot(vm.authKey, selectedAgents[0].DataSource, now);
					});
			});
		}


		vm.sendState = function (userCode, dataSource, code) {
			rtaToolService.sendState(createState(vm.authKey, userCode, dataSource, code, now()))
				.then(function () {
					if (vm.snapshot)
						closeSnapshot(vm.authKey, dataSource, now)
				});
		}

		function now() {
			return moment.utc().format('YYYY-MM-DD HH:mm:ss');
		}

		function createState(authKey, userCode, dataSource, code, now) {
			return {
				AuthenticationKey: authKey,
				UserCode: userCode,
				StateCode: code,
				StateDescription: code,
				IsLoggedOn: true,
				SecondsInState: 0,
				TimeStamp: now,
				PlatformTypeId: '00000000-0000-0000-0000-000000000000',
				SourceId: dataSource,
				BatchId: now,
				SnapshotId: now,
				IsSnapshot: vm.snapshot
			};
		}

		function closeSnapshot(authKey, dataSource, snapshotId) {
			rtaToolService.closeSnapshot({
				AuthenticationKey: authKey,
				SourceId: dataSource,
				SnapshotId: snapshotId
			});
		}

		function startSendingBatchWithRandomStates() {
			sendingBatchWithRandomStatesTrigger = $interval(function () {
				vm.sendRandomBatch();
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
