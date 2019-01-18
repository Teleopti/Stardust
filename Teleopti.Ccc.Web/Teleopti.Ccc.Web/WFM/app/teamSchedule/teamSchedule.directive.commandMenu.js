(function (angular) {
	'use strict';
	angular.module('wfm.teamSchedule').directive('teamscheduleCommandMenu', teamscheduleCommandMenuDirective);

	function teamscheduleCommandMenuDirective() {
		return {
			restrict: 'E',
			scope: {
				validateWarningEnabled: '=?',
				triggerCommand: '&?',
				selectedDate: '<'
			},
			controller: [
				'$scope',
				'$translate',
				'PersonSelection',
				'ShortCuts',
				'keyCodes',
				'teamsPermissions',
				'serviceDateFormatHelper',
				'teamsBootstrapData',
				teamscheduleCommandMenuCtrl
			],
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/commandMenu.tpl.html'
		};
	}

	function teamscheduleCommandMenuCtrl(
		$scope,
		$translate,
		personSelectionSvc,
		shortCuts,
		keyCodes,
		teamsPermissions,
		serviceDateFormatHelper,
		teamsBootstrapData
	) {
		var vm = this;
		vm.permissions = teamsPermissions.all();

		function buildAction(label, openSidePanel) {
			return function () {
				if (vm.triggerCommand) {
					vm.triggerCommand({
						label: label,
						needToOpenSidePanel: openSidePanel
					});
				}
			};
		}

		vm.commands = [
			{
				label: 'AddAbsence',
				name: $translate.instant('AddAbsence'),
				shortcut: 'Alt+A',
				keys: [[keyCodes.A], [keyCodes.ALT]],
				action: buildAction('AddAbsence', true),
				clickable: function () {
					return personSelectionSvc.anyAgentChecked();
				},
				visible: function () {
					return vm.canActiveAddAbsence();
				}
			},
			{
				label: 'AddActivity',
				name: $translate.instant('AddActivity'),
				shortcut: 'Alt+T',
				keys: [[keyCodes.T], [keyCodes.ALT]],
				action: buildAction('AddActivity', true),
				clickable: function () {
					return personSelectionSvc.anyAgentChecked();
				},
				visible: function () {
					return vm.canActiveAddActivity();
				}
			},
			{
				label: 'AddPersonalActivity',
				name: $translate.instant('AddPersonalActivity'),
				shortcut: 'Alt+P',
				keys: [[keyCodes.P], [keyCodes.ALT]],
				action: buildAction('AddPersonalActivity', true),
				clickable: function () {
					return personSelectionSvc.anyAgentChecked();
				},
				visible: function () {
					return vm.canActiveAddPersonalActivity();
				}
			},
			{
				label: 'AddOvertimeActivity',
				name: $translate.instant('AddOvertimeActivity'),
				shortcut: 'Alt+O',
				keys: [[keyCodes.O], [keyCodes.ALT]],
				action: buildAction('AddOvertimeActivity', true),
				clickable: function () {
					return vm.canAddOvertime();
				},
				visible: function () {
					return vm.canActiveAddOvertime();
				}
			},
			{
				label: 'AddDayOff',
				name: $translate.instant('AddDayOff'),
				shortcut: 'Alt+F',
				keys: [[keyCodes.F], [keyCodes.ALT]],
				action: buildAction('AddDayOff', true),
				clickable: function () {
					return personSelectionSvc.anyAgentChecked();
				},
				visible: function () {
					return vm.canActiveAddDayOff();
				}
			},
			{
				label: 'MoveActivity',
				name: $translate.instant('MoveActivity'),
				shortcut: 'Alt+M',
				keys: [[keyCodes.M], [keyCodes.ALT]],
				action: buildAction('MoveActivity', true),
				clickable: function () {
					return vm.canMoveActivity();
				},
				visible: function () {
					return vm.canActiveMoveActivity();
				}
			},
			{
				label: 'MoveInvalidOverlappedActivity',
				name: $translate.instant('MoveInvalidOverlappedActivity'),
				shortcut: 'Alt+I',
				keys: [[keyCodes.I], [keyCodes.ALT]],
				action: buildAction('MoveInvalidOverlappedActivity', false),
				clickable: function () {
					return vm.canMoveInvalidOverlappedActivity();
				},
				visible: function () {
					return vm.canActiveMoveInvalidOverlappedActivity();
				}
			},
			{
				label: 'MoveShift',
				name: $translate.instant('MoveShift'),
				shortcut: 'Alt+E',
				keys: [[keyCodes.E], [keyCodes.ALT]],
				action: buildAction('MoveShift', true),
				clickable: function () {
					return vm.canMoveShift();
				},
				visible: function () {
					return vm.canActiveMoveShift();
				}
			},
			{
				label: 'SwapShifts',
				name: $translate.instant('SwapShifts'),
				shortcut: 'Alt+S',
				keys: [[keyCodes.S], [keyCodes.ALT]],
				action: buildAction('SwapShifts', false),
				clickable: function () {
					return personSelectionSvc.canSwapShifts();
				},
				visible: function () {
					return vm.canActiveSwapShifts();
				}
			},
			{
				label: 'RemoveAbsence',
				name: $translate.instant('RemoveAbsence'),
				shortcut: 'Alt+R',
				keys: [[keyCodes.R], [keyCodes.ALT]],
				action: buildAction('RemoveAbsence', false),
				clickable: function () {
					return vm.canRemoveAbsence();
				},
				visible: function () {
					return vm.canActiveRemoveAbsence();
				}
			},
			{
				label: 'RemoveActivity',
				name: $translate.instant('RemoveActivity'),
				shortcut: 'Alt+X',
				keys: [[keyCodes.X], [keyCodes.ALT]],
				action: buildAction('RemoveActivity', false),
				clickable: function () {
					return vm.canRemoveActivity();
				},
				visible: function () {
					return vm.canActiveRemoveActivity();
				}
			},
			{
				label: 'RemoveDayOff',
				name: $translate.instant('RemoveDayOff'),
				shortcut: 'Alt+B',
				keys: [[keyCodes.B], [keyCodes.ALT]],
				action: buildAction('RemoveDayOff', false),
				clickable: function () {
					return vm.canRemoveDayOff();
				},
				visible: function () {
					return vm.canActiveRemoveDayOff();
				}
			},
			{
				label: 'RemoveShift',
				name: $translate.instant('RemoveShift'),
				shortcut: 'Alt+K',
				keys: [[keyCodes.K], [keyCodes.ALT]],
				action: buildAction('RemoveShift', true),
				clickable: function () {
					return vm.canRemoveShift();
				},
				visible: function () {
					return vm.canActiveRemoveShift();
				}
			},
			{
				label: 'EditShiftCategory',
				name: $translate.instant('EditShiftCategory'),
				shortcut: 'Alt+C',
				keys: [[keyCodes.C], [keyCodes.ALT]],
				action: buildAction('EditShiftCategory', true),
				clickable: function () {
					return vm.canModifyShiftCategory();
				},
				visible: function () {
					return vm.canActiveModifyShiftCategory();
				}
			},
			{
				label: 'Undo',
				name: $translate.instant('Undo'),
				shortcut: 'Alt+U',
				keys: [[keyCodes.U], [keyCodes.ALT]],
				action: buildAction('Undo', false),
				clickable: function () {
					return vm.canUndoSchedule();
				},
				visible: function () {
					return true;
				}
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

		vm.canActiveAddOvertime = function () {
			return vm.permissions.HasAddingOvertimeActivityPermission;
		};

		vm.canActiveAddDayOff = function () {
			return vm.permissions.HasAddDayOffPermission;
		};

		vm.canActiveRemoveDayOff = function () {
			return vm.permissions.HasRemoveDayOffPermission;
		};

		vm.canActiveRemoveShift = function () {
			return vm.permissions.HasRemoveShiftPermission;
		};

		vm.canActiveMoveActivity = function () {
			return vm.permissions.HasMoveActivityPermission || vm.permissions.HasMoveOvertimePermission;
		};

		vm.canActiveMoveInvalidOverlappedActivity = function () {
			return vm.permissions.HasMoveInvalidOverlappedActivityPermission;
		};

		vm.canActiveMoveShift = function () {
			return vm.permissions.HasMoveActivityPermission;
		};

		vm.canActiveRemoveAbsence = function () {
			return vm.permissions.IsRemoveAbsenceAvailable;
		};

		vm.canActiveRemoveActivity = function () {
			return vm.permissions.HasRemoveActivityPermission || vm.permissions.HasRemoveOvertimePermission;
		};

		vm.canActiveSwapShifts = function () {
			return vm.permissions.IsSwapShiftsAvailable;
		};

		vm.canActiveModifyShiftCategory = function () {
			return vm.permissions.HasEditShiftCategoryPermission;
		};

		vm.canMoveActivity = function () {
			return (
				personSelectionSvc.isAnyAgentSelected() &&
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount === 0 &&
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount > 0
			);
		};

		vm.canMoveInvalidOverlappedActivity = function () {
			return personSelectionSvc.anyAgentChecked() && vm.validateWarningEnabled;
		};

		vm.canMoveShift = function () {
			return (
				personSelectionSvc.anyAgentChecked() &&
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount > 0
			);
		};

		vm.canAddOvertime = function () {
			return personSelectionSvc.anyAgentChecked();
		};

		vm.canRemoveAbsence = function () {
			return personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount > 0;
		};

		vm.canRemoveActivity = function () {
			return personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount > 0;
		};

		vm.canModifyShiftCategory = function () {
			return (
				vm.permissions.HasEditShiftCategoryPermission &&
				personSelectionSvc.anyAgentChecked() &&
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount > 0
			);
		};

		vm.canUndoSchedule = function () {
			return teamsBootstrapData.isScheduleAuditTrailEnabled() && personSelectionSvc.anyAgentChecked();
		};

		vm.canRemoveDayOff = function () {
			var selectedDate = serviceDateFormatHelper.getDateOnly(vm.selectedDate);
			var selectedPersonInfoList = personSelectionSvc.getCheckedPersonInfoList();
			if (selectedPersonInfoList.length === 0) return false;
			var selectedDayOffs = selectedPersonInfoList.filter(function (p) {
				var dayOffsOnCurrentDay = p.SelectedDayOffs.filter(function (d) {
					return d.Date === selectedDate;
				});
				return dayOffsOnCurrentDay.length > 0;
			});
			return selectedDayOffs.length > 0;
		};

		vm.canRemoveShift = function () {
			var selectedPersonInfoList = personSelectionSvc.getCheckedPersonInfoList();

			return (
				!!selectedPersonInfoList.length &&
				!!selectedPersonInfoList.filter(function (p) {
					return (
						p.Checked &&
						p.SelectedActivities &&
						p.SelectedActivities.filter(function (a) {
							return !a.isOvertime;
						}).length > 0
					);
				}).length
			);
		};

		vm.activateCommandMenu = function () {
			return vm.canRemoveActivity() || vm.canRemoveAbsence() || personSelectionSvc.anyAgentChecked();
		};


		vm.isMenuVisible = false;
		vm.toggleMenu = function () {
			vm.isMenuVisible = true;
		}

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
			registerShortCuts();
		};

		vm.init();
	}
})(angular);
