(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerArchiveScheduleCtrl', [
			'ArchiveScheduleSrvc',
			'signalRSVC',
			'guidgenerator',
			'$interval',
			'$timeout',
			'$scope',
			function (ArchiveScheduleSrvc, signalRSVC, guidgenerator, $interval, $timeout, $scope) {
				var vm = this;
				vm.scenarios = [];
				vm.fromScenario = null;
				vm.toScenario = null;
				vm.dateRangeTemplateType = 'popup';
				vm.period = {
					startDate: moment().utc().startOf('month').toDate(),
					endDate: moment().utc().add(1, 'months').startOf('month').toDate()
				};
				vm.totalMessages = 0;
				vm.recievedMessages = 0;
				vm.progress = 0;
				var trackingId = guidgenerator.newGuid();

				var updateProgress = function () {
					if (vm.totalMessages === 0) return 0;
					return 100 * (vm.recievedMessages / vm.totalMessages);
				}
				var scenariosPromise = ArchiveScheduleSrvc.scenarios.query().$promise;
				scenariosPromise.then(function (result) {
					vm.scenarios = result;
					vm.fromScenario = result[0];
				});
				var updateStatus = function (messages) {
					console.log(messages);
					vm.recievedMessages += messages.length;
					if (vm.totalMessages === vm.recievedMessages) {
						$timeout(function() {
							vm.showProgress = false;
							vm.progress = 0;
						}, 3000);
						
					}
					
				};

				vm.runArchiving = function (fromScenario, toScenario, period, peopleSelection) {
					vm.totalMessages = 0;
					vm.recievedMessages = 0;
					vm.showProgress = true;
					var archiveScheduleModel = {
						FromScenario: fromScenario.Id,
						ToScenario: toScenario.Id,
						StartDate: period.startDate,
						EndDate: period.endDate,
						TrackId: trackingId
					};
					ArchiveScheduleSrvc.runArchiving.post({}, JSON.stringify(archiveScheduleModel)).$promise.then(function (result) {
						vm.totalMessages = result.TotalMessages;
					});
				};

				signalRSVC.subscribeBatchMessage({ DomainType: 'TrackingMessage', DomainId: trackingId }, updateStatus, 50);
				$interval(function () {
					if (vm.showProgress) {
						$scope.$apply(function() {
							vm.progress = updateProgress();
						});
					}
						
				}, 100, 0, false);
			}
		]);
})();
