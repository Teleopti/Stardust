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
				$scope.stateCodes = [];
				var statesCellTemplate = '<div class="ui-grid-cell-contents" style=" margin: 0 auto; text-align: center"><button ng-click="grid.appScope.sendState(row.entity.UserCode, row.entity.DataSource, COL_FIELD)" class="wfm-btn wfm-btn-primary" style="width: 120px;">{{COL_FIELD}}</button></div>';

				$scope.gridOptions = {
					rowHeight: 60,
					enableGridMenu: true
				};

				$scope.gridOptions.onRegisterApi = function (gridApi) {
					$scope.gridApi = gridApi.grid.api;
				};

				function getStateCodes() {
					return $resource('../RtaTool/PhoneStates/For', {}, {
						query: {
							method: 'GET',
							isArray: true
						}
					}).query().$promise;
				}

				function getAgents() {
					return $resource('../RtaTool/Agents/For', {}, {
						query: {
							method: 'GET',
							isArray: true
						}
					}).query().$promise;
				};

				(function init() {
					getStateCodes()
						.then(function (states) {
							$scope.stateCodes = states;
						})
						.then(getAgents)
						.then(function (agents) {
							$scope.agents = agents;
							$scope.filteredAgents = $scope.agents;
							$scope.agents.forEach(function (a) {
								$scope.stateCodes.forEach(function (s) {
									a[s.Code] = s.Code;
								})
							});
						})
						.then(function () {
							var coloumnDefs = [{
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

							$scope.gridOptions = {
								columnDefs: coloumnDefs,
								rowHeight: 60,
								enableGridMenu: true
							};

							$scope.gridOptions.data = $scope.filteredAgents;
						});
				})();

				$scope.sendState = function (userCode, dataSource, code) {
					var data = createState(userCode, dataSource, code)
					$resource('../Rta/State/Change', {}, {
						query: {
							method: 'POST',
						},
					}).query(data).$promise;
				}

				$scope.sendBatches = function (stateCode) {
					var selectedAgents = $scope.gridApi.selection.getSelectedRows();
					if (selectedAgents.length == 0)
						selectedAgents = $scope.filteredAgents;
					sendBatch(selectedAgents.map(function (s) {
						return createState(s.UserCode, s.DataSource, stateCode);
					}));
				}

				$scope.sendRandomBatch = function () {
					var selectedAgents = $scope.gridApi.selection.getSelectedRows();
					if (selectedAgents.length === 0)
						sendBatch($scope.agents.map(function (a) {
							return createState(a.UserCode, a.DataSource, getRandomStateCode().Code)
						}));
					else {
						sendBatch(selectedAgents.map(function (s) {
							return createState(s.UserCode, a.DataSource, getRandomStateCode().Code);
						}));
					}
				}
				function getRandomStateCode() {
					return $scope.stateCodes[Math.floor(Math.random() * $scope.stateCodes.length)]
				}

				function sendBatch(batch) {
					return $resource('../Rta/State/Batch', {}, {
						query: {
							method: 'POST',
						},
					}).query(batch).$promise;
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
