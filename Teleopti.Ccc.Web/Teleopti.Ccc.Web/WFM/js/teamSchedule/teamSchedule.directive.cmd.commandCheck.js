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
			templateUrl: 'js/teamSchedule/html/commandCheck.tpl.html',
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
		if (!containerCtrl.hasToggle('AutoMoveOverwrittenActivityForOperationsEnabled')){
			scope.vm.actionOptions.shift();
			scope.vm.currentActionOptionValue = scope.vm.actionOptions[0].value;
		}

		var focusTarget = elem[0].querySelector('.focus-default');
		if (focusTarget) angular.element(focusTarget).focus();
		elem.removeAttr('tabindex');
	}

	commandCheckCtrl.$inject = ['$scope', '$translate', 'CommandCheckService', 'PersonSelection', 'ScheduleManagement'];

	function commandCheckCtrl($scope, $translate, CommandCheckService, personSelectionSvc, ScheduleManagementSvc) {
		var vm = this;
		vm.showCheckbox = false;
		vm.overlappedAgents = [];
		vm.overlappedLayers = [];
		vm.initFinished = false;

		var moveConflictLayerAllowed = false;

		vm.actionOptions = [
			{
				value: 'MoveNonoverwritableActivityForTheseAgents',
				getName: function() {
					return $translate.instant(this.value);
				},
				beforeAction: function() {
					moveConflictLayerAllowed = true;
				}
			},
			{
				value: 'DoNotModifyForTheseAgents',
				getName: function(currentCmdLabel) {
					return $translate.instant(this.value).replace('{0}', $translate.instant(currentCmdLabel));
				},
				beforeAction: function () {
					vm.toggleAllPersonSelection(false);
				}
			},
			{
				value: 'OverrideForTheseAgents',
				getName: function() {
					return $translate.instant(this.value);
				},
				beforeAction: function() {
					vm.toggleAllPersonSelection(true);
				}
			}
		];

		vm.currentActionOptionValue = vm.actionOptions[0].value;

		vm.updatePersonSelection = function(agent) {
			personSelectionSvc.updatePersonSelection(agent);
			personSelectionSvc.toggleAllPersonProjections(agent, vm.getDate());
		};

		vm.toggleAllPersonSelection = function(isSelected) {
			if (vm.currentCommandLabel == 'MoveActivity'){
				var personActivities = CommandCheckService.getRequestData().PersonActivities;
				vm.overlappedAgents.forEach(function(agent) {
					agent.IsSelected = isSelected;
					personActivities.forEach(function(personActivity){
						if(personActivity.PersonId == agent.PersonId){
							personSelectionSvc.updatePersonSelection(agent, personActivity.ShiftLayerIds);
						}
					});
				});
			}else{
				vm.overlappedAgents.forEach(function(agent) {
					agent.IsSelected = isSelected;
					personSelectionSvc.updatePersonSelection(agent);
				});
			}
		};

		vm.getOverlappedLayersForAgent = function (personId) {
			var overlappedLayers, result = [];

			vm.overlappedLayers.forEach(function (agent) {
				if (agent.PersonId == personId)
					overlappedLayers = agent.OverlappedLayers;
			});

			overlappedLayers.forEach(function (overlappedLayer) {
				result.push(overlappedLayer.Name);
				result.push(moment(overlappedLayer.StartTime).format('YYYY-MM-DD HH:mm'));
				result.push(moment(overlappedLayer.EndTime).format('YYYY-MM-DD HH:mm'));
			});
			return result;
		};

		vm.applyCommandFix = function() {
			vm.actionOptions.forEach(function (option) {
				if (option.value == vm.currentActionOptionValue) {
					option.beforeAction && option.beforeAction();
				}
			});
			CommandCheckService.completeCommandCheck({
				moveConflictLayerAllowed: moveConflictLayerAllowed
			});
		};

		vm.init = function() {
			var personIds = [];

			CommandCheckService.getOverlappingAgentList().forEach(function(agent) {
				personIds.push(agent.PersonId);
				vm.overlappedLayers.push({
					PersonId: agent.PersonId,
					OverlappedLayers: agent.OverlappedLayers
				});
			});

			vm.overlappedAgents = ScheduleManagementSvc.groupScheduleVm.Schedules.filter(function(personSchedule) {
				return personIds.indexOf(personSchedule.PersonId) > -1;
			});

			vm.initFinished = true;
		};

		vm.init();
	}
})();