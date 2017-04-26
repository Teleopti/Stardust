(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodSelectController', Controller);

    Controller.$inject = ['$state', '$stateParams', 'planningPeriodService'];

    function Controller($state, $stateParams, planningPeriodService) {
        var vm = this;
        var agentGroupId = $stateParams.groupId;
        vm.agentGroup = {};
        vm.suggestions = [];
        vm.planningPeriods = undefined;
        vm.originLastPp = undefined;
        vm.lastPp = undefined;
        vm.selectedSuggestion = {
            startDate: null,
            endDate: null
        };
        vm.startNextPlanningPeriod = startNextPlanningPeriod;
        vm.getLastPp = getLastPp;
        vm.selectPp = selectPp;
        vm.isEnddateChanged = isEnddateChanged;
        vm.changeEndDateForLastPp = changeEndDateForLastPp;
        vm.deleteLastPp = deleteLastPp;
        vm.getSugesstionsForFirstPp = getSugesstionsForFirstPp;
        vm.selectSuggestion = selectSuggestion;
        vm.createFirstPp = createFirstPp;
        vm.resetSelectedSuggestion = resetSelectedSuggestion;

        getAgentGroupbyId();
        getPlanningPeriod();

        function getAgentGroupbyId() {
            if (agentGroupId !== null) {
                var getAgentGroup = planningPeriodService.getAgentGroupById({ agentGroupId: agentGroupId });
                return getAgentGroup.$promise.then(function (data) {
                    vm.agentGroup = data;
                    return vm.agentGroup;
                });
            }
        }

        function getPlanningPeriod() {
            var query = planningPeriodService.getPlanningPeriodsForAgentGroup({ agentGroupId: agentGroupId });
            return query.$promise.then(function (data) {
                vm.planningPeriods = data;
                return vm.planningPeriods;
            });
        }

        function startNextPlanningPeriod() {
            var nextPlanningPeriod = planningPeriodService.nextPlanningPeriod({ agentGroupId: agentGroupId });
            return nextPlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods.push(data);
                return vm.planningPeriods;
            });
        }

        function selectPp(pp) { 
            $state.go('resourceplanner.oneagentgroup', { groupId: agentGroupId, ppId: pp.Id });
        }

        function getLastPp(p) {
            vm.originLastPp = angular.copy(p);
            vm.lastPp = {
                startDate: moment(p.StartDate).toDate(),
                endDate: moment(p.EndDate).toDate()
            };
            var elementResult = document.getElementsByClassName('date-range-start-date');
            elementResult[0].classList.add("pp-startDate-picker"); //css think mores
            return vm.lastPp;
        }

        function deleteLastPp() {
            var deletePlanningPeriod = planningPeriodService.deleteLastPlanningPeriod({ agentGroupId: agentGroupId });
            return deletePlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods = data;
                return vm.planningPeriods;
            });
        }

        function isEnddateChanged() {
            if (vm.lastPp !== undefined && vm.originLastPp !== undefined) {
                var origin = moment(vm.originLastPp.EndDate).toDate();
                var diff = moment(origin).diff(vm.lastPp.endDate, 'days');
                if (diff == 0) {
                    return false;
                }
                else {
                    return true;
                }
            }
        }

        function createFirstPp() { // chagned later?
            var nextPlanningPeriod = planningPeriodService.nextPlanningPeriod({ agentGroupId: agentGroupId });
            nextPlanningPeriod.$promise.then(function () {
                changeDateForPp(vm.selectedSuggestion);
            });
        }

        function changeDateForPp(pp) {
            var startDate = moment(pp.startDate).format('YYYY-MM-DD');
            var newEndDate = moment(pp.endDate).format('YYYY-MM-DD');
            var changeEndDateForLastPlanningPeriod = planningPeriodService.changeEndDateForLastPlanningPeriod({ agentGroupId: agentGroupId, startDate: startDate, endDate: newEndDate });
            return changeEndDateForLastPlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods = data;
                return vm.planningPeriods;
            });
        }

        function changeEndDateForLastPp(last) {
            var newEndDate = moment(last.endDate).format('YYYY-MM-DD');
            var changeEndDateForLastPlanningPeriod = planningPeriodService.changeEndDateForLastPlanningPeriod({ agentGroupId: agentGroupId, startDate: null, endDate: newEndDate });
            return changeEndDateForLastPlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods = data;
                return vm.planningPeriods;
            });
        }

        function getSugesstionsForFirstPp() {
            vm.suggestions = [];
            var suggestionsForFirstPp = planningPeriodService.getPlanningPeriodSuggestions({ agentGroupId: agentGroupId });
            return suggestionsForFirstPp.$promise.then(function (data) {
                vm.suggestions = data;
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
            vm.selectedSuggestion = {
                startDate: null,
                endDate: null
            };
        }
    }
})();
