(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodSelectController', Controller);

    Controller.$inject = ['$state', '$stateParams', '$translate', 'planningPeriodServiceNew', 'planningGroupInfo', 'planningPeriods', 'localeLanguageSortingService'];

    function Controller($state, $stateParams, $translate, planningPeriodServiceNew, planningGroupInfo, planningPeriods, localeLanguageSortingService) {
        var vm = this;
        var planningGroupId = $stateParams.groupId ? $stateParams.groupId : null;
        vm.planningGroup = planningGroupInfo;
        vm.planningPeriods = planningPeriods.sort(localeLanguageSortingService.localeSort('-EndDate'));
        vm.suggestions = [];
        vm.originLastPp = undefined;
        vm.lastPp = undefined;
        vm.selectedSuggestion = undefined;
        vm.openCreatePpModal = false;
        vm.modifyLastPpModal = false;
        vm.confirmDeletePpModal = false;
        vm.dateIsChanged = false;
        vm.selectedIsChanged = false;
        vm.textForDeletePp = '';
        vm.textForChangeThisPpMeg = '';
        vm.textForCreatePpMeg = '';
        vm.startNextPlanningPeriod = startNextPlanningPeriod;
        vm.getLastPp = getLastPp;
        vm.selectPp = selectPp;
        vm.isDateChanged = isDateChanged;
        vm.isSelectedChanged = isSelectedChanged;
        vm.changeDateForLastPp = changeDateForLastPp;
        vm.deleteLastPp = deleteLastPp;
        vm.selectSuggestion = selectSuggestion;
        vm.createFirstPp = createFirstPp;
        vm.resetSelectedSuggestion = resetSelectedSuggestion;
        vm.getPpInfo = getPpInfo;
        vm.isNonePp = isNonePp;

        getSuggestionsForFirstPp();
        getTotalDaysForEachPp();

        function getTotalDaysForEachPp() {
            vm.planningPeriods.forEach(function(p) {
                p.TotalDays = moment(p.EndDate).diff(p.StartDate, 'days');
            });
        }

        function youAreGoingToChangeThisPlanningPeriodMessage() {
            if (!vm.originLastPp.startDate && !vm.lastPp.startDate)
                return;
            return vm.textForChangeThisPpMeg = $translate.instant("YouAreGoingToChangeThisPlanningPeriodFrom")
                .replace("{0}", moment(vm.lastPp.startDate).format('LL'))
                .replace("{1}", moment(vm.lastPp.endDate).format('LL'));
        }

        function youAreGoingToCreateAPlanningPeriodMessage() {
            if (!vm.selectedSuggestion.startDate)
                return;
            return vm.textForCreatePpMeg = $translate.instant("YouAreGoingToCreateAPlanningPeriodFrom")
                .replace("{0}", moment(vm.selectedSuggestion.startDate).format('LL'))
                .replace("{1}", moment(vm.selectedSuggestion.endDate).format('LL'));
        }

        function isNonePp() {
            if (vm.planningPeriods.length == 0) {
                return true;
            } else {
                return false;
            }
        }

        function startNextPlanningPeriod() {
            if (planningGroupId == null)
                return;
            var nextPlanningPeriod = planningPeriodServiceNew.nextPlanningPeriod({ planningGroupId: planningGroupId });
            return nextPlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods.splice(0, 0, data);
                return vm.planningPeriods;
            });
        }

        function selectPp(pp) {
            if (planningGroupId !== null && pp.Id !== null) {
                $state.go('resourceplanner.planningperiodoverview', { groupId: planningGroupId, ppId: pp.Id });
            }
        }

        function getLastPp(p) {
            vm.modifyLastPpModal = true;
            vm.lastPp = {
                startDate: moment(p.StartDate).toDate(),
                endDate: moment(p.EndDate).toDate()
            };
            vm.originLastPp = angular.copy(vm.lastPp);
            if (vm.planningPeriods.length > 1) {
                var elementResult = document.getElementsByClassName('date-range-start-date');
                if (elementResult.length > 0) {
                    elementResult[0].classList.add("pp-startDate-picker");
                }
            }
            return vm.lastPp;
        }

        function deleteLastPp() {
            if (planningGroupId == null)
                return;
            vm.confirmDeletePpModal = false;
            var deletePlanningPeriod = planningPeriodServiceNew.deleteLastPlanningPeriod({ planningGroupId: planningGroupId });
            return deletePlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods = data;
                return vm.planningPeriods;
            });
        }

        function isDateChanged() {
            if (angular.isUndefined(vm.lastPp))
                return vm.dateIsChanged = false;
            var diffStartDate = moment(vm.originLastPp.startDate).diff(vm.lastPp.startDate, 'days');
            var diffEndDate = moment(vm.originLastPp.endDate).diff(vm.lastPp.endDate, 'days');
            if (diffStartDate !== 0 || diffEndDate !== 0) {
                youAreGoingToChangeThisPlanningPeriodMessage();
                return vm.dateIsChanged = true;
            }
        }

        function isSelectedChanged() {
            if (angular.isUndefined(vm.selectedSuggestion))
                return vm.selectedIsChanged = false;
            youAreGoingToCreateAPlanningPeriodMessage();
            return vm.selectedIsChanged = true;
        }

        function createFirstPp() {
            if (planningGroupId == null)
                return;
            vm.openCreatePpModal = false;
            var startDate = moment(vm.selectedSuggestion.startDate).format('YYYY-MM-DD');
            var newEndDate = moment(vm.selectedSuggestion.endDate).format('YYYY-MM-DD');
            var firstPp = planningPeriodServiceNew.firstPlanningPeriod({ planningGroupId: planningGroupId, startDate: startDate, endDate: newEndDate });
            return firstPp.$promise.then(function (data) {
                vm.planningPeriods.push(data);
                return vm.planningPeriods;
            });
        }

        function changeDateForLastPp(pp) {
            vm.modifyLastPpModal = false;
            if (vm.planningPeriods.length == 1) {
                changeDateForPp(pp);
            } else {
                changeEndDateForLastPp(pp);
            }
        }

        function changeDateForPp(pp) {
            if (planningGroupId == null)
                return;
            vm.planningPeriods = [];
            var startDate = moment(pp.startDate).format('YYYY-MM-DD');
            var newEndDate = moment(pp.endDate).format('YYYY-MM-DD');
            var changeEndDateForLastPlanningPeriod = planningPeriodServiceNew.changeEndDateForLastPlanningPeriod({ planningGroupId: planningGroupId, startDate: startDate, endDate: newEndDate });
            return changeEndDateForLastPlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods = data.sort(localeLanguageSortingService.localeSort('-EndDate'));
                vm.dateIsChanged = undefined;
                return vm.planningPeriods;
            });
        }

        function changeEndDateForLastPp(pp) {
            if (planningGroupId == null)
                return;
            vm.planningPeriods = [];
            var newEndDate = moment(pp.endDate).format('YYYY-MM-DD');
            var changeEndDateForLastPlanningPeriod = planningPeriodServiceNew.changeEndDateForLastPlanningPeriod({ planningGroupId: planningGroupId, startDate: null, endDate: newEndDate });
            return changeEndDateForLastPlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods = data.sort(localeLanguageSortingService.localeSort('-EndDate'));
                vm.dateIsChanged = undefined;
                return vm.planningPeriods;
            });
        }

        function getSuggestionsForFirstPp() {
            if (planningGroupId == null)
                return;
            if (vm.planningPeriods.length > 0)
                return;
            vm.suggestions = [];
            var suggestionsForFirstPp = planningPeriodServiceNew.getPlanningPeriodSuggestions({ planningGroupId: planningGroupId });
            return suggestionsForFirstPp.$promise.then(function (data) {
                vm.suggestions = data;
                if (data.length > 0) {
                    selectSuggestion(vm.suggestions[0]);
                }
                return vm.suggestions;
            });
        }

        function selectSuggestion(s) {
            vm.selectedSuggestion = {
                startDate: moment(s.StartDate).toDate(),
                endDate: moment(s.EndDate).toDate()
            };
            return vm.selectedSuggestion;
        }

        function resetSelectedSuggestion() {
            vm.openCreatePpModal = false;
            selectSuggestion(vm.suggestions[0]);
        }

        function getPpInfo(p) {
            vm.confirmDeletePpModal = true;
            vm.textForDeletePp = $translate.instant("AreYouSureYouWantToDeleteThePlanningPeriod")
                .replace("{0}", moment(p.StartDate).format('L'))
                .replace("{1}", moment(p.EndDate).format('L'));
        }
    }
})();
