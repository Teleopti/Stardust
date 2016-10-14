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
			templateUrl: 'app/teamSchedule/html/commandCheck.tpl.html',
			require: ['^teamscheduleCommandContainer', 'commandCheck'],
			compile: function (tElement, tAttrs) {
				var tabindex = angular.isDefined(tAttrs.tabindex) ? tAttrs.tabindex : '0';
				function addTabindexTo() {
					angular.forEach(arguments, function (arg) {
						angular.forEach(arg, function (elem) {
							elem.setAttribute('tabIndex', tabindex);
						});
					});
				}
				addTabindexTo(
					tElement[0].querySelectorAll('md-select.command-check-selector'),
					tElement[0].querySelectorAll('button#applyCommandFix')
				);
				return postlink;
			}
		};
	}

	function postlink(scope, elem, attrs, ctrls) {
		var containerCtrl = ctrls[0];

		scope.vm.currentCommandLabel = containerCtrl.activeCmd;
		scope.vm.getDate = containerCtrl.getDate;

		var focusTarget = elem[0].querySelector('.focus-default');
		if (focusTarget) angular.element(focusTarget).focus();
		elem.removeAttr('tabindex');
	}

	commandCheckCtrl.$inject = ['$scope', '$translate', 'CommandCheckService', 'PersonSelection', 'ScheduleManagement'];

	function commandCheckCtrl($scope, $translate, CommandCheckService, personSelectionSvc, ScheduleManagementSvc) {
		var vm = this;
		vm.showCheckbox = false;

		vm.checkFailedAgentList = [];
		vm.initFinished = false;
				

		vm.updatePersonSelection = function(agent) {
			personSelectionSvc.updatePersonSelection(agent);
			personSelectionSvc.toggleAllPersonProjections(agent, vm.getDate());
		};

		vm.toggleAllPersonSelection = function(isSelected) {			
			vm.agentSchedulesList.forEach(function (agent) {
				agent.scheduleVm.IsSelected = isSelected;
				personSelectionSvc.updatePersonSelection(agent.scheduleVm);
			});			
		};

		function getTooltipMessageForAgent(overlappedLayers) {
			var result = [];
			overlappedLayers.forEach(function(overlappedLayer) {
				result.push(overlappedLayer.Name);
				result.push(moment(overlappedLayer.StartTime).format('YYYY-MM-DD HH:mm'));
				result.push(moment(overlappedLayer.EndTime).format('YYYY-MM-DD HH:mm'));
			});
			return result;
		}

		vm.applyCommandFix = function () {
			var moveConflictLayerAllowed = false;
			vm.config.actionOptions.forEach(function (option) {
				if (option == vm.currentActionOptionValue) {
					if (option == 'DoNotModifyForTheseAgents') {
						vm.toggleAllPersonSelection(false);
					} else if (option == 'OverrideForTheseAgents') {
						vm.toggleAllPersonSelection(true);
					} else if (option == 'MoveNonoverwritableActivityForTheseAgents') {
						moveConflictLayerAllowed = true;
					}
				}
			});

			CommandCheckService.completeCommandCheck({
				moveConflictLayerAllowed: moveConflictLayerAllowed
			});
		};

		vm.init = function() {

			var groupSchedules = ScheduleManagementSvc.groupScheduleVm.Schedules;

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

		vm.init();
	}
})();
