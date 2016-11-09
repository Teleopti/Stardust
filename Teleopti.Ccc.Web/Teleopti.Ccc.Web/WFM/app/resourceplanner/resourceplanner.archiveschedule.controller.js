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
				vm.selectedTeams = [];
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
					vm.recievedMessages += messages.length;
					if (vm.totalMessages === vm.recievedMessages) {
						$timeout(function() {
							vm.showProgress = false;
							vm.progress = 0;
						}, 3000);
						
					}
					
				};
				vm.canRunArchiving = function() {
					return !vm.showProgress;
				};
				vm.runArchiving = function (fromScenario, toScenario, period, teamSelection) {
					if (!vm.canRunArchiving()) return;
					vm.totalMessages = 0;
					vm.recievedMessages = 0;
					vm.showProgress = true;
					var archiveScheduleModel = {
						FromScenario: fromScenario.Id,
						ToScenario: toScenario.Id,
						StartDate: period.startDate,
						EndDate: period.endDate,
						TrackId: trackingId,
						SelectedTeams: teamSelection
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

				$scope.$on('teamSelectionChanged',
					function(event, args) {
						angular.forEach(args,
							function (value, key) {
								if (value.type === "Team") {
									// We only care about teams
									var itemIndex = vm.selectedTeams.indexOf(value.id);
									if (value.selected) {
										// It should be added to selectedTeams
										if (itemIndex === -1)
											vm.selectedTeams.push(value.id);
									} else {
										// It should be removed from selectedTeams
										if (itemIndex !== -1)
											vm.selectedTeams.splice(itemIndex, 1);
									}
								}
							});
					});
			}
		]);
})();
