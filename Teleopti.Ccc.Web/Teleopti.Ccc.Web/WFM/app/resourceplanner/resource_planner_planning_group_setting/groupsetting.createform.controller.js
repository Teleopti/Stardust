(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningGroupSettingEditController', Controller);

    Controller.$inject = ['$state', '$stateParams', '$translate', 'NoticeService', 'PlanGroupSettingService', 'debounceService'];

    function Controller($state, $stateParams, $translate, NoticeService, PlanGroupSettingService, debounceService) {
        var vm = this;

        var maxHits = 100;
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
            var filters = PlanGroupSettingService.getFilterData({ searchString: vm.searchString }).$promise.then(function (data) {
                removeSelectedFiltersInList(data, vm.selectedResults);
                return vm.filterResults = data;
            });
            return filters;
        }

        function removeSelectedFiltersInList(filters, selectedFilters) {
            if (selectedFilters.length == 0)
                return;
            for (var i = filters.length - 1; i >= 0; i--) {
                angular.forEach(selectedFilters, function (selectedItem) {
                    if (filters[i].Id === selectedItem.Id) {
                        filters.splice(i, 1);
                    }
                });
            }
        }

        function clearInput() {
            vm.searchString = '';
            vm.results = [];
        }

        function isValid() {
            return vm.isValidDayOffsPerWeek() &&
                vm.isValidConsecDaysOff() &&
                vm.isValidFilters() &&
                vm.isValidName() &&
                vm.isValidBlockScheduling();
        }

        function isValidDayOffsPerWeek() {
            return isInteger(vm.dayOffsPerWeek.MinDayOffsPerWeek) &&
                isInteger(vm.dayOffsPerWeek.MaxDayOffsPerWeek) &&
                vm.dayOffsPerWeek.MaxDayOffsPerWeek <= 7 &&
                vm.dayOffsPerWeek.MinDayOffsPerWeek <= vm.dayOffsPerWeek.MaxDayOffsPerWeek;
        }

        function isValidConsecDaysOff() {
            return isInteger(vm.consecDaysOff.MinConsecDaysOff) &&
                isInteger(vm.consecDaysOff.MaxConsecDaysOff) &&
                vm.consecDaysOff.MinConsecDaysOff <= vm.consecDaysOff.MaxConsecDaysOff;
        }

        function isValidConsecWorkDays() {
            return isInteger(vm.consecWorkDays.MinConsecWorkDays) &&
                isInteger(vm.consecWorkDays.MaxConsecWorkDays) &&
                vm.consecWorkDays.MinConsecWorkDays <= vm.consecWorkDays.MaxConsecWorkDays;
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
                vm.blockSchedulingName = $translate.instant("BlockScheduling");
            } else {
                vm.blockSchedulingName = $translate.instant("IndividualFlexible") + " (" + $translate.instant("Default") + ")";
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
            if (type == null) {
                return vm.blockSchedulingSetting.BlockFinderType = 0;
            }
            return vm.blockSchedulingSetting.BlockFinderType = type.Code;
        }

        function setBlockSchedulingOption(option) {
            option.Selected = !option.Selected;
            vm.blockSchedulingSetting[option.Id] = option.Selected;
        }

        function getItemName(id) {
            return $translate.instant(id);
        }

        function persist() {
            if (!vm.isValid())
                return;
            if (!vm.requestSent) {
                vm.requestSent = true;
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
