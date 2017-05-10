(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodSelectController', Controller);

    Controller.$inject = ['$state', '$stateParams', '$translate', 'planningPeriodServiceNew'];

    function Controller($state, $stateParams, $translate, planningPeriodServiceNew) {
        var vm = this;
        var agentGroupId = $stateParams.groupId ? $stateParams.groupId : null;
        vm.agentGroup = {};
        vm.suggestions = [];
        vm.planningPeriods = [];
        vm.originLastPp = undefined;
        vm.lastPp = undefined;
        vm.selectedSuggestion = {
            startDate: null,
            endDate: null
        };
        vm.show = false;
        vm.displayButton = false;
        vm.openCreatePpModal = false;
        vm.modifyLastPpModal = false;
        vm.confirmDeletePpModal = false;
        vm.dateIsChanged = false;
        vm.textForDeletePp = '';
        vm.startNextPlanningPeriod = startNextPlanningPeriod;
        vm.getLastPp = getLastPp;
        vm.selectPp = selectPp;
        vm.isDateChanged = isDateChanged;
        vm.changeDateForLastPp = changeDateForLastPp;
        vm.deleteLastPp = deleteLastPp;
        vm.getSuggestionsForFirstPp = getSuggestionsForFirstPp;
        vm.selectSuggestion = selectSuggestion;
        vm.createFirstPp = createFirstPp;
        vm.resetSelectedSuggestion = resetSelectedSuggestion;
        vm.getPpInfo = getPpInfo;
        vm.isNonePp = isNonePp;

        getAgentGroupById();
        getPlanningPeriod();

        function getAgentGroupById() {
            if (agentGroupId !== null) {
                var getAgentGroup = planningPeriodServiceNew.getAgentGroupById({ agentGroupId: agentGroupId });
                return getAgentGroup.$promise.then(function (data) {
                    vm.agentGroup = data;
                    return vm.agentGroup;
                });
            }
        }

        function getPlanningPeriod() {
            if (agentGroupId !== null) {
                var query = planningPeriodServiceNew.getPlanningPeriodsForAgentGroup({ agentGroupId: agentGroupId });
                return query.$promise.then(function (data) {
                    vm.planningPeriods = data;
                    vm.displayButton = true;
                    return vm.planningPeriods;
                });
            }
        }

        function startNextPlanningPeriod() {
            if (agentGroupId !== null) {
                var nextPlanningPeriod = planningPeriodServiceNew.nextPlanningPeriod({ agentGroupId: agentGroupId });
                return nextPlanningPeriod.$promise.then(function (data) {
                    vm.planningPeriods.push(data);
                    return vm.planningPeriods;
                });
            }
        }

        function selectPp(pp) {
            if (agentGroupId !== null && pp.Id !== null) {
                $state.go('resourceplanner.planningperiodoverview', { groupId: agentGroupId, ppId: pp.Id });
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
                elementResult[0].classList.add("pp-startDate-picker");
            }
            return vm.lastPp;
        }

        function deleteLastPp() {
            if (agentGroupId !== null) {
                vm.confirmDeletePpModal = false;
                var deletePlanningPeriod = planningPeriodServiceNew.deleteLastPlanningPeriod({ agentGroupId: agentGroupId });
                return deletePlanningPeriod.$promise.then(function (data) {
                    vm.planningPeriods = data;
                    return vm.planningPeriods;
                });
            }
        }

        function isDateChanged () {
            var diffStartDate = moment(vm.originLastPp.startDate).diff(vm.lastPp.startDate, 'days');
            var diffEndDate = moment(vm.originLastPp.endDate).diff(vm.lastPp.endDate, 'days');
            if (diffStartDate !== 0 || diffEndDate !== 0) {
                return vm.dateIsChanged = true;
            }
            else {
                return vm.dateIsChanged = false;
            }
        }

        function createFirstPp() {
            if (agentGroupId !== null) {
                vm.openCreatePpModal=false; 
                vm.displayButton = false;
                var nextPlanningPeriod = planningPeriodServiceNew.nextPlanningPeriod({ agentGroupId: agentGroupId });
                nextPlanningPeriod.$promise.then(function () {
                    changeDateForPp(vm.selectedSuggestion);
                    vm.show = false;
                });
            }
        }

        function changeDateForPp(pp) {
            if (agentGroupId !== null) {
                var startDate = moment(pp.startDate).format('YYYY-MM-DD');
                var newEndDate = moment(pp.endDate).format('YYYY-MM-DD');
                var changeEndDateForLastPlanningPeriod = planningPeriodServiceNew.changeEndDateForLastPlanningPeriod({ agentGroupId: agentGroupId, startDate: startDate, endDate: newEndDate });
                return changeEndDateForLastPlanningPeriod.$promise.then(function (data) {
                    vm.planningPeriods = data;
                    vm.displayButton = true;
                    return vm.planningPeriods;
                });
            }
        }

        function changeDateForLastPp(pp) {
            if (vm.planningPeriods.length == 1) {
                changeDateForPp(pp);
            } else {
                changeDateForLastAndOnlyPp(pp);
            }
        }

        function changeEndDateForLastPp(pp) {
            if (agentGroupId !== null) {
                var newEndDate = moment(pp.endDate).format('YYYY-MM-DD');
                var changeEndDateForLastPlanningPeriod = planningPeriodServiceNew.changeEndDateForLastPlanningPeriod({ agentGroupId: agentGroupId, startDate: null, endDate: newEndDate });
                return changeEndDateForLastPlanningPeriod.$promise.then(function (data) {
                    vm.planningPeriods = data;
                    return vm.planningPeriods;
                });
            }
        }

        function getSuggestionsForFirstPp() {
            vm.openCreatePpModal = true;
            if (agentGroupId !== null) {
                vm.suggestions = [];
                var suggestionsForFirstPp = planningPeriodServiceNew.getPlanningPeriodSuggestions({ agentGroupId: agentGroupId });
                return suggestionsForFirstPp.$promise.then(function (data) {
                    vm.suggestions = data;
                    if (data.length > 0) {
                        selectSuggestion(vm.suggestions[0]);
                    }
                    vm.show = true;
                    return vm.suggestions;
                });
            }
        }

        function selectSuggestion(s) {
            vm.selectedSuggestion = {
                startDate: moment(s.StartDate).toDate(),
                endDate: moment(s.EndDate).toDate()
            };
            return vm.selectedSuggestion;
        }

        function resetSelectedSuggestion() {
            vm.openCreatePpModal=false; 
            vm.selectedSuggestion = {
                startDate: null,
                endDate: null
            };
            vm.show = false;
        }

        function getPpInfo(p) {
            vm.confirmDeletePpModal = true;
            vm.textForDeletePp = $translate.instant("AreYouSureYouWantToDeleteThePlanningPeriod")
                .replace("{0}", moment(p.StartDate).format('L'))
                .replace("{1}", moment(p.EndDate).format('L'));
        }

        function isNonePp() {
            if (vm.planningPeriods.length == 0) {
                return true;
            } else {
                return false;
            }
        }
    }
})();
