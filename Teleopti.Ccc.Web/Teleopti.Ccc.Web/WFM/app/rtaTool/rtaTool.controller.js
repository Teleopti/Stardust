(function () {
	'use strict';
	angular.module('wfm.rtaTool')
		.controller('RtaToolCtrl', [
			'$scope',
			'$resource',
			'$q',
			'$interval',
			'$filter',
			function ($scope, $resource, $q, $interval, $filter) {
				$scope.pause = true;
				var polling = null;
				$scope.sendInterval = { time: 5 };
				$scope.stateCodes = [
					{ Name: 'Ready', Code: 'Ready' },
					{ Name: 'InCall', Code: 'InCall' },
					{ Name: 'LOGGED ON', Code: 'LOGGED ON' },
					{ Name: 'ACW', Code: 'ACW' },
					{ Name: 'IDLE', Code: 'IDLE' },
					{ Name: 'OFF', Code: 'OFF' }
					];
				var statesCellTemplate = '<div class="ui-grid-cell-contents" style=" margin: 0 auto; text-align: center"><button ng-click="grid.appScope.sendState(row.entity.UserCode, row.entity.DataSource, COL_FIELD)" class="wfm-btn wfm-btn-primary" style="width: 120px;">{{COL_FIELD}}</button></div>';

				function columnDefs() {
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
						cellTemplate: '<div style="margin: 0 auto; text-align: center">{{row.entity.UserCode}}</div>',
						width: '8%'
					}].concat($scope.stateCodes.map(function (s) {
						return {
							field: s.Code,
							displayName: s.Name,
							cellTemplate: statesCellTemplate,
							enableSorting: false
						}
					}));
				}

				$scope.gridOptions = {
					columnDefs: columnDefs(),
					rowHeight: 60,
					enableGridMenu: true
				};
				$scope.gridOptions.onRegisterApi = function (gridApi) {
					$scope.gridApi = gridApi.grid.api;
				};

				function getAgents() {
					return $resource('../RtaTool/Agents/For', {}, {
						query: {
							method: 'GET',
							isArray: true
						}
					}).query().$promise;
				};

				getAgents()
					.then(function (agents) {
						console.table(agents)
						$scope.agents = agents;
						$scope.filteredAgents = $scope.agents;
						$scope.agents.forEach(function (a) {
							$scope.stateCodes.forEach(function (s) {
								a[s.Code] = s.Code;
							})
						});
						$scope.gridOptions.data = $scope.filteredAgents;
					});


				$scope.sendBatches = function (stateCode) {
					var selectedAgents = $scope.gridApi.selection.getSelectedRows();
					var batch = selectedAgents.map(function (s) {
						return createState(s.UserCode, s.DataSource, stateCode);
					});
					$resource('../Rta/State/Batch', {}, {
						query: {
							method: 'POST',
						},
					})
						.query(batch).$promise;
				}

				$scope.sendState = function (userCode, dataSource, code) {
					var data = createState(userCode, dataSource, code)
					$resource('../Rta/State/Change', {}, {
						query: {
							method: 'POST',
						},
					})
						.query(data).$promise;
				}

				function createState(userCode, dataSource, code) {
					return {
						AuthenticationKey: "!#¤atAbgT%",
						UserCode: userCode,
						StateCode: code,
						StateDescription: code,
						IsLoggedOn: true,
						SecondsInState: 0,
						TimeStamp: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
						PlatformTypeId: '00000000-0000-0000-0000-000000000000',
						SourceId: dataSource,
						BatchId: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
						SnapshotId: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
						IsSnapshot: false
					};
				}

				function randomBatchForAll() {
					return $scope.agents.map(function (a) {
						var randomStateCode = $scope.stateCodes[Math.floor(Math.random() * $scope.stateCodes.length)];
						return createState(a.UserCode, randomStateCode.Code)
					});
				}

				$scope.sendRandomBatch = function () {
					var selectedAgents = $scope.gridApi.selection.getSelectedRows();
					var stateCode = $scope.stateCodes[Math.floor(Math.random() * $scope.stateCodes.length)];

					var batch = selectedAgents.length === 0 ?
						randomBatchForAll() :
						selectedAgents.map(function (s) {
							return createState(s.UserCode, stateCode.Code);
						});
					$resource('../Rta/State/Batch', {}, {
						query: {
							method: 'POST',
						},
					})
						.query(batch).$promise;

				}

				function setupPolling() {
					polling = $interval(function () {
						$scope.sendRandomBatch();
					}, parseInt($scope.sendInterval.time) * 1000);
				}

				function cancelPolling() {
					if (polling != null) {
						$interval.cancel(polling);
					}
				}

				$scope.$watch('pause', function () {
					if ($scope.pause) {
						cancelPolling();
					} else {
						setupPolling();
					}
				});

				$scope.togglePause = function () {
					$scope.pause = !$scope.pause;
				}
			}
		]);
})();
