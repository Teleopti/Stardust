(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.filter('defaultScenario',
			function() {
				return function(input, defaultScenario) {
					var result = [];
					angular.forEach(input,
						function(value) {
							if (value.DefaultScenario === defaultScenario)
								result.push(value);
						});
					return result;
				};
			})
		.controller('ResourceplannerArchiveScheduleCtrl',
		[
			'ArchiveScheduleSrvc',
			'signalRSVC',
			'guidgenerator',
			'$interval',
			'$timeout',
			'$scope',
			'NoticeService',
			'defaultScenarioFilter',
			'$translate',
			function(ArchiveScheduleSrvc,
				signalRSVC,
				guidgenerator,
				$interval,
				$timeout,
				$scope,
				NoticeService,
				defaultScenarioFilter,
				$translate) {
				var vm = this;
				vm.scenarios = [];
				vm.dateRangeTemplateType = 'popup';
				vm.selection = {
					teams: [],
					fromScenario: null,
					toScenario: null,
					period: {
						startDate: moment().utc().startOf('month').toDate(),
						endDate: moment().utc().add(1, 'months').startOf('month').toDate()
					}
				};

				vm.showConfirmModal = false;

				var validateArchivingParameters = function (fromScenario, toScenario, period, teamSelection) {
					var validationResult = { messages: [] };

					if (fromScenario == null)
						validationResult.messages.push($translate.instant('YouNeedToPickAScenarioToArchiveFromDot'));
					else if(!fromScenario.DefaultScenario)
						validationResult.messages.push($translate.instant('TheScenarioYouArchiveFromMustBeTheDefaultScenarioDot'));
					if (toScenario == null)
						validationResult.messages.push($translate.instant('YouNeedToPickAScenarioToArchiveToDot'));
					else if (toScenario.DefaultScenario)
						validationResult.messages.push($translate.instant('TheScenarioYouArchiveToMustNotBeTheDefaultScenarioDot'));
					if (teamSelection.length === 0)
						validationResult.messages.push($translate.instant('PickAtleastOneTeamDot'));
					if (moment(period.endDate).diff(period.startDate, 'days') > 65)
						validationResult.messages.push($translate.instant('PickASmallerDatePeriodDot'));
					validationResult.successful = validationResult.messages.length === 0;
					return validationResult;
				}
				var resetTracking = function() {
					vm.tracking = {
						totalMessages: 0,
						recievedMessages: 0,
						totalPeople: 0,
						progress: 0
					};
				};
				resetTracking();

				var trackingId = guidgenerator.newGuid();

				var updateProgress = function() {
					if (vm.tracking.totalMessages === 0) return 0;
					return 100 * (vm.tracking.recievedMessages / vm.tracking.totalMessages);
				}

				ArchiveScheduleSrvc.scenarios.query()
					.$promise.then(function(result) {
						vm.scenarios = result;
						vm.selection.fromScenario = defaultScenarioFilter(result, true)[0];
					});

				var completedArchiving = function() {
					if (vm.tracking.totalPeople === 0) {
						NoticeService.info($translate.instant('YourSelectionResultedInZeroPeopleDot'), null, true);
					} else {
						
						NoticeService.success($translate.instant('DoneArchivingForXPeopleDot').replace('{0}', vm.tracking.totalPeople), null, true);
					}

					$timeout(function() {
							vm.showProgress = false;
							resetTracking();
						}, 3000);
				}
				var updateStatus = function(messages) {
					vm.tracking.recievedMessages += messages.length;
					if (vm.tracking.totalMessages === vm.tracking.recievedMessages) {
						completedArchiving();
					}
				};
				vm.canRunArchiving = function() {
					return !vm.showProgress;
				};

				vm.validateAndShowModal = function (fromScenario, toScenario, period, teamSelection) {
					if (!vm.canRunArchiving()) return;
					var validationResult = validateArchivingParameters(fromScenario, toScenario, period, teamSelection);
					if (!validationResult.successful) {
						angular.forEach(validationResult.messages, function (value) {
							NoticeService.error(value, null, true);
						});
						return;
					}
					vm.confirmationText = $translate.instant('ArchivingConfirmationWithParameters')
						.replace('{0}', toScenario.Name)
						.replace('{1}', moment(period.startDate).format('L'))
						.replace('{2}', moment(period.endDate).format('L'));
					vm.showConfirmModal = true;
				};

				vm.runArchiving = function (fromScenario, toScenario, period, teamSelection) {
					vm.showConfirmModal = false;
					resetTracking();
					vm.showProgress = true;
					var archiveScheduleModel = {
						FromScenario: fromScenario.Id,
						ToScenario: toScenario.Id,
						StartDate: period.startDate,
						EndDate: period.endDate,
						TrackId: trackingId,
						SelectedTeams: teamSelection
					};
					ArchiveScheduleSrvc.runArchiving.post({}, JSON.stringify(archiveScheduleModel))
						.$promise.then(function(result) {
							vm.tracking.totalMessages = result.TotalMessages;
							vm.tracking.totalPeople = result.TotalSelectedPeople;
							if (vm.tracking.totalPeople === 0)
								completedArchiving();
						});
				};

				signalRSVC.subscribeBatchMessage({ DomainType: 'TrackingMessage', DomainId: trackingId }, updateStatus, 50);
				$interval(function() {
						if (vm.showProgress) {
							$scope.$apply(function() {
								vm.tracking.progress = updateProgress();
							});
						}
					}, 100, 0, false);

				$scope.$on('teamSelectionChanged',
					function(event, args) {
						angular.forEach(args,
							function(value) {
								if (value.type === "Team") {
									// We only care about teams
									var itemIndex = vm.selection.teams.indexOf(value.id);
									if (value.selected) {
										// It should be added to selectedTeams
										if (itemIndex === -1)
											vm.selection.teams.push(value.id);
									} else {
										// It should be removed from selectedTeams
										if (itemIndex !== -1)
											vm.selection.teams.splice(itemIndex, 1);
									}
								}
							});
					});
			}
		]);
})();
