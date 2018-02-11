(function() {
	'use strict';
	angular.module('wfm.teamSchedule').directive('commandCheck', commandCheckDirective);

	function commandCheckDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: commandCheckCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/commands/teams.directive.cmd.commandCheck.html',
			require: ['^teamscheduleCommandContainer', 'commandCheck'],
			link: function (scope, elem, attrs, ctrls) {
				var containerCtrl = ctrls[0];

				scope.vm.containerCtrl = containerCtrl;

				scope.vm.currentCommandLabel = containerCtrl.activeCmd;
				scope.vm.getDate = containerCtrl.getDate;

				scope.vm.init();
			}
		};
	}

	commandCheckCtrl.$inject = ['$scope', '$translate', 'CommandCheckService','serviceDateFormatHelper'];

	function commandCheckCtrl($scope, $translate, CommandCheckService, serviceDateFormatHelper) {
		var vm = this;
		vm.showCheckbox = false;

		vm.checkFailedAgentList = [];
		vm.initFinished = false;


		vm.updatePersonSelection = function(agent) {
			personSelectionSvc.updatePersonSelection(agent);
			personSelectionSvc.toggleAllPersonProjections(agent, vm.getDate());
		};

		function getAgentListModifier(keepAgents) {
			var checkFailedAgentIdList = vm.checkFailedAgentList.map(function(agent) { return agent.personId; });
			return function(requestData) {
				if (keepAgents) return requestData;

				if (requestData.PersonDates) {
				    requestData.PersonDates = requestData.PersonDates.filter(function (pd) {
						return checkFailedAgentIdList.indexOf(pd.PersonId) < 0;
					});
				}
				if (requestData.PersonIds) {
				    requestData.PersonIds = requestData.PersonIds.filter(function (id) {
				        return checkFailedAgentIdList.indexOf(id) < 0;
				    });
				}
				if (requestData.PersonActivities) {
					requestData.PersonActivities = requestData.PersonActivities.filter(function (personActivity) {
						return checkFailedAgentIdList.indexOf(personActivity.PersonId) < 0;
					});
				}
				return requestData;
			}
		};

		function getTooltipMessageForAgent(overlappedLayers) {
			if (!overlappedLayers) {
				return [];
			}
			var result = [];
			overlappedLayers.forEach(function(overlappedLayer) {
				result.push(overlappedLayer.Name);
				result.push(serviceDateFormatHelper.getDateTime(overlappedLayer.StartTime));
				result.push(serviceDateFormatHelper.getDateTime(overlappedLayer.EndTime));
			});
			return result;
		}

		vm.applyCommandFix = function () {
			vm.config.actionOptions.forEach(function (option) {
				if (option == vm.currentActionOptionValue) {
					if (option == 'DoNotModifyForTheseAgents') {
						CommandCheckService.completeCommandCheck(getAgentListModifier(false));
					} else if (option == 'OverrideForTheseAgents') {
						CommandCheckService.completeCommandCheck(getAgentListModifier(true));
					} else if (option == 'MoveNonoverwritableActivityForTheseAgents') {
						CommandCheckService.completeCommandCheck(function (requestData) {
							requestData.MoveConflictLayerAllowed = true;
							return requestData;
						});
					}
				}
			});
		};

		vm.init = function() {

			var groupSchedules = vm.containerCtrl.scheduleManagementSvc.schedules();

			function getScheduleVm(personId) {
				var filtered = groupSchedules.filter(function(schedule) {
					return schedule.PersonId === personId;
				});
				return filtered.length > 0? filtered[0]: null;
			}

			CommandCheckService.getCheckFailedAgentList().forEach(function(agent) {

				vm.checkFailedAgentList.push({
					personId: agent.PersonId,
					tooltips: getTooltipMessageForAgent(agent.OverlappedLayers),
					scheduleVm: getScheduleVm(agent.PersonId)
				});
			});

			vm.config = CommandCheckService.getCheckConfig();
			vm.currentActionOptionValue = vm.config.actionOptions[0];

			vm.initFinished = true;
		};

	}
})();
