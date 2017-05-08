(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodSelectController', Controller);

    Controller.$inject = ['$state', '$stateParams', '$translate','planningPeriodService'];

    function Controller($state, $stateParams, $translate, planningPeriodService) {
        var vm = this;
        var agentGroupId = $stateParams.groupId;
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
        vm.textForDeletePp = '';
        vm.startNextPlanningPeriod = startNextPlanningPeriod;
        vm.getLastPp = getLastPp;
        vm.selectPp = selectPp;
        vm.isEndDateChanged = isEndDateChanged;
        vm.changeEndDateForLastPp = changeEndDateForLastPp;
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
                vm.displayButton = true;
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
            if (vm.planningPeriods.length > 1) {
                var elementResult = document.getElementsByClassName('date-range-start-date');
                elementResult[0].classList.add("pp-startDate-picker"); 
            }
            return vm.lastPp;
        }

        function deleteLastPp() {
            var deletePlanningPeriod = planningPeriodService.deleteLastPlanningPeriod({ agentGroupId: agentGroupId });
            return deletePlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods = data;
                return vm.planningPeriods;
            });
        }

        function isEndDateChanged() {
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

        function createFirstPp() { 
            vm.displayButton = false; 
            var nextPlanningPeriod = planningPeriodService.nextPlanningPeriod({ agentGroupId: agentGroupId });
            nextPlanningPeriod.$promise.then(function () {
                changeDateForPp(vm.selectedSuggestion);
                vm.show = false;
            });
        }

        function changeDateForPp(pp) {
			var startDate = moment(pp.startDate).format('YYYY-MM-DD');
			var newEndDate = moment(pp.endDate).format('YYYY-MM-DD');
            var changeEndDateForLastPlanningPeriod = planningPeriodService.changeEndDateForLastPlanningPeriod({ agentGroupId: agentGroupId, startDate: startDate, endDate: newEndDate });
            return changeEndDateForLastPlanningPeriod.$promise.then(function (data) {
                vm.planningPeriods = data;
                vm.displayButton = true;
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

        function getSuggestionsForFirstPp() {
            vm.suggestions = [];
            var suggestionsForFirstPp = planningPeriodService.getPlanningPeriodSuggestions({ agentGroupId: agentGroupId });
            return suggestionsForFirstPp.$promise.then(function (data) {
                vm.suggestions = data;
                if (data.length > 0) {
                    selectSuggestion(vm.suggestions[0]);
                }
                vm.show = true;
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
            vm.show = false;
        }

        function getPpInfo(p) {
            vm.textForDeletePp = $translate.instant("AreYouSureYouWantToDeleteThePlanningPeriod")
            .replace("{0}", moment(p.startDate).format('L'))
            .replace("{1}", moment(p.startDate).format('L'));
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
