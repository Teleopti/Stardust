(function(){
	"use strict";

	angular.module('wfm.teamSchedule').directive('modifyShiftCategory', modifyShiftCategoryDirective);

	modifyShiftCategoryCtrl.$inject = ['ShiftCategoryService', 'PersonSelection', 'teamScheduleNotificationService'];

	function modifyShiftCategoryCtrl(shiftCategorySvc, personSelectionSvc, teamScheduleNotificationService){
		var vm = this;

		vm.label = 'EditShiftCategory';
		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
		vm.shiftCategoriesLoaded = false;

		shiftCategorySvc.fetchShiftCategories().then(function(data){
			vm.shiftCategoriesList = data.data;
			vm.shiftCategoriesLoaded = true;
		});

		vm.modifyShiftCategory = function(){
			var requestData = {
				PersonIds: vm.selectedAgents.map(function(agent) {return agent.PersonId}),
				Date: vm.selectedDate(),
				ShiftCategoryId: vm.selectedShiftCategoryId,
				TrackedCommandInfo: vm.trackId
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
	
	function modifyShiftCategoryDirective(){
		return{
			restrict: 'E',
			controller: modifyShiftCategoryCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/modifyShiftCategory.tpl.html',
			require: ['^teamscheduleCommandContainer', 'modifyShiftCategory'],
			link: linkFn
		};
	}

	function linkFn(scope, elem, attrs, ctrls){
		var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

		scope.vm.selectedDate = containerCtrl.getDate;
		scope.vm.trackId = containerCtrl.getTrackId();
		scope.vm.getActionCb = containerCtrl.getActionCb;
	}

})();