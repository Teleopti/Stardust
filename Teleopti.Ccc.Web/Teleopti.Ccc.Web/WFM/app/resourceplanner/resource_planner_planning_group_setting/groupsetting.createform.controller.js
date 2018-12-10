(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningGroupSettingEditController', Controller)
		.directive('planningGroupSetting', planningGroupSettingDirective);

    Controller.$inject = ['$state', '$stateParams', '$translate', '$filter', 'NoticeService', 'PlanGroupSettingService', 'debounceService'];

    function Controller($state, $stateParams, $translate, $filter, NoticeService, PlanGroupSettingService, debounceService) {
        var vm = this;

        var filterId = vm.settingInfo ? vm.settingInfo.Id : null;
        vm.isEdit = !!filterId;
        if(!vm.isEdit){
			vm.settingInfo = {
				BlockSameShift: false,
				BlockSameShiftCategory: false,
				BlockSameStartTime: false,
				MinDayOffsPerWeek: 1,
				MaxDayOffsPerWeek: 3,
				MinConsecutiveWorkdays: 2,
				MaxConsecutiveWorkdays: 6,
				MinConsecutiveDayOffs: 1,
				MaxConsecutiveDayOffs: 3,
				MinFullWeekendsOff: 0,
				MaxFullWeekendsOff: 8,
				MinWeekendDaysOff: 0,
				MaxWeekendDaysOff: 16,
				Priority: null,
				Id: filterId,
				Filters: [],
				Default: false,
				Name: ""
			};
		}
        
        
        vm.blockFinderTypeOptions = [
			"Off",
			"BlockFinderTypeBetweenDayOff",
			"BlockFinderTypeSchedulePeriod"
		];
		vm.blockFinderType = vm.blockFinderTypeOptions[0];
        
        vm.blockComparisonTypeOptions = [
        	"BlockSameShiftCategory",
			"BlockSameStartTime",
			"BlockSameShift"
		];
		vm.blockComparisonType = vm.blockComparisonTypeOptions[0];

        vm.requestSent = false;
        vm.selectedItem = undefined;
        vm.searchString = undefined;
        vm.inputFilterData = debounceService.debounce(inputFilterData, 250);
        vm.clearInput = clearInput;
        vm.isValid = isValid;
        vm.isValidDayOffsPerWeek = isValidDayOffsPerWeek;
        vm.isValidConsecDaysOff = isValidConsecDaysOff;
        vm.isValidConsecWorkDays = isValidConsecWorkDays;
        vm.isValidFullWeekEndsOff = isValidFullWeekEndsOff;
        vm.isValidFullWeekEndDaysOff = isValidFullWeekEndDaysOff;
        vm.isValidFilters = isValidFilters;
        vm.isValidName = isValidName;
        vm.selectResultItem = selectResultItem;
        vm.removeSelectedFilter = removeSelectedFilter;
        vm.cancelCreate = returnFromCreate;
        vm.persist = persist;

        checkIfEditDefaultRule();

        function checkIfEditDefaultRule() {
            if (!vm.isEdit)
                return vm.settingInfo;
            
			vm.blockFinderType = vm.blockFinderTypeOptions[vm.settingInfo.BlockFinderType];
			if(vm.settingInfo.BlockSameShiftCategory){
				vm.blockComparisonType = vm.blockComparisonTypeOptions[0];
			}
			if(vm.settingInfo.BlockSameStartTime){
				vm.blockComparisonType = vm.blockComparisonTypeOptions[1];
			}
			if(vm.settingInfo.BlockSameShift){
				vm.blockComparisonType = vm.blockComparisonTypeOptions[2];
			}
        }

        function inputFilterData() {
            if (!!vm.searchString)
                return PlanGroupSettingService.getFilterData({ searchString: vm.searchString }).$promise.then(function (data) {
                    return vm.filterResults = removeSelectedFiltersInList(data, vm.settingInfo.Filters);
                });
            return [];
        }

        function removeSelectedFiltersInList(filters, selectedFilters) {
            if (selectedFilters.length === 0 || filters.length === 0)
                return filters;
            var result = angular.copy(filters);
            for (var i = filters.length - 1; i >= 0; i--) {
                angular.forEach(selectedFilters, function (selectedItem) {
                    if (filters[i].Id === selectedItem.Id) {
                        result.splice(i, 1);
                    }
                });
            }
            return result;
        }

        function clearInput() {
            vm.searchString = undefined;
        }

        function isValid() {
            return isValidDayOffsPerWeek() &&
                isValidConsecDaysOff() &&
                isValidFilters() &&
                isValidName() &&
                isValidFullWeekEndsOff() &&
                isValidFullWeekEndDaysOff()
        }

        function isValidDayOffsPerWeek() {
            return isValidDaysNumber(vm.settingInfo.MinDayOffsPerWeek, vm.settingInfo.MaxDayOffsPerWeek, 8);
        }

        function isValidConsecDaysOff() {
            return isValidDaysNumber(vm.settingInfo.MinConsecutiveDayOffs, vm.settingInfo.MaxConsecutiveDayOffs, 100);
        }

        function isValidConsecWorkDays() {
            return isValidDaysNumber(vm.settingInfo.MinConsecutiveWorkdays, vm.settingInfo.MaxConsecutiveWorkdays, 100);
        }

        function isValidFullWeekEndsOff() {
            return isValidDaysNumber(vm.settingInfo.MinFullWeekendsOff, vm.settingInfo.MaxFullWeekendsOff, 100);
        }

        function isValidFullWeekEndDaysOff() {
            return isValidDaysNumber(vm.settingInfo.MinWeekendDaysOff, vm.settingInfo.MaxWeekendDaysOff, 100);
        }

        function isValidDaysNumber(min, max, limit) {
            return isInteger(min) && isInteger(max) && min <= max && max < limit;
        }

        function isInteger(value) {
            return angular.isNumber(value) && isFinite(value) && Math.floor(value) === value;
        }

        function isValidFilters() {
            return vm.settingInfo.Filters.length > 0 || vm.settingInfo.Default;
        }

        function isValidName() {
            return vm.settingInfo.Name.length > 0 && vm.settingInfo.Name.length <= 100;
        }

        function isValidUnit(item) {
            return !vm.settingInfo.Filters.some(function(filter){ return filter.Id == item.Id; });
        }

        function selectResultItem(item) {
            if (!item)
                return;
            if (isValidUnit(item)) {
                vm.settingInfo.Filters.push(item);
                vm.clearInput();
            } else {
                vm.clearInput();
                NoticeService.warning("Unit already exists", 5000, true);
            }
        }

        function removeSelectedFilter(node) {
            var p = vm.settingInfo.Filters.indexOf(node);
            vm.settingInfo.Filters.splice(p, 1);
        }

        function persist() {
            if (!vm.isValid())
                return;
            if (!vm.requestSent) {
                vm.requestSent = true;
                vm.settingInfo.BlockFinderType = vm.blockFinderTypeOptions.indexOf(vm.blockFinderType);
				for (var i = 0; i < vm.blockComparisonTypeOptions.length; i++) {
					vm.settingInfo[vm.blockComparisonTypeOptions[i]] = (vm.blockComparisonType===vm.blockComparisonTypeOptions[i]);
				} 
                PlanGroupSettingService.saveSetting(vm.settingInfo).$promise.then(function () {
                    returnFromCreate();
                });
            }
        }

        function returnFromCreate() {
            if (!$stateParams.groupId)
                return;
            $state.go('resourceplanner.settingoverview', { groupId: $stateParams.groupId });
        }
    }

	function planningGroupSettingDirective() {
		return {
			restrict: 'EA',
			scope: {
				settingInfo: '='
			},
			templateUrl: 'app/resourceplanner/resource_planner_planning_group_setting/groupsetting.createform.html',
			controller: 'planningGroupSettingEditController as vm',
			bindToController: true
		};
	}
})();
