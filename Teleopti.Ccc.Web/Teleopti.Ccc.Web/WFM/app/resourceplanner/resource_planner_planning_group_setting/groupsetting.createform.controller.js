(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningGroupSettingEditController', Controller);

    Controller.$inject = ['$state', '$stateParams', '$translate', '$filter', 'NoticeService', 'PlanGroupSettingService', 'debounceService'];

    function Controller($state, $stateParams, $translate, $filter, NoticeService, PlanGroupSettingService, debounceService) {
        var vm = this;

        var filterId = $stateParams.filterId ? $stateParams.filterId : null;
        vm.default = $stateParams.isDefault ? $stateParams.isDefault : false;
        vm.isEdit = $stateParams.filterId ? true : false;
        vm.settingInfo = {
            BlockFinderType: 0,
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
            Default: vm.default,
            Filters: [],
            Name: $stateParams.isDefault ? $translate.instant('Default') : "",
            PlanningGroupId: $stateParams.groupId
        };
        vm.schedulingSettings = [
            { Id: "IndividualFlexible", Selected: true, Name: $translate.instant('IndividualFlexible') + " (" + $translate.instant('Default') + ")" },
            {
                Id: "BlockScheduling", Selected: false, Name: $translate.instant('BlockScheduling'),
                Types: [
                    { Id: "BlockFinderTypeBetweenDayOff", Selected: false, Code: 1, Name: $translate.instant('BlockFinderTypeBetweenDayOff') },
                    { Id: "BlockFinderTypeSchedulePeriod", Selected: false, Code: 2, Name: $translate.instant('BlockFinderTypeSchedulePeriod') }
                ], Options: [
                    { Id: "BlockSameShiftCategory", Selected: true },
                    { Id: "BlockSameStartTime", Selected: false },
                    { Id: "BlockSameShift", Selected: false },
                ]
            }
        ];
        vm.requestSent = false;
        vm.selectedItem = undefined;
        vm.selectedType = vm.schedulingSettings[1].Types[0];
        vm.selectedOptionName = vm.schedulingSettings[1].Options[0].Id;
        vm.searchString = undefined;
        vm.selectedSchedulingMethod = vm.schedulingSettings[0];
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
        vm.isValidBlockScheduling = isValidBlockScheduling;
        vm.selectResultItem = selectResultItem;
        vm.removeSelectedFilter = removeSelectedFilter;
        vm.cancelCreate = returnFromCreate;
        vm.selectSchedulingSetting = selectSchedulingSetting;
        vm.setBlockSchedulingType = setBlockSchedulingType;
        vm.setBlockSchedulingOption = setBlockSchedulingOption;
        vm.persist = persist;
        vm.filterOptions = filterOptions;

        checkIfEditDefaultRule();

        function checkIfEditDefaultRule() {
            if (!filterId)
                return vm.settingInfo;
            return PlanGroupSettingService.getSetting({ id: $stateParams.filterId })
                .$promise.then(function (result) {
                    vm.settingInfo.Name = result.Name;
                    vm.settingInfo.Filters = result.Filters;
                    vm.settingInfo.Priority = result.Priority;
                    vm.settingInfo.BlockFinderType = result.BlockFinderType;
                    vm.settingInfo.BlockSameShiftCategory = result.BlockSameShiftCategory;
                    vm.settingInfo.BlockSameStartTime = result.BlockSameStartTime;
                    vm.settingInfo.BlockSameShift = result.BlockSameShift;
                    vm.settingInfo.MinDayOffsPerWeek = result.MinDayOffsPerWeek;
                    vm.settingInfo.MaxDayOffsPerWeek = result.MaxDayOffsPerWeek;
                    vm.settingInfo.MinConsecutiveWorkdays = result.MinConsecutiveWorkdays;
                    vm.settingInfo.MaxConsecutiveWorkdays = result.MaxConsecutiveWorkdays;
                    vm.settingInfo.MinConsecutiveDayOffs = result.MinConsecutiveDayOffs;
                    vm.settingInfo.MaxConsecutiveDayOffs = result.MaxConsecutiveDayOffs;
                    vm.settingInfo.MinFullWeekendsOff = result.MinFullWeekendsOff;
                    vm.settingInfo.MaxFullWeekendsOff = result.MaxFullWeekendsOff;
                    vm.settingInfo.MinWeekendDaysOff = result.MinWeekendDaysOff;
                    vm.settingInfo.MaxWeekendDaysOff = result.MaxWeekendDaysOff;
					vm.settingInfo.PreferencePercent = result.PreferencePercent;
                    if (result.BlockFinderType > 0) {
                        return setBlockSchedulingIsSelected(result);
                    }
                });
        }

        function setBlockSchedulingIsSelected(result) {
            vm.schedulingSettings[1].Options = [
                { Id: "BlockSameShiftCategory", Selected: result.BlockSameShiftCategory },
                { Id: "BlockSameStartTime", Selected: result.BlockSameStartTime },
                { Id: "BlockSameShift", Selected: result.BlockSameShift },
            ];
            vm.selectedOptionName = vm.schedulingSettings[1].Options.find(function (option) {
                return option.Selected == true;
            }).Id;
            return setBlockSchedulingDetail(result.BlockFinderType);
        }

        function setBlockSchedulingDetail(id) {
            vm.schedulingSettings[0].Selected = false;
            vm.schedulingSettings[1].Selected = true;
            vm.selectedSchedulingMethod = vm.schedulingSettings[1];
            vm.selectedType = vm.schedulingSettings[1].Types.find(function (type) {
                return type.Code == id;
            });
        }

        function setBlockSchedulingType(type) {
            if (type)
                return vm.settingInfo.BlockFinderType = type.Code;
        }

        function setBlockSchedulingOption(index) {
            vm.schedulingSettings[1].Options.forEach(function (option, id) {
                if (id == index) {
                    option.Selected = true;
                    return vm.settingInfo[option.Id] = true;
                } else {
                    option.Selected = false;
                    return vm.settingInfo[option.Id] = false;
                }
            });
        }

        function selectSchedulingSetting() {
            if (vm.schedulingSettings[1].Selected == true) {
                vm.selectedSchedulingMethod = vm.schedulingSettings[1];
                vm.schedulingSettings[0].Selected = false;
                setBlockSchedulingSettingDataToDefault();
            } else {
                vm.selectedSchedulingMethod = vm.schedulingSettings[0];
                vm.schedulingSettings[0].Selected = true;
                clearBlockSchedulingSettingData();
            }
        }

        function clearBlockSchedulingSettingData() {
            vm.settingInfo.BlockFinderType = 0;
            vm.settingInfo.BlockSameShift = false;
            vm.settingInfo.BlockSameShiftCategory = false;
            vm.settingInfo.BlockSameStartTime = false;
        }

        function setBlockSchedulingSettingDataToDefault() {
            vm.settingInfo.BlockFinderType = 1;
            vm.settingInfo.BlockSameShift = false;
            vm.settingInfo.BlockSameShiftCategory = true;
            vm.settingInfo.BlockSameStartTime = false;
        }

        function filterOptions(text) {
            if (!!text)
                return $filter('filter')(vm.schedulingSettings[1].Types, text);
            return vm.schedulingSettings[1].Types;
        }

        function inputFilterData() {
            if (!!vm.searchString)
                return PlanGroupSettingService.getFilterData({ searchString: vm.searchString }).$promise.then(function (data) {
                    return vm.filterResults = removeSelectedFiltersInList(data, vm.settingInfo.Filters);
                });
            return [];
        }

        function removeSelectedFiltersInList(filters, selectedFilters) {
            if (selectedFilters.length == 0 || filters.length == 0)
                return filters;
            var result = angular.copy(filters);
            for (var i = filters.length - 1; i >= 0; i--) {
                angular.forEach(selectedFilters, function (selectedItem) {
                    if (filters[i].Id == selectedItem.Id) {
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
                isValidFullWeekEndDaysOff() &&
                isValidBlockScheduling();
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
            return vm.settingInfo.Filters.length > 0 || vm.default;
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

        function isValidBlockScheduling() {
            if (vm.schedulingSettings[0].Selected == true)
                return true;
            return !!vm.selectedType &&
                vm.schedulingSettings[1].Options.some(
                    function (option) {
                        return option.Selected == true;
                    });
        }

        function persist() {
            if (!vm.isValid())
                return;
            if (!vm.requestSent) {
                vm.requestSent = true;
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
})();
