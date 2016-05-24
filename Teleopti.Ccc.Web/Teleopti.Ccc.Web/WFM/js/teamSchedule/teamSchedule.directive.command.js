"use strict";
(function () {
	angular.module('wfm.teamSchedule').directive('teamscheduleCommand', [teamscheduleCommand]);

	function teamscheduleCommand() {
		return {
			restrict: 'E',
			scope: {
				configurations: '=',
				triggerCommand: '&?'
			},
			controller: ['$scope', '$mdSidenav', '$mdComponentRegistry', '$translate', 'PersonSelection', 'ShortCuts', 'keyCodes', 'NoticeService', teamscheduleCommandCtrl],
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/teamscheduleCommand.html'
		};
	}

	function teamscheduleCommandCtrl($scope, $mdSidenav, $mdComponentRegistry, $translate, personSelectionSvc, shortCuts, keyCodes, noticeService) {
		var vm = this;
		var parentVm = $scope.$parent.vm;

		vm.commands = [
			{
				label: "AddAbsence",
				shortcut: "Alt+A",
				panelName: 'report-absence',
				action: function () { vm.setCurrentCommand('AddAbsence'); parentVm.addAbsence(); },
				clickable: function () { return personSelectionSvc.anyAgentChecked(); },
				visible: function () { return vm.canActiveAddAbsence(); }
			},
			{
				label: "AddActivity",
				shortcut: "Alt+T",
				panelName: "add-activity",
				action: function () {
					vm.setCurrentCommand('AddActivity'); parentVm.addActivity();
				},
				clickable: function () { return personSelectionSvc.anyAgentChecked(); },
				visible: function () { return vm.canActiveAddActivity(); }
			},
			{
				label: "AddPersonalActivity",
				shortcut: "Alt+P",
				panelName: "add-personal-activity",
				action: function () {				
					if (vm.triggerCommand) {
						vm.triggerCommand({
							label: "AddPersonalActivity",
							needToOpenSidePanel: true
						});
					}
				},
				clickable: function () { return personSelectionSvc.anyAgentChecked(); },
				visible: function () { return vm.canActiveAddPersonalActivity(); },
				withCommandContainer: true
			},
			{
				label: "MoveActivity",
				shortcut: "Alt+M",
				panelName: "move-activity",
				action: function () { vm.moveActivityAction(); },
				clickable: function () { return vm.canMoveActivity(); },
				visible: function () { return vm.canActiveMoveActivity(); }
			},
			{
				label: "SwapShifts",
				shortcut: "Alt+S",
				panelName: "", // Leave empty if not creating a mdSidenav panel
				action: function () { vm.setCurrentCommand('SwapShifts'); parentVm.swapShifts(); },
				clickable: function () { return personSelectionSvc.canSwapShifts(); },
				visible: function () { return vm.canActiveSwapShifts(); }
			},
			{
				label: "RemoveAbsence",
				shortcut: "Alt+R",
				panelName: "", // Leave empty if not creating a mdSidenav panel
				action: function () { vm.setCurrentCommand('RemoveAbsence'); parentVm.confirmRemoveAbsence(); },
				clickable: function () { return vm.canRemoveAbsence(); },
				visible: function () { return vm.canActiveRemoveAbsence(); }
			},
			{
				label: "RemoveActivity",
				shortcut: "Alt+X",
				panelName: "", // Leave empty if not creating a mdSidenav panel
				action: function () {
					if (vm.triggerCommand) {
						vm.triggerCommand({
							label: "RemoveActivity",
							needToOpenSidePanel: false
						});
					}				
				},
				clickable: function () { return vm.canRemoveActivity(); },
				visible: function () { return vm.canActiveRemoveActivity(); },
				withCommandContainer: true
			}
		];

		vm.canActiveAddActivity = function () {
			return vm.toggles.AddActivityEnabled && vm.permissions.HasAddingActivityPermission;
		};

		vm.canActiveAddPersonalActivity = function() {
			return vm.toggles.AddPersonalActivityEnabled && vm.permissions.HasAddingPersonalActivityPermission;
		}

		vm.canActiveAddAbsence = function () {
			return vm.toggles.AbsenceReportingEnabled
				&& (vm.permissions.IsAddFullDayAbsenceAvailable || vm.permissions.IsAddIntradayAbsenceAvailable);
		};

		vm.canActiveMoveActivity = function () {
			return vm.toggles.MoveActivityEnabled && vm.permissions.HasMoveActivityPermission;
		};

		vm.canActiveRemoveAbsence = function () {
			return vm.toggles.RemoveAbsenceEnabled && vm.permissions.IsRemoveAbsenceAvailable;
		};

		vm.canActiveRemoveActivity = function () {
			return vm.toggles.RemoveActivityEnabled && vm.permissions.HasRemoveActivityPermission;
		};

		vm.canActiveSwapShifts = function () {
			return vm.toggles.SwapShiftEnabled && vm.permissions.IsSwapShiftsAvailable;
		};

		vm.canMoveActivity = function () {
			return personSelectionSvc.isAnyAgentSelected() &&
				!(personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount > 0) &&
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount > 0;
		};

		vm.canRemoveAbsence = function () {
			return personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount > 0;
		};

		vm.canRemoveActivity = function () {
			return personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount > 0;
		};

		vm.moveActivityAction = function () {
			var enableMove = true, message, notAllowNameList = '',
				personList = personSelectionSvc.getSelectedPersonInfoList();

			personList.forEach(function (agent) {
				if (agent.selectedActivities.length > 1) {
					enableMove = false;
					notAllowNameList += agent.name + ', ';
				}
			});
			if (enableMove) {
				vm.setCurrentCommand('MoveActivity');
				parentVm.moveActivity();
			} else {
				message = $translate.instant('CanNotMoveMultipleAcivitiesForAgent').replace('{0}', notAllowNameList.substring(0, notAllowNameList.length - 2));
				noticeService.error(message, null, true);
			}
		    $scope.$evalAsync();
		};

		vm.toggleCommandContainer = function(cmdName) {
			$mdSidenav.toggle(cmdName);
		};

		vm.toggleCommandState = function (menuName) {
			if (menuName !== undefined) {
				$mdSidenav(menuName).toggle();
			}
		};

		vm.getCurrentCommand = function (currentCmdName) {
			if (currentCmdName != undefined) {
				for (var i = 0; i < vm.commands.length; i++) {
					var cmd = vm.commands[i];
					if (cmd.label.toLowerCase() === currentCmdName.toLowerCase()) {
						return cmd;
					}
				};
			}
			return undefined;
		};

		$scope.$watch(function () { return vm.configurations.currentCommandName; }, function (newValue, oldValue) {
			newValue === null && vm.setCurrentCommand(newValue);
		});

		parentVm.toggleCurrentSidenav = function () { return false; };

		vm.setCurrentCommand = function (currentCmdName) {
			vm.configurations.currentCommandName = currentCmdName;

			var currentCmd = vm.getCurrentCommand(currentCmdName);
			if (currentCmd !== undefined) {
				vm.commands.forEach(function (cmd) {
					if (!cmd.withCommandContainer && cmd.panelName.length > 0 && cmd.panelName !== currentCmd.panelName && $mdSidenav(cmd.panelName).isOpen()) {
						$mdSidenav(cmd.panelName).close();
					}
				});
			} else {
				vm.commands.forEach(function (cmd) {
					if (!cmd.withCommandContainer &&  cmd.panelName.length > 0 && $mdSidenav(cmd.panelName).isOpen()) {
						$mdSidenav(cmd.panelName).close();
					}
				});
			}

			if (currentCmd != undefined && currentCmd.panelName != undefined && currentCmd.panelName.length > 0) {
				$mdComponentRegistry.when(currentCmd.panelName).then(function (sideNav) {
					parentVm.toggleCurrentSidenav = angular.bind(sideNav, sideNav.isOpen);
				});
				vm.toggleCommandState(currentCmd.panelName);
			}
		};

		function registerShortCuts() {
			shortCuts.registerKeySequence([keyCodes.A], [keyCodes.ALT], function () {
				if (!personSelectionSvc.anyAgentChecked() || !vm.canActiveAddAbsence()) return;
				vm.commands[0].action(); // Alt+A for add absence
			});
			shortCuts.registerKeySequence([keyCodes.T], [keyCodes.ALT], function () {
				if (!personSelectionSvc.anyAgentChecked() || !vm.canActiveAddActivity()) return;
				vm.commands[1].action(); // Alt+T for add activity
			});
			shortCuts.registerKeySequence([keyCodes.P], [keyCodes.ALT], function () {
				if (!personSelectionSvc.anyAgentChecked() || !vm.canActiveAddPersonalActivity()) return;
				vm.commands[2].action(); // Alt+P for add personal activity
			});
			shortCuts.registerKeySequence([keyCodes.M], [keyCodes.ALT], function () {
				if (!vm.canMoveActivity() || !vm.canActiveMoveActivity()) return;
				vm.commands[3].action(); // Alt+M for move activity
			});
			shortCuts.registerKeySequence([keyCodes.S], [keyCodes.ALT], function () {
				if (!personSelectionSvc.canSwapShifts() || !vm.canActiveSwapShifts()) return;
				vm.commands[4].action(); // Alt+S for swap shifts
			});
			shortCuts.registerKeySequence([keyCodes.R], [keyCodes.ALT], function () {
				if (!vm.canRemoveAbsence() || !vm.canActiveRemoveAbsence()) return;
				vm.commands[5].action(); // Alt+R for remove absence
			});
			shortCuts.registerKeySequence([keyCodes.X], [keyCodes.ALT], function () {
				if (!vm.canRemoveActivity() || !vm.canActiveRemoveActivity()) return;
				vm.commands[6].action(); // Alt+X for remove activity
			});
		}

		vm.init = function () {
			vm.toggles = vm.configurations.toggles;
			vm.permissions = vm.configurations.permissions;
			vm.isMenuVisible = function () {
				return vm.canActiveAddAbsence()
					|| vm.canActiveSwapShifts()
					|| vm.canActiveRemoveAbsence()
					|| vm.canActiveAddActivity()
					|| vm.canActiveRemoveActivity()
					|| vm.canActiveMoveActivity()
					|| vm.canActiveAddPersonalActivity()
				;
			};
			registerShortCuts();
		};

		vm.init();
	}
})();