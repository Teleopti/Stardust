(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('modifyShiftCategory', modifyShiftCategoryDirective);

	function modifyShiftCategoryDirective() {
		return {
			restrict: 'E',
			controller: modifyShiftCategoryCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/commands/teams.directive.cmd.modifyShiftCategory.html',
			require: ['^teamscheduleCommandContainer', 'modifyShiftCategory'],
			link: function linkFn(scope, elem, attrs, ctrls) {
				var containerCtrl = ctrls[0];

				scope.vm.containerCtrl = containerCtrl;

				scope.vm.selectedDate = containerCtrl.getDate;
				scope.vm.trackId = containerCtrl.getTrackId();
				scope.vm.getActionCb = containerCtrl.getActionCb;
				scope.vm.getCurrentTimezone = containerCtrl.getCurrentTimezone;

				scope.vm.init();
			}
		};
	}

	modifyShiftCategoryCtrl.$inject = ['$translate', 'ScheduleManagement', 'ShiftCategoryService', 'PersonSelection', 'teamScheduleNotificationService', 'colorUtils'];

	function modifyShiftCategoryCtrl($translate, scheduleMgmtSvc, shiftCategorySvc, personSelectionSvc, teamScheduleNotificationService, colorUtils) {
		var vm = this;

		vm.label = 'EditShiftCategory';
		vm.runningCommand = false;
		vm.shiftCategoriesLoaded = false;
		vm.selectedAgents = [];
		vm.invalidAgents = [];
		vm.init = init;
		vm.anyValidAgent = anyValidAgent;

		function init() {
			vm.invalidAgents = getInvalidAgents();
		}

		function getInvalidAgents() {
			var invalidAgents = {};
			vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();

			for (var i = 0; i < vm.selectedAgents.length; i++) {
				var agent = vm.selectedAgents[i];
				if (agent.Timezone.IanaId !== vm.getCurrentTimezone()) {
					invalidAgents[agent.PersonId] = {
						PersonId: agent.PersonId,
						Name: agent.Name
					}
					continue;
				}
				var agentSchedule = scheduleMgmtSvc.findPersonScheduleVmForPersonId(agent.PersonId);
				var hasDayOffSelected = agentSchedule.DayOffs.filter(function (d) {
					return d.Date === vm.selectedDate();
				}).length > 0;
				if (agentSchedule.IsFullDayAbsence || hasDayOffSelected) {
					invalidAgents[agent.PersonId] = {
						PersonId: agent.PersonId,
						Name: agent.Name
					}
				}
			}
			return Object.keys(invalidAgents).map(function (key) { return invalidAgents[key] });
		}

		function anyValidAgent() {
			return vm.selectedAgents.length > vm.invalidAgents.length;
		}

		function getContrastYIQ(hexcolor) {
			var r = parseInt(hexcolor.substr(0, 2), 16);
			var g = parseInt(hexcolor.substr(2, 2), 16);
			var b = parseInt(hexcolor.substr(4, 2), 16);
			var yiq = ((r * 299) + (g * 587) + (b * 114)) / 1000;
			return (yiq >= 128) ? 'black' : 'white';
		}

		shiftCategorySvc.fetchShiftCategories().then(function (response) {
			vm.shiftCategoriesList = response.data;
			if (angular.isArray(response.data)) {
				response.data.forEach(function (shiftCat) {
					shiftCat.TextColor = colorUtils.getTextColorBasedOnBackgroundColor(shiftCat.DisplayColor);
				});
			}
			vm.shiftCategoriesLoaded = true;
		});

		vm.modifyShiftCategory = function () {
			var invalidAgentIds = vm.invalidAgents.map(function (a) { return a.PersonId; });
			var validAgents = vm.selectedAgents.filter(function (agent) {
				return invalidAgentIds.indexOf(agent.PersonId) < 0;
			});

			var requestData = {
				PersonIds: validAgents.map(function (agent) { return agent.PersonId }),
				Date: vm.selectedDate(),
				ShiftCategoryId: vm.selectedShiftCategoryId,
				TrackedCommandInfo: { TrackId: vm.trackId }
			}

			vm.runningCommand = true;

			shiftCategorySvc.modifyShiftCategories(requestData).then(function (response) {
				vm.runningCommand = false;

				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, requestData.PersonIds);
				}

				teamScheduleNotificationService.reportActionResult({
					success: $translate.instant('SuccessfulMessageForEditingShiftCategory'),
					warning: $translate.instant('PartialSuccessMessageForEditingShiftCategory')
				}, vm.selectedAgents.map(function (agent) {
					return {
						PersonId: agent.PersonId,
						Name: agent.Name
					}
				}), response.data);
			});
		};
	}
})();
