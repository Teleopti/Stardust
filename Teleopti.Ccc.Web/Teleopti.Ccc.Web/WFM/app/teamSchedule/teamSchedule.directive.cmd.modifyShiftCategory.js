(function(){
	'use strict';

	angular.module('wfm.teamSchedule').directive('modifyShiftCategory', modifyShiftCategoryDirective);

	modifyShiftCategoryCtrl.$inject = ['ShiftCategoryService', 'PersonSelection', 'teamScheduleNotificationService'];

	function modifyShiftCategoryCtrl(shiftCategorySvc, personSelectionSvc, teamScheduleNotificationService){
		var vm = this;

		vm.label = 'EditShiftCategory';
		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
		vm.shiftCategoriesLoaded = false;

		function getContrast50(hexcolor) {
			return (parseInt(hexcolor, 16) > 0xffffff/2) ? 'black' : 'white';
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
			if (Array.isArray(response.data)) {
				response.data.forEach(function (shiftCat) {
					var displayColorHex = shiftCat.DisplayColor.substring(1);
					//shiftCat.ContrastColor = getContrast50(displayColorHex);
					shiftCat.ContrastColor = getContrastYIQ(displayColorHex);
				});
			}
			vm.shiftCategoriesLoaded = true;
		});

		vm.modifyShiftCategory = function(){
			var requestData = {
				PersonIds: vm.selectedAgents.map(function(agent) {return agent.PersonId}),
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

	function modifyShiftCategoryDirective(){
		return{
			restrict: 'E',
			controller: modifyShiftCategoryCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/modifyShiftCategory.tpl.html',
			require: ['^teamscheduleCommandContainer', 'modifyShiftCategory'],
			compile: function (tElement, tAttrs) {
				var tabindex = angular.isDefined(tAttrs.tabindex) ? tAttrs.tabindex : '0';
				function addTabindexTo() {
					angular.forEach(arguments, function (elements) {
						angular.forEach(elements, function (element) {
							element.setAttribute('tabIndex', tabindex);
						});
					});
				}
				addTabindexTo(
					tElement[0].querySelectorAll('.shift-category-selector'),
					tElement[0].querySelectorAll('#applyShiftCategory')
				);
				return linkFn;
			}
		};
	}

	function linkFn(scope, elem, attrs, ctrls){
		var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

		scope.vm.selectedDate = containerCtrl.getDate;
		scope.vm.trackId = containerCtrl.getTrackId();
		scope.vm.getActionCb = containerCtrl.getActionCb;

		scope.$on('teamSchedule.command.focus.default', function () {
			var focusTarget = elem[0].querySelector('.focus-default');
			if (focusTarget) angular.element(focusTarget).focus();
		});

		elem.removeAttr('tabindex');
	}

})();
