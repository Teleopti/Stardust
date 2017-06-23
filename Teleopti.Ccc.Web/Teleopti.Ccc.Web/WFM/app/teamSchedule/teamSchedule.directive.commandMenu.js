"use strict";
(function () {
	angular.module('wfm.teamSchedule').directive('teamscheduleCommandMenu', teamscheduleCommandMenuDirective);

	function teamscheduleCommandMenuDirective() {
		return {
			restrict: 'E',
			scope: {
				validateWarningEnabled: '=?',
				triggerCommand: '&?'
			},
			controller: ['$scope', 'PersonSelection', 'ValidateRulesService','ShortCuts', 'keyCodes', 'teamsPermissions', 'teamsToggles',  teamscheduleCommandMenuCtrl],
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/commandMenu.tpl.html'
		};
	}

	function teamscheduleCommandMenuCtrl($scope, personSelectionSvc, validateRulesService, shortCuts, keyCodes, teamsPermissions, teamsToggles) {
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
				label: "AddOvertimeActivity",
				shortcut: "Alt+O",
				keys: [[keyCodes.O], [keyCodes.ALT]],
				action: buildAction("AddOvertimeActivity", true),
				clickable: function () { return vm.canAddOvertime(); },
				visible: function () { return vm.canActiveAddOvertime(); }
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
				label: "MoveInvalidOverlappedActivity",
				shortcut: "Alt+I",
				keys: [[keyCodes.I], [keyCodes.ALT]],
				action: buildAction("MoveInvalidOverlappedActivity", false),
				clickable: function () { return vm.canMoveInvalidOverlappedActivity(); },
				visible: function () { return vm.canActiveMoveInvalidOverlappedActivity(); }
			},
			{
				label: "MoveShift",
				shortcut: "Alt+E",
				keys: [[keyCodes.E], [keyCodes.ALT]],
				action: buildAction("MoveShift", true),
				clickable: function () { return vm.canMoveShift(); },
				visible: function () { return vm.canActiveMoveShift(); }
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
				label: "EditShiftCategory",
				shortcut: "Alt+C",
				keys: [[keyCodes.C], [keyCodes.ALT]],
				action: buildAction("EditShiftCategory", true),
				clickable: function () { return vm.canModifyShiftCategory(); },
				visible: function () { return vm.canActiveModifyShiftCategory(); }
			},
			{
				label: "Undo",
				shortcut: "Alt+U",
				keys: [[keyCodes.U], [keyCodes.ALT]],
				action: buildAction("Undo", false),
				clickable: function () { return vm.canUndoSchedule(); },
				visible: function () { return vm.canActiveUndoScheduleCmd(); }
			}
		];


		vm.canActiveAddActivity = function () {
			return vm.permissions.HasAddingActivityPermission;
		};

		vm.canActiveAddPersonalActivity = function () {
			return vm.permissions.HasAddingPersonalActivityPermission;
		};

		vm.canActiveAddAbsence = function () {
			return vm.permissions.IsAddFullDayAbsenceAvailable || vm.permissions.IsAddIntradayAbsenceAvailable;
		};

		vm.canActiveAddOvertime = function() {
			return vm.toggles.AddOvertimeEnabled && vm.permissions.HasAddingOvertimeActivityPermission;
		};

		vm.canActiveMoveActivity = function () {
			return vm.toggles.MoveActivityEnabled && vm.permissions.HasMoveActivityPermission;
		};

		vm.canActiveMoveInvalidOverlappedActivity = function () {
			return vm.toggles.MoveInvalidOverlappedActivityEnabled && vm.permissions.HasMoveInvalidOverlappedActivityPermission;
		};

		vm.canActiveMoveShift = function() {
			return vm.toggles.MoveEntireShiftEnabled && vm.permissions.HasMoveActivityPermission;
		};

		vm.canActiveRemoveAbsence = function () {
			return vm.toggles.RemoveAbsenceEnabled && vm.permissions.IsRemoveAbsenceAvailable;
		};

		vm.canActiveRemoveActivity = function () {
			return (vm.toggles.RemoveActivityEnabled && vm.permissions.HasRemoveActivityPermission) || (vm.toggles.WfmTeamSchedule_RemoveOvertime_42481 && vm.permissions.HasRemoveOvertimePermission);
		};

		vm.canActiveSwapShifts = function () {
			return vm.toggles.SwapShiftEnabled && vm.permissions.IsSwapShiftsAvailable;
		};

		vm.canActiveModifyShiftCategory = function(){
			return vm.toggles.ModifyShiftCategoryEnabled && vm.permissions.HasEditShiftCategoryPermission;
		};

		vm.canActiveUndoScheduleCmd = function () {
			return vm.toggles.UndoScheduleEnabled;
		};

		vm.canMoveActivity = function () {
			return personSelectionSvc.isAnyAgentSelected() &&
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount === 0 &&
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount > 0;
		};

		vm.canMoveInvalidOverlappedActivity = function () {			
			return personSelectionSvc.anyAgentChecked() && vm.validateWarningEnabled;
		};

		vm.canMoveShift = function() {
			return personSelectionSvc.anyAgentChecked();
		};

		vm.canAddOvertime = function() {
			return personSelectionSvc.anyAgentChecked();
		};

		vm.canRemoveAbsence = function () {
			return personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount > 0;
		};

		vm.canRemoveActivity = function () {
			return personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount > 0;
		};

		vm.canModifyShiftCategory = function (){
			return vm.toggles.ModifyShiftCategoryEnabled && vm.permissions.HasEditShiftCategoryPermission && personSelectionSvc.anyAgentChecked();
		};

		vm.canUndoSchedule = function () {
			return personSelectionSvc.anyAgentChecked();
		};

		vm.activateCommandMenu = function(){
			return vm.canRemoveActivity() || vm.canRemoveAbsence() || personSelectionSvc.anyAgentChecked();
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
			vm.toggles = teamsToggles.all();
			vm.permissions = teamsPermissions.all();
		
			vm.isMenuVisible = function () {
				return vm.canActiveAddAbsence()
					|| vm.canActiveSwapShifts()
					|| vm.canActiveRemoveAbsence()
					|| vm.canActiveAddActivity()
					|| vm.canActiveRemoveActivity()
					|| vm.canActiveMoveActivity()
					|| vm.canActiveAddPersonalActivity()
					|| vm.canActiveModifyShiftCategory()
					|| vm.canActiveUndoScheduleCmd()
					|| vm.canActiveMoveInvalidOverlappedActivity()
				;
			};
			registerShortCuts();
		};

		vm.init();
	}
})();