(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningGroupSettingEditController', Controller)
		.directive('planningGroupSetting', planningGroupSettingDirective);

    Controller.$inject = ['$state', '$stateParams', '$translate', '$filter', 'NoticeService', 'PlanGroupSettingService', 'debounceService'];

    function Controller($state, $stateParams, $translate, $filter, NoticeService, PlanGroupSettingService, debounceService) {
        var vm = this;

        vm.isEdit = !!vm.settingInfo;
		vm.settingInfo.isValid = isValid;

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


        vm.teamGroupPageTypeOptions = [
            "Off",
            "Main"
        ];
		vm.teamGroupPageType = vm.teamGroupPageTypeOptions[0];

        vm.teamComparisonTypeOptions = [
            "SameShiftCategory",
        ];
		vm.teamComparisonType = vm.teamComparisonTypeOptions[0];

        vm.requestSent = false;
        vm.selectedItem = undefined;
        vm.searchString = undefined;
        vm.inputFilterData = debounceService.debounce(inputFilterData, 250);
        vm.clearInput = clearInput;
        vm.isValidDayOffsPerWeek = isValidDayOffsPerWeek;
        vm.isValidConsecDaysOff = isValidConsecDaysOff;
        vm.isValidConsecWorkDays = isValidConsecWorkDays;
        vm.isValidFullWeekEndsOff = isValidFullWeekEndsOff;
        vm.isValidFullWeekEndDaysOff = isValidFullWeekEndDaysOff;
        vm.isValidFilters = isValidFilters;
        vm.isValidName = isValidName;
        vm.selectResultItem = selectResultItem;
        vm.removeSelectedFilter = removeSelectedFilter;
        vm.blockFinderTypeOptionChanged = blockFinderTypeOptionChanged;
        vm.teamFinderTypeOptionChanged = teamFinderTypeOptionChanged;
        vm.blockComparisonTypeOptionChanged = blockComparisonTypeOptionChanged;
        vm.teamComparisonTypeOptionChanged = teamComparisonTypeOptionChanged;

        checkIfEditDefaultRule();

        function checkIfEditDefaultRule() {
            if (!vm.isEdit)
                return;
            
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

            vm.teamGroupPageType = vm.teamGroupPageTypeOptions[vm.teamSettings.GroupPageType];
            vm.teamComparisonType = vm.teamComparisonTypeOptions[vm.teamSettings.TeamSameType];
        }

        function inputFilterData() {
			return PlanGroupSettingService.getFilterData({ searchString: vm.searchString }).$promise.then(function (data) {
				return vm.filterResults = removeSelectedFiltersInList(data, vm.settingInfo.Filters);
			});
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
            vm.searchString = '';
        }

        function isValid() {
            return isValidDayOffsPerWeek() &&
                isValidConsecDaysOff() &&
                isValidFilters() &&
                isValidName() &&
                isValidFullWeekEndsOff() &&
                isValidFullWeekEndDaysOff();
        }
        
        vm.minChanged = function(min, max){
            if(vm.settingInfo[min] > vm.settingInfo[max]){
                vm.settingInfo[max] = vm.settingInfo[min];
            }
        };
        vm.maxChanged = function(min, max){
            if(vm.settingInfo[min] > vm.settingInfo[max]){
                vm.settingInfo[min] = vm.settingInfo[max];
            }
        };

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
        
        function blockFinderTypeOptionChanged(){
			vm.settingInfo.BlockFinderType = vm.blockFinderTypeOptions.indexOf(vm.blockFinderType);
		}
        
        function blockComparisonTypeOptionChanged(){
			for (var i = 0; i < vm.blockComparisonTypeOptions.length; i++) {
				vm.settingInfo[vm.blockComparisonTypeOptions[i]] = (vm.blockComparisonType===vm.blockComparisonTypeOptions[i]);
			}
		}

		
        function teamFinderTypeOptionChanged(){
            vm.teamSettings.GroupPageType = vm.teamGroupPageTypeOptions.indexOf(vm.teamGroupPageType);
        }
        
        function teamComparisonTypeOptionChanged(){
            vm.teamSettings.TeamSameType = vm.teamComparisonTypeOptions.indexOf(vm.teamComparisonType);
        }
    }

	function planningGroupSettingDirective() {
		return {
			restrict: 'EA',
			scope: {
				settingInfo: '=',
				preferencePercent: '=',
                teamSettings: '='
			},
			templateUrl: 'app/resourceplanner/planning_group_setting/groupsetting.createform.html',
			controller: 'planningGroupSettingEditController as vm',
			bindToController: true
		};
	}
})();
