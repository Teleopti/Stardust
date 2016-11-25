(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.filter('defaultScenario',
			function () {
				return function (input, defaultScenario) {
					var result = [];
					angular.forEach(input,
						function (value) {
							if (value.DefaultScenario === defaultScenario)
								result.push(value);
						});
					return result;
				};
			})
		.controller('ResourceplannerArchiveScheduleCtrl',
		[
			'ArchiveScheduleSrvc',
			'guidgenerator',
			'$timeout',
			'$scope',
			'NoticeService',
			'defaultScenarioFilter',
			'$translate',
			'$stateParams',
			function (ArchiveScheduleSrvc,
				guidgenerator,
				$timeout,
				$scope,
				NoticeService,
				defaultScenarioFilter,
				$translate,
				$stateParams) {
				var vm = this;
				vm.isImportSchedule = $stateParams.isImportSchedule;
				vm.ArchiveScheduleOrImportSchedule = !vm.isImportSchedule ? 'ArchiveSchedule' : 'ImportSchedule';
				vm.ArchivingOrImporting = !vm.isImportSchedule ? 'Archiving' : 'Importing';
				vm.ArchiveOrImport = !vm.isImportSchedule ? 'Archive' : 'Import';
				vm.scenarios = [];
				vm.dateRangeTemplateType = 'popup';
				vm.selection = {
					teams: [],
					fromScenario: null,
					toScenario: null,
					period: {
						startDate: moment().utc().add(1, 'months').startOf('month').toDate(),
						endDate: moment().utc().add(2, 'months').startOf('month').toDate()
					}
				};

				vm.showConfirmModal = false;

				var validateArchivingParameters = function (fromScenario, toScenario, period, teamSelection) {
					var validationResult = { messages: [] };

					if (fromScenario == null)
						validationResult.messages.push($translate.instant('YouNeedToPickAScenarioTo' + vm.ArchiveOrImport + 'FromDot'));
					else if (!vm.isImportSchedule && !fromScenario.DefaultScenario)
						validationResult.messages.push($translate.instant('TheScenarioYouArchiveFromMustBeTheDefaultScenarioDot'));
					else if (vm.isImportSchedule && fromScenario.DefaultScenario)
						validationResult.messages.push($translate.instant('TheScenarioYouImportFromMustNotBeTheDefaultScenarioDot'));
					if (toScenario == null)
						validationResult.messages.push($translate.instant('YouNeedToPickAScenarioTo' + vm.ArchiveOrImport + 'ToDot'));
					else if (!vm.isImportSchedule && toScenario.DefaultScenario)
						validationResult.messages.push($translate.instant('TheScenarioYouArchiveToMustNotBeTheDefaultScenarioDot'));
					else if (vm.isImportSchedule && !toScenario.DefaultScenario)
						validationResult.messages.push($translate.instant('TheScenarioYouImportToMustBeTheDefaultScenarioDot'));
					if (teamSelection.length === 0)
						validationResult.messages.push($translate.instant('PickAtleastOneTeamDot'));
					if (moment(period.endDate).diff(period.startDate, 'days') > 65)
						validationResult.messages.push($translate.instant('PickASmallerDatePeriodDot'));
					if (moment(period.startDate).diff(moment(), 'days') < 1)
						validationResult.messages.push($translate.instant('YouNeedToSelectADateInTheFutureDot'));
					validationResult.successful = validationResult.messages.length === 0;
					return validationResult;
				}
				var resetTracking = function () {
					vm.tracking = {
						totalMessages: 0,
						recievedMessages: 0,
						totalPeople: 0,
						progress: 0,
						jobId: null
					};
				};
				resetTracking();

				var updateProgress = function () {
					if (vm.tracking.totalMessages === 0) return 0;
					return 100 * (vm.tracking.recievedMessages / vm.tracking.totalMessages);
				}

				ArchiveScheduleSrvc.scenarios.query()
					.$promise.then(function (result) {
						vm.scenarios = result;
						if (vm.isImportSchedule)
							vm.selection.toScenario = defaultScenarioFilter(result, true)[0];
						else
							vm.selection.fromScenario = defaultScenarioFilter(result, true)[0];
					});

				var completedArchiving = function () {
					if (vm.tracking.totalPeople === 0) {
						NoticeService.info($translate.instant('YourSelectionResultedInZeroPeopleDot'), null, true);
					} else {
						NoticeService.success($translate.instant('Done' + vm.ArchivingOrImporting + 'ForXPeopleDot').replace('{0}', vm.tracking.totalPeople), null, true);
					}

					$timeout(function () {
						vm.showProgress = false;
						resetTracking();
					}, 3000);
				}

				vm.canRunArchiving = function () {
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
					vm.confirmationText = $translate.instant(vm.ArchivingOrImporting + 'ConfirmationWithParameters')
						.replace('{0}', toScenario.Name)
						.replace('{1}', moment(period.startDate).format('L'))
						.replace('{2}', moment(period.endDate).format('L'));
					vm.showConfirmModal = true;
				};
				var checkProgress = function () {
					$timeout(function () {
						if (vm.showProgress) {
							ArchiveScheduleSrvc.getStatus.query({ id: vm.tracking.jobId })
								.$promise.then(function (result) {
									vm.tracking.recievedMessages = result.Successful;
									vm.tracking.progress = updateProgress();
									if (vm.tracking.totalMessages === vm.tracking.recievedMessages) {
										completedArchiving();
									} else {
										checkProgress();
									}
								});
						}
					},
						1000);
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
						SelectedTeams: teamSelection
					};
					(vm.isImportSchedule ? ArchiveScheduleSrvc.runImporting : ArchiveScheduleSrvc.runArchiving).post({}, JSON.stringify(archiveScheduleModel))
						.$promise.then(function (result) {
							vm.tracking.totalMessages = result.TotalMessages;
							vm.tracking.totalPeople = result.TotalSelectedPeople;
							vm.tracking.jobId = result.JobId;
							checkProgress();
						});
				};

				$scope.$on('teamSelectionChanged',
					function (event, args) {
						angular.forEach(args,
							function (value) {
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
