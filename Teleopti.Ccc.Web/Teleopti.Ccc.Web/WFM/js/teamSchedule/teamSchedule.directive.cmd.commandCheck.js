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
		scope.vm.updateActionOptionsText();

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

		vm.actionOptions = [{
			Name: $translate.instant('DoNotModifyForTheseAgents'),
			OnSelected: disableCheckbox,
			BeforeAction: function() {
				vm.toggleAllPersonSelection(false);
			}
		}, {
			Name: $translate.instant('OverrideForTheseAgents'),
			OnSelected: disableCheckbox,
			BeforeAction: function() {
				vm.toggleAllPersonSelection(true);
			}
		}, {
			Name: $translate.instant('OverrideForSelectedAgents'),
			OnSelected: enableCheckbox
		}];

		vm.onActionChange = function() {
			vm.actionOptions.forEach(function(option) {
				if (option.Name == vm.currentActionOption) {
					option.OnSelected();
				}
			});
		};

		function disableCheckbox() {
			vm.showCheckbox = false;
		}

		function enableCheckbox() {
			vm.showCheckbox = true;
		}

		vm.updateActionOptionsText = function() {
			if (vm.currentCommandLabel == 'AddActivity') {
				vm.actionOptions[0].Name = vm.actionOptions[0].Name.replace('{0}', $translate.instant('AddActivity'));
			}

			if (vm.currentCommandLabel == 'MoveActivity') {
				vm.actionOptions[0].Name = vm.actionOptions[0].Name.replace('{0}', $translate.instant('MoveActivity'));
				vm.actionOptions.pop();
			}

			if (vm.currentCommandLabel == 'AddPersonalActivity') {
				vm.actionOptions[0].Name = vm.actionOptions[0].Name.replace('{0}', $translate.instant('AddPersonalActivity'));
			}

			vm.currentActionOption = vm.actionOptions[0].Name;
		};

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

		vm.applyCommandFix = function() {
			vm.actionOptions.forEach(function(option) {
				if (option.Name == vm.currentActionOption) {
					option.BeforeAction && option.BeforeAction();
				}
			});
			CommandCheckService.completeCommandCheck();
		};

		vm.getOverlappedLayersForAgent = function(personId) {
			var overlappedLayers, result = [];

			vm.overlappedLayers.forEach(function(agent) {
				if (agent.PersonId == personId)
					overlappedLayers = agent.OverlappedLayers;
			});

			overlappedLayers.forEach(function(overlappedLayer) {
				result.push(overlappedLayer.Name);
				result.push(moment(overlappedLayer.StartTime).format('YYYY-MM-DD HH:mm'));
				result.push(moment(overlappedLayer.EndTime).format('YYYY-MM-DD HH:mm'));
			});
			return result;
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
		};

		vm.init();
	}
})();