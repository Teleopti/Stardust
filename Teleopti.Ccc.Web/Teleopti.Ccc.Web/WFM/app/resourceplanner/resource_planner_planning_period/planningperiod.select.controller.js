(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningPeriodSelectController', Controller);

	Controller.$inject = ['$state', '$stateParams', '$translate', 'planningPeriodServiceNew', 'planningGroupInfo', 'planningPeriods', 'localeLanguageSortingService'];

	function Controller($state, $stateParams, $translate, planningPeriodServiceNew, planningGroupInfo, planningPeriods, localeLanguageSortingService) {
		var vm = this;
		var planningGroupId = $stateParams.groupId ? $stateParams.groupId : null;
		vm.planningGroup = planningGroupInfo;
        var periods = planningPeriods.sort(localeLanguageSortingService.localeSort('-EndDate'));
        angular.forEach(periods, function (period) {
            period.PeriodString = moment(period.StartDate).format('LL')+' - '+moment(period.EndDate).format('LL');
        });
        vm.planningPeriods = periods;
		vm.suggestions = [];
		vm.types = ['Week', 'Month'];
		vm.intervalRange = 0;
		vm.intervalType = vm.types[0];
		vm.originLastPp = undefined;
		vm.selectedSuggestion = {
			startDate: null,
			endDate: null
		};
		vm.openCreatePpModal = false;
		vm.modifyLastPpModal = false;
		vm.confirmDeletePpModal = false;
		vm.selectedIsValid = false;
		vm.typeIsWrong = false;
		vm.textForDeletePp = '';
		vm.textForChangeThisPpMeg = '';
		vm.textForCreatePpMeg = '';
		vm.isNonePp = isNonePp;
		vm.typeChanged = typeChanged;
		vm.intervalChanged = intervalChanged;
		vm.intervalLengthValid = intervalLengthValid;
		vm.startNextPlanningPeriod = startNextPlanningPeriod;
		vm.selectPp = selectPp;
		vm.getLastPp = getLastPp;
		vm.isSelectedChanged = isSelectedChanged;
		vm.setSelectedDate = setSelectedDate;
		vm.resetSelectedSuggestion = resetSelectedSuggestion;
		vm.createFirstPp = createFirstPp;
		vm.openModifyModal = openModifyModal;
		vm.changeDateForLastPp = changeDateForLastPp;
		vm.getPpInfo = getPpInfo;
		vm.deleteLastPp = deleteLastPp;
		vm.isValidPeriod = isValidPeriod;

		getSuggestionsForFirstPp();
		getLastPp();

		function isNonePp() {
			return vm.planningPeriods.length === 0;
		}

		function getSuggestionsForFirstPp() {
			if (planningGroupId == null || vm.planningPeriods.length > 0)
				return;
			var suggestionsForFirstPp = planningPeriodServiceNew.getPlanningPeriodSuggestions({ planningGroupId: planningGroupId });
			return suggestionsForFirstPp.$promise.then(function(data) {
				vm.suggestions = data;
				if (data.length > 0) {
					setSelectedDate(vm.suggestions[0]);
				}
				return vm.suggestions;
			});
		}

		function getLastPp() {
			if (vm.planningPeriods.length === 0)
				return;
			var lastPp = vm.planningPeriods[0];
			return vm.originLastPp = angular.copy(setSelectedDate(lastPp));
		}

		function setSelectedDate(s) {
			vm.selectedSuggestion = {
				startDate: moment(s.StartDate).toDate(),
				endDate: moment(s.EndDate).toDate()
			};
			vm.intervalRange = s.Number;
			vm.intervalType = s.PeriodType || s.Type;
			vm.selectedIsValid = checkSelectedIsValid();
			return vm.selectedSuggestion;
		}

		function resetSelectedSuggestion() {
			vm.openCreatePpModal = false;
			setSelectedDate(vm.suggestions[0]);
		}

		function createFirstPp() {
			if (planningGroupId == null)
				return;
			vm.openCreatePpModal = false;
			var startDate = moment(vm.selectedSuggestion.startDate).format('YYYY-MM-DD');
			var firstPp = planningPeriodServiceNew.firstPlanningPeriod({
				planningGroupId: planningGroupId,
				startDate: startDate,
				schedulePeriodType: vm.intervalType,
				lengthOfThePeriodType: vm.intervalRange
			});
			return firstPp.$promise.then(function(data) {
				vm.planningPeriods.push(data);
				return vm.planningPeriods;
			});
		}

		function typeChanged(output) {
			vm.intervalType = output;
			return autoUpdateEndDate();
		}

		function intervalChanged() {
			return autoUpdateEndDate();
		}

		function intervalLengthValid() {
			if (vm.selectedSuggestion.endDate == null) {
				return 'SetupIntervalLength';
			}
		}

		function autoUpdateEndDate() {
			var startDate = vm.selectedSuggestion.startDate;
			if (vm.intervalRange === (0 || null)) {
				vm.selectedSuggestion = {
					startDate: moment(startDate).toDate(),
					endDate: null
				};
				vm.selectedIsValid = checkSelectedIsValid();
				return;
			}
			if (startDate) {
				vm.selectedSuggestion = {
					startDate: moment(startDate).toDate(),
					endDate: moment(startDate).add(vm.intervalRange, vm.intervalType.toLowerCase()).subtract(1, 'day').toDate()
				};
			} else {
				vm.selectedSuggestion = {
					startDate: null,
					endDate: null
				};
			}
			vm.selectedIsValid = checkSelectedIsValid();

		}

		function openModifyModal(type) {
			if (type === 'Day')
				vm.typeIsWrong = true;
			return vm.modifyLastPpModal = true;
		}

		function isSelectedChanged() {
			vm.selectedIsValid = checkSelectedIsValid();
			autoUpdateEndDate();
		}

		function checkSelectedIsValid() {
			return !!(!!vm.selectedSuggestion.startDate && !!vm.selectedSuggestion.endDate && (isValidPeriod()));
		}

		function changeDateForLastPp(pp) {
			vm.modifyLastPpModal = false;
			if (vm.planningPeriods.length === 1) {
				changeDateForPp(moment(pp.startDate).format('YYYY-MM-DD'), moment(pp.endDate).format('YYYY-MM-DD'));
			} else {
				changeEndDateForLastPp(moment(pp.endDate).format('YYYY-MM-DD'));
			}
		}

		function changeDateForPp(startDate, endDate) {
			if (planningGroupId == null)
				return;
			vm.planningPeriods = [];
			var changeEndDateForLastPlanningPeriod = planningPeriodServiceNew.changeEndDateForLastPlanningPeriod({
				planningGroupId: planningGroupId,
				startDate: startDate,
				schedulePeriodType: vm.intervalType,
				lengthOfThePeriodType: vm.intervalRange,
				endDate: endDate
			});
			return changeEndDateForLastPlanningPeriod.$promise.then(function(data) {
                var periods = data.sort(localeLanguageSortingService.localeSort('-EndDate'));
                angular.forEach(periods, function (period) {
                    period.PeriodString = moment(period.StartDate).format('LL')+' - '+moment(period.EndDate).format('LL');
                });
                vm.planningPeriods = periods;
				vm.selectedIsValid = undefined;
				return getLastPp();
			});
		}

		function changeEndDateForLastPp(endDate) {
		    return changeDateForPp(null, endDate);
		}

		function getPpInfo(p) {
			vm.confirmDeletePpModal = true;
			vm.textForDeletePp = $translate.instant('AreYouSureYouWantToDeleteThePlanningPeriod')
				.replace('{0}', moment(p.StartDate).format('L'))
				.replace('{1}', moment(p.EndDate).format('L'));
		}

		function deleteLastPp() {
			if (planningGroupId == null)
				return;
			vm.confirmDeletePpModal = false;
			var deletePlanningPeriod = planningPeriodServiceNew.deleteLastPlanningPeriod({ planningGroupId: planningGroupId });
			return deletePlanningPeriod.$promise.then(function(data) {
			    var periods = data.sort(localeLanguageSortingService.localeSort('-EndDate'));
                angular.forEach(periods, function (period) {
                    period.PeriodString = moment(period.StartDate).format('LL')+' - '+moment(period.EndDate).format('LL');
                });
				return vm.planningPeriods = periods;
			});
		}

		function startNextPlanningPeriod() {
			if (planningGroupId == null)
				return;
			var nextPlanningPeriod = planningPeriodServiceNew.nextPlanningPeriod({ planningGroupId: planningGroupId });
			return nextPlanningPeriod.$promise.then(function(data) {
                data.PeriodString= moment(data.StartDate).format('LL')+' - '+moment(data.EndDate).format('LL');
				vm.planningPeriods.splice(0, 0, data);
				return getLastPp();
			});
		}

		function selectPp(pp) {
			if (planningGroupId !== null && pp.Id !== null) {
				$state.go('resourceplanner.planningperiodoverview', { groupId: planningGroupId, ppId: pp.Id });
			}
		}

		function isValidDaysNumber(value, limit) {
			return isInteger(value) && value <= limit && value > 0;
		}

		function isInteger(value) {
			return angular.isNumber(value) && isFinite(value) && Math.floor(value) === value;
		}

		function isValidPeriod() {
			if (vm.intervalType === 'Week') {
				return isValidDaysNumber(vm.intervalRange, 8);
			}
			if (vm.intervalType === 'Month') {
				return isValidDaysNumber(vm.intervalRange, 2);
			}
			return false;
		}
	}
})();
