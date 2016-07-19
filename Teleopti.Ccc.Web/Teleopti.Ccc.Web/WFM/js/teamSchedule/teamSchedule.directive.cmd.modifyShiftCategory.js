(function(){
	"use strict";

	angular.module('wfm.teamSchedule').directive('modifyShiftCategory', modifyShiftCategoryDirective);


	modifyShiftCategoryCtrl.$inject = ['ShiftCategoryService', 'PersonSelection'];

	function modifyShiftCategoryCtrl(shiftCategorySvc, personSelectionSvc){
		var vm = this;

		vm.label = 'EditShiftCategory';
		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();

		vm.modifyShiftCategory = function(){
			
		};

		vm.init = function(){
			shiftCategorySvc.modifyShiftCategories().then(function(data){
				vm.shiftCategoriesList = data;
			});
		};

		vm.init();
	}
	
	function modifyShiftCategoryDirective(){
		return{
			restrict: 'E',
			controller: modifyShiftCategoryCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamschedule/html/modifyShiftCategory.tpl.html',
			require: ['^teamscheduleCommandContainer', 'modifyShiftCategory'],
			link: linkFn
		};
	}

	function linkFn(scope, elem, attrs, ctrls){
		var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

		scope.vm.selectedDate = containerCtrl.getDate;
	}

})();