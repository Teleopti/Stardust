(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningGroupSettingEditController', Controller);

    Controller.$inject = ['$state', '$stateParams', '$translate', 'NoticeService', 'PlanGroupSettingService', 'debounceService'];

    function Controller($state, $stateParams, $translate, NoticeService, PlanGroupSettingService, debounceService) {
        var vm = this;

        var maxHits = 100;
        var priorityNumber = null;
        vm.dayOffsPerWeek = {
            MinDayOffsPerWeek: 1,
            MaxDayOffsPerWeek: 3
        };
        vm.consecDaysOff = {
            MinConsecDaysOff: 1,
            MaxConsecDaysOff: 3
        };
        vm.consecWorkDays = {
            MinConsecWorkDays: 2,
            MaxConsecWorkDays: 6
        };
        vm.fullWeekEndsOff = {
            MinFullWeekEndsOff: 0,
            MaxFullWeekEndsOff: 8
        };
        vm.fullWeekEndDaysOff = {
            MinFullWeekEndDaysOff: 0,
            MaxFullWeekEndDaysOff: 16
        };
        vm.blockSchedulingSetting = {
            BlockFinderType: 0,
            BlockSameShift: false,
            BlockSameShiftCategory: false,
            BlockSameStartTime: false
        };
        vm.schedulingSettings = [
            { Id: "IndividualFlexible", Selected: true },
            { Id: "BlockScheduling", Selected: false }
            // {Name:"Team Scheduling", Selected: false}
        ];
        vm.blockSchedulingTypes = [
            { Id: "BlockFinderTypeBetweenDayOff", Code: 1 },
            { Id: "BlockFinderTypeSchedulePeriod", Code: 2 }
        ];
        vm.blockSchedulingOptions = [
            { Id: "BlockSameShiftCategory", Selected: false },
            { Id: "BlockSameStartTime", Selected: false },
            { Id: "BlockSameShift", Selected: false },
        ];
        vm.requestSent = false;
        vm.name = $stateParams.isDefault ? $translate.instant("Default") : "";
        vm.selectedItem = undefined;
        vm.selectedType = undefined;
        vm.searchString = '';
        vm.results = [];
        vm.filterId = $stateParams.filterId ? $stateParams.filterId : "";
        vm.default = $stateParams.isDefault ? $stateParams.isDefault : false;
        vm.isEdit = $stateParams.filterId ? true : false;
        vm.selectedResults = [];
        vm.blockSchedulingName = $translate.instant("IndividualFlexible") + " (" + $translate.instant("Default") + ")";

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
        vm.getItemName = getItemName;
        vm.persist = persist;

        checkIfEditDefaultRule();

        function checkIfEditDefaultRule() {
            if (vm.filterId == '')
                return;
            return PlanGroupSettingService.getSetting({ id: $stateParams.filterId })
                .$promise.then(function (result) {
                    vm.name = result.Name;
                    vm.selectedResults = result.Filters;
                    vm.dayOffsPerWeek = {
                        MinDayOffsPerWeek: result.MinDayOffsPerWeek,
                        MaxDayOffsPerWeek: result.MaxDayOffsPerWeek
                    };
                    vm.consecDaysOff = {
                        MinConsecDaysOff: result.MinConsecutiveDayOffs,
                        MaxConsecDaysOff: result.MaxConsecutiveDayOffs
                    };
                    vm.consecWorkDays = {
                        MinConsecWorkDays: result.MinConsecutiveWorkdays,
                        MaxConsecWorkDays: result.MaxConsecutiveWorkdays
                    }
                    vm.fullWeekEndsOff = {
                        MinFullWeekEndsOff: result.MinFullWeekendsOff,
                        MaxFullWeekEndsOff: result.MaxFullWeekendsOff
                    };
                    vm.fullWeekEndDaysOff = {
                        MinFullWeekEndDaysOff: result.MinWeekendDaysOff,
                        MaxFullWeekEndDaysOff: result.MaxWeekendDaysOff
                    };
                    priorityNumber = result.Priority;
                    if (result.BlockFinderType !== 0) {
                        vm.blockSchedulingSetting = {
                            BlockFinderType: result.BlockFinderType,
                            BlockSameShift: result.BlockSameShift,
                            BlockSameShiftCategory: result.BlockSameShiftCategory,
                            BlockSameStartTime: result.BlockSameStartTime
                        }
                        setBlockSchedulingSetting(result.BlockFinderType);
                    }
                });
        }

        function inputFilterData() {
            if (vm.searchString == '')
                return [];
            return PlanGroupSettingService.getFilterData({ searchString: vm.searchString }).$promise.then(function (data) {
                return vm.filterResults = removeSelectedFiltersInList(data, vm.selectedResults);
            });
        }

        function removeSelectedFiltersInList(filters, selectedFilters) {
			var result = angular.copy(filters);
			if (selectedFilters.length == 0 || filters.length == 0)
				return filters;
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
            vm.searchString = '';
            vm.results = [];
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
            return isInteger(vm.dayOffsPerWeek.MinDayOffsPerWeek) &&
                isInteger(vm.dayOffsPerWeek.MaxDayOffsPerWeek) &&
                vm.dayOffsPerWeek.MaxDayOffsPerWeek <= 7 &&
                vm.dayOffsPerWeek.MinDayOffsPerWeek <= vm.dayOffsPerWeek.MaxDayOffsPerWeek;
        }

        function isValidConsecDaysOff() {
            return isValidDaysNumber(vm.consecDaysOff.MinConsecDaysOff, vm.consecDaysOff.MaxConsecDaysOff);
        }

        function isValidConsecWorkDays() {
            return isValidDaysNumber(vm.consecWorkDays.MinConsecWorkDays, vm.consecWorkDays.MaxConsecWorkDays);
        }

        function isValidFullWeekEndsOff() {
            return isValidDaysNumber(vm.fullWeekEndsOff.MinFullWeekEndsOff, vm.fullWeekEndsOff.MaxFullWeekEndsOff);
        }

        function isValidFullWeekEndDaysOff() {
            return isValidDaysNumber(vm.fullWeekEndDaysOff.MinFullWeekEndDaysOff, vm.fullWeekEndDaysOff.MaxFullWeekEndDaysOff);
        }

        function isValidDaysNumber(min, max) {
            return isInteger(min) && isInteger(max) && min <= max;
        }

        function isInteger(value) {
            return angular.isNumber(value) &&
                isFinite(value) &&
                Math.floor(value) === value;
        }

        function isValidFilters() {
            return vm.selectedResults.length > 0 || vm.default;
        }

        function isValidName() {
            return vm.name.length > 0 && vm.name.length <= 100;
        }

        function isValidUnit(item) {
            var check = true;
            vm.selectedResults.forEach(function (node) {
                if (node.Id === item.Id) {
                    check = false;
                };
            });
            return check;
        }

        function isValidBlockScheduling() {
            return vm.selectedType !== null;
        }

        function selectResultItem(item) {
            if (item === null)
                return;
            if (isValidUnit(item)) {
                vm.selectedResults.push(item);
                vm.clearInput();
            } else {
                vm.clearInput();
                NoticeService.warning("Unit already exists", 5000, true);
            }
        }

        function removeSelectedFilter(node) {
            var p = vm.selectedResults.indexOf(node);
            vm.selectedResults.splice(p, 1);
        }

        function selectSchedulingSetting(index) {
            var item = vm.schedulingSettings[index];
            item.Selected = true;
            if (item.Id == "BlockScheduling") {
                vm.blockSchedulingName = $translate.instant(item.Id);
            }
            if (item.Id == "IndividualFlexible") {
                vm.blockSchedulingName = $translate.instant(item.Id) + " (" + $translate.instant("Default") + ")";
            }
            vm.schedulingSettings.forEach(function (item, id) {
                if (id != index)
                    item.Selected = false;
            });
        }

        function setBlockSchedulingSetting(typeId) {
            if (typeId > 0) {
                vm.schedulingSettings[1].Selected = true;
                vm.blockSchedulingTypes.forEach(function (item) {
                    if (item.Code == typeId)
                        vm.selectedType = item;
                });
                vm.blockSchedulingOptions.forEach(function (item) {
                    item.Selected = vm.blockSchedulingSetting[item.Id];
                });
                return setBlockSchedulingIsSelected();
            }
            return;
        }

        function setBlockSchedulingIsSelected() {
            vm.blockSchedulingName = $translate.instant("BlockScheduling");
            vm.schedulingSettings.forEach(function (item, id) {
                if (id !== 1)
                    item.Selected = false;
            });
        }

        function setBlockSchedulingType(type) {
            if (type == null)
                return;
            return vm.blockSchedulingSetting.BlockFinderType = type.Code;
        }

        function setBlockSchedulingOption(option) {
            if (option == null)
                return;
            option.Selected = !option.Selected;
            vm.blockSchedulingSetting[option.Id] = option.Selected;
        }

        function getItemName(id) {
            return $translate.instant(id);
        }

        function clearBlockSchedulingSettingData() {
            vm.blockSchedulingSetting = {
                BlockFinderType: 0,
                BlockSameShift: false,
                BlockSameShiftCategory: false,
                BlockSameStartTime: false
            };
        }

        function persist() {
            if (!vm.isValid())
                return;
            if (!vm.requestSent) {
                vm.requestSent = true;
                if (vm.schedulingSettings[0].Selected == true)
                    clearBlockSchedulingSettingData();
                PlanGroupSettingService.saveSetting({
                    BlockFinderType: vm.blockSchedulingSetting.BlockFinderType,
                    BlockSameShift: vm.blockSchedulingSetting.BlockSameShift,
                    BlockSameShiftCategory: vm.blockSchedulingSetting.BlockSameShiftCategory,
                    BlockSameStartTime: vm.blockSchedulingSetting.BlockSameStartTime,
                    MinDayOffsPerWeek: vm.dayOffsPerWeek.MinDayOffsPerWeek,
                    MaxDayOffsPerWeek: vm.dayOffsPerWeek.MaxDayOffsPerWeek,
                    MinConsecutiveWorkdays: vm.consecWorkDays.MinConsecWorkDays,
                    MaxConsecutiveWorkdays: vm.consecWorkDays.MaxConsecWorkDays,
                    MinConsecutiveDayOffs: vm.consecDaysOff.MinConsecDaysOff,
                    MaxConsecutiveDayOffs: vm.consecDaysOff.MaxConsecDaysOff,
                    MinFullWeekendsOff: vm.fullWeekEndsOff.MinFullWeekEndsOff,
                    MaxFullWeekendsOff: vm.fullWeekEndsOff.MaxFullWeekEndsOff,
                    MinWeekendDaysOff: vm.fullWeekEndDaysOff.MinFullWeekEndDaysOff,
                    MaxWeekendDaysOff: vm.fullWeekEndDaysOff.MaxFullWeekEndDaysOff,
                    Priority: priorityNumber,
                    Id: vm.filterId,
                    Name: vm.name,
                    Default: vm.default,
                    Filters: vm.selectedResults,
                    PlanningGroupId: $stateParams.groupId
                }).$promise.then(function () {
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
