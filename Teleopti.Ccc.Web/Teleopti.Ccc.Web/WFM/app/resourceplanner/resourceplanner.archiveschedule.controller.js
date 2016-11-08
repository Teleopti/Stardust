﻿(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerArchiveScheduleCtrl', [
			'ArchiveScheduleSrvc',
			'signalRSVC',
			'guidgenerator',
			function (ArchiveScheduleSrvc, signalRSVC, guidgenerator) {
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
				var trackingId = guidgenerator.newGuid();

				var scenariosPromise = ArchiveScheduleSrvc.scenarios.query().$promise;
				scenariosPromise.then(function (result) {
					vm.scenarios = result;
					vm.fromScenario = result[0];
				});
				var updateStatus = function (messages) {
					console.log(messages);
					vm.recievedMessages += messages.length;
				};

				vm.runArchiving = function (fromScenario, toScenario, period, peopleSelection) {
					//console.log("From", fromScenario);
					//console.log("To", toScenario);
					//console.log("Period", period);
					//console.log("People", peopleSelection);
					var archiveScheduleModel = {
						FromScenario: fromScenario.Id,
						ToScenario: toScenario.Id,
						StartDate: period.startDate,
						EndDate: period.endDate,
						TrackId: trackingId
					};
					ArchiveScheduleSrvc.runArchiving.post({}, JSON.stringify(archiveScheduleModel)).$promise.then(function (result) {
						console.log({
							TotalMessages: result.TotalMessages
						});
						vm.totalMessages = result.TotalMessages;
					});
				};

				signalRSVC.subscribeBatchMessage({ DomainType: 'TrackingMessage', DomainId: trackingId }, updateStatus, 500);
				
			}
		]);
})();
