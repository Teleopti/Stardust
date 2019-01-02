(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodValidationController', Controller)
        .directive('ppValidation', planningperiodValidationDirective);

    Controller.$inject = ['$stateParams', '$translate', '$timeout', 'planningPeriodServiceNew', 'localeLanguageSortingService'];

    function Controller($stateParams, $translate, $timeout, planningPeriodServiceNew, localeLanguageSortingService) {
        var vm = this;

        vm.valLoading = false;
        vm.getValidationByPpId = getValidationByPpId;
        vm.hasValidations = hasValidations;
        vm.color = {
            render: 'class',
			className: 'pp-validation-color'
        };

        getValidationByPpId();

        function getValidationByPpId() {
            if ($stateParams.ppId == null)
                return;
            vm.valLoading = true;
            planningPeriodServiceNew.getValidation({ id: $stateParams.ppId }).$promise.then(function (data) {

                vm.valData.preValidation = data.InvalidResources;
                vm.valLoading = false;
                vm.valNumber();
            });
        }

        function hasValidations() {
            if (!vm.valData.totalValNum == 0)
                return true;
        }
    }

    function planningperiodValidationDirective() {
		return {
			restrict: 'EA',
			scope: {
				valData: '=',
				valLoading: '=',
				valNumber: '&'
			},
			templateUrl: 'app/resourceplanner/planning_period/planningperiod.validation.html',
			controller: 'planningPeriodValidationController as vm',
			bindToController: true
		};
    }
})();
