(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('dayoffRuleCreateController', Controller);

    Controller.$inject = ['$state', '$stateParams', 'NoticeService', 'dayOffRuleService', 'debounceService'];

    function Controller($state, $stateParams, NoticeService, dayOffRuleService, debounceService) {
        var vm = this;

        var maxHits = 100;
        vm.name = "";
        vm.isEnabled = true;
        vm.selectedItem = undefined;
        vm.searchString = '';
        vm.results = [];
        vm.default = false;
        vm.selectedResults = [];
        vm.filterId = "";
        vm.inputFilterData = debounceService.debounce(inputFilterData, 250);
        vm.isValid = isValid;
        vm.isValidDayOffsPerWeek = isValidDayOffsPerWeek;
        vm.isValidConsecDaysOff = isValidConsecDaysOff;
        vm.clearInput = clearInput;
        vm.isValidConsecWorkDays = isValidConsecWorkDays;
        vm.isValidFilters = isValidFilters;
        vm.isValidName = isValidName;
        vm.selectResultItem = selectResultItem;
        vm.moreResultsExists = moreResultsExists;
        vm.noResultsExists = noResultsExists;
        vm.removeNode = removeNode;
        vm.persist = persist;
        vm.cancelCreate = returnFromCreate;
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

        checkIfEditDefaultRule();

        function checkIfEditDefaultRule() {
            if (Object.keys($stateParams.filterId).length > 0) {
                if ($stateParams.isDefault) {
                    vm.default = $stateParams.isDefault;
                    vm.name = "Default";
                    vm.filterId = $stateParams.filterId;
                    if (vm.filterId !== '00000000-0000-0000-0000-000000000000') {
                        dayOffRuleService.getDayOffRule({ id: $stateParams.filterId })
                            .$promise.then(function (result) {
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
                            });
                    }
                } else {
                    dayOffRuleService.getDayOffRule({ id: $stateParams.filterId })
                        .$promise.then(function (result) {
                            vm.name = result.Name;
                            vm.filterId = $stateParams.filterId;
                            vm.default = result.Default;
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
                        });
                }
            };
        }

        function inputFilterData() {
			if (vm.searchString !== '') {
				return dayOffRuleService.getFilterData({ searchString: vm.searchString }).$promise.then(function (data) {
					if (vm.selectedResults.length > 0) {
						for (var i = data.length - 1; i >= 0; i--) {
							angular.forEach(vm.selectedResults, function (selectedItem) {
								if (data[i].Id === selectedItem.Id) {
									data.splice(i, 1);
								}
							});
						}
					}
					return data;
				});
			} else {
				return [];
			}
		}

        function isValid() {
            return vm.isValidDayOffsPerWeek() &&
                vm.isValidConsecDaysOff() &&
                vm.isValidFilters() &&
                vm.isValidName();
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

        function clearInput() {
            vm.searchString = '';
            vm.results = [];
        }

        function isValidConsecWorkDays() {
            return isInteger(vm.consecWorkDays.MinConsecWorkDays) &&
                isInteger(vm.consecWorkDays.MaxConsecWorkDays) &&
                vm.consecWorkDays.MinConsecWorkDays <= vm.consecWorkDays.MaxConsecWorkDays;
        }

        function isInteger(value) {
            return typeof value === "number" &&
                isFinite(value) &&
                Math.floor(value) === value;
        }

        function isValidFilters() {
            return vm.selectedResults.length > 0 || vm.default;
        }

        function isValidName() {
            return vm.name.length > 0 && vm.name.length <= 100;
        }

        function isVaildUnit(item) {
            var check = true;
            vm.selectedResults.forEach(function (node) {
                if (node.Id === item.Id) {
                    check = false;
                };
            });
            return check;
        }

        function selectResultItem(item) {
            if (item === null) {
                return;
            }
            if (isVaildUnit(item)) {
                vm.selectedResults.push(item);
                vm.clearInput();
            } else {
                vm.clearInput();
                NoticeService.warning("Unit already exists", 5000, true);
            }
        }

        function moreResultsExists() {
            return vm.results.length >= maxHits;
        }

        function noResultsExists() {
            return vm.results.length === 0 && vm.searchString !== '';
        }

        function removeNode(node) {
            var p = vm.selectedResults.indexOf(node);
            vm.selectedResults.splice(p, 1);
        }

        function persist() {
            if (vm.isValid()) {
                vm.isEnabled = false;
                dayOffRuleService.saveDayOffRule({
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
                    AgentGroupId: $stateParams.groupId
                }).$promise.then(function () {
                    returnFromCreate();
                });
            }
        }

        function returnFromCreate() {
            if ($stateParams.groupId) {
                $state.go('resourceplanner.dayoffrulesOverview',
                    {
                        groupId: $stateParams.groupId
                    });
            } else {
                $state.go('resourceplanner.planningperiod',
                    {
                        id: $stateParams.periodId
                    });
            }
        }

    }
})();
