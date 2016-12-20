(function(){
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
				var containerCtrl = ctrls[0],
					selfCtrl = ctrls[1];

				scope.vm.containerCtrl = containerCtrl;

				scope.vm.selectedDate = containerCtrl.getDate;
				scope.vm.trackId = containerCtrl.getTrackId();
				scope.vm.getActionCb = containerCtrl.getActionCb;
				scope.vm.getCurrentTimezone = containerCtrl.getCurrentTimezone;

				scope.vm.init();
			}
		};
	}

	modifyShiftCategoryCtrl.$inject = ['ShiftCategoryService', 'PersonSelection', 'teamScheduleNotificationService'];

	function modifyShiftCategoryCtrl(shiftCategorySvc, personSelectionSvc, teamScheduleNotificationService){
		var vm = this;

		vm.label = 'EditShiftCategory';
		
		vm.shiftCategoriesLoaded = false;
		vm.selectedAgents = [];
		vm.invalidAgents = [];
		vm.init = init;
		vm.anyValidAgent = anyValidAgent;

		function init() {
			vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
			vm.invalidAgents = vm.selectedAgents.filter(function(agent) {
				return agent.Timezone.IanaId !== vm.getCurrentTimezone();
			});
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

		shiftCategorySvc.fetchShiftCategories().then(function(response){
			vm.shiftCategoriesList = response.data;
			if (angular.isArray(response.data)) {
				response.data.forEach(function (shiftCat) {
					var displayColorHex = shiftCat.DisplayColor.substring(1);
					shiftCat.ContrastColor = getContrastYIQ(displayColorHex);
				});
			}
			vm.shiftCategoriesLoaded = true;
		});

		vm.modifyShiftCategory = function () {

			var validAgents = vm.selectedAgents.filter(function(agent) {
				return vm.invalidAgents.indexOf(agent) < 0;
			});

			var requestData = {
				PersonIds: validAgents.map(function (agent) { return agent.PersonId }),
				Date: vm.selectedDate(),
				ShiftCategoryId: vm.selectedShiftCategoryId,
				TrackedCommandInfo: { TrackId: vm.trackId }
			}

			shiftCategorySvc.modifyShiftCategories(requestData).then(function(response){
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, requestData.PersonIds);
				}

				teamScheduleNotificationService.reportActionResult({
					success: 'SuccessfulMessageForEditingShiftCategory',
					warning: 'PartialSuccessMessageForEditingShiftCategory'
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
