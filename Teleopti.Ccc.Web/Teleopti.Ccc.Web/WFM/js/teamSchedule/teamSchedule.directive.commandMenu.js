﻿"use strict";
(function () {
	angular.module('wfm.teamSchedule').directive('teamscheduleCommandMenu', [teamscheduleCommandMenu]);

	function teamscheduleCommandMenu() {
		return {
			restrict: 'E',
			scope: {
				configurations: '=',
				triggerCommand: '&?'
			},
			controller: ['$scope', 'PersonSelection', 'ShortCuts', 'keyCodes', teamscheduleCommandMenuCtrl],
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/commandMenu.tpl.html'
		};
	}

	function teamscheduleCommandMenuCtrl($scope, personSelectionSvc, shortCuts, keyCodes) {
		var vm = this;

		function buildAction(label, openSidePanel) {
			return function () {
				if (vm.triggerCommand) {
					vm.triggerCommand({
						label: label,
						needToOpenSidePanel: openSidePanel
					});
				}
			}
		}

		vm.commands = [
			{
				label: "AddAbsence",
				shortcut: "Alt+A",
				keys: [[keyCodes.A], [keyCodes.ALT]],
				action: buildAction("AddAbsence", true),
				clickable: function () { return personSelectionSvc.anyAgentChecked(); },
				visible: function () { return vm.canActiveAddAbsence(); }
			},
			{
				label: "AddActivity",
				shortcut: "Alt+T",
				keys: [[keyCodes.T], [keyCodes.ALT]],
				action: buildAction("AddActivity", true),
				clickable: function () { return personSelectionSvc.anyAgentChecked(); },
				visible: function () { return vm.canActiveAddActivity(); }
			},
			{
				label: "AddPersonalActivity",
				shortcut: "Alt+P",
				keys: [[keyCodes.P], [keyCodes.ALT]],
				action: buildAction("AddPersonalActivity", true),
				clickable: function () { return personSelectionSvc.anyAgentChecked(); },
				visible: function () { return vm.canActiveAddPersonalActivity(); }
			},
			{
				label: "MoveActivity",
				shortcut: "Alt+M",
				keys: [[keyCodes.M], [keyCodes.ALT]],
				action: buildAction("MoveActivity", true),
				clickable: function () { return vm.canMoveActivity(); },
				visible: function () { return vm.canActiveMoveActivity(); }
			},
			{
				label: "SwapShifts",
				shortcut: "Alt+S",
				keys: [[keyCodes.S], [keyCodes.ALT]],
				action: buildAction("SwapShifts", false),
				clickable: function () { return personSelectionSvc.canSwapShifts(); },
				visible: function () { return vm.canActiveSwapShifts(); }
			},
			{
				label: "RemoveAbsence",
				shortcut: "Alt+R",
				keys: [[keyCodes.R], [keyCodes.ALT]],
				action: buildAction("RemoveAbsence", false),
				clickable: function () { return vm.canRemoveAbsence(); },
				visible: function () { return vm.canActiveRemoveAbsence(); }
			},
			{
				label: "RemoveActivity",
				shortcut: "Alt+X",
				keys: [[keyCodes.X], [keyCodes.ALT]],
				action: buildAction("RemoveActivity", false),
				clickable: function () { return vm.canRemoveActivity(); },
				visible: function () { return vm.canActiveRemoveActivity(); }
			},
			{
				label: "Backout",
				shortcut: "Alt+B",
				keys: [[keyCodes.B], [keyCodes.ALT]],
				action: buildAction("Backout", false),
				clickable: function () { return vm.canBackout(); },
				visible: function () { return vm.canActiveBackoutCmd(); }
			}
		];

		vm.canBackout = function () {
			return personSelectionSvc.anyAgentChecked();
		};

		vm.canActiveBackoutCmd = function () {
			return vm.toggles.BackoutPreviousScheduleEnabled;
		}

		vm.canActiveAddActivity = function () {
			return vm.toggles.AddActivityEnabled && vm.permissions.HasAddingActivityPermission;
		};

		vm.canActiveAddPersonalActivity = function () {
			return vm.toggles.AddPersonalActivityEnabled && vm.permissions.HasAddingPersonalActivityPermission;
		};

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


		function registerShortCuts() {
			vm.commands.forEach(function (cmd) {
				function wrappedAction() {
					if (cmd.clickable() && cmd.visible()) {
						cmd.action();
						$scope.$apply();
					}
				}

				shortCuts.registerKeySequence.apply(null, cmd.keys.concat([wrappedAction]));
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