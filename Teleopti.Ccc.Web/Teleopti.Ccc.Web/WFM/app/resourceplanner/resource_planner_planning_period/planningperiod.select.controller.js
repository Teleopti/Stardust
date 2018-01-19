(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningPeriodSelectController', Controller);

	Controller.$inject = ['$state', '$stateParams', '$translate', 'planningPeriodServiceNew', 'planningGroupInfo', 'planningPeriods', 'localeLanguageSortingService'];

	function Controller($state, $stateParams, $translate, planningPeriodServiceNew, planningGroupInfo, planningPeriods, localeLanguageSortingService) {
		var vm = this;
		var planningGroupId = $stateParams.groupId ? $stateParams.groupId : null;
		var saveLocalSelections;
		vm.planningGroup = planningGroupInfo;
		vm.planningPeriods = planningPeriods.sort(localeLanguageSortingService.localeSort('-EndDate'));
		vm.suggestions = [];
		vm.types = ['Week', 'Month'];
		vm.intervalRange = 0;
		vm.intervalType = vm.types[0];
		vm.originLastPp = undefined;
		vm.selectedSuggestion = {
			startDate: null,
			endDate: null
		}
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
		vm.isValidMaxWeeks = isValidMaxWeeks;
		vm.isValidMaxMonths = isValidMaxMonths;

		getSuggestionsForFirstPp();
		getLastPp();

		function isNonePp() {
			if (vm.planningPeriods.length == 0) {
				return true;
			} else {
				return false;
			}
		}

		function getSuggestionsForFirstPp() {
			if (planningGroupId == null || vm.planningPeriods.length > 0)
				return;
			var suggestionsForFirstPp = planningPeriodServiceNew.getPlanningPeriodSuggestions({ planningGroupId: planningGroupId });
			return suggestionsForFirstPp.$promise.then(function (data) {
				vm.suggestions = data;
				if (data.length > 0) {
					setSelectedDate(vm.suggestions[0]);
				}
				return vm.suggestions;
			});
		}

		function getLastPp() {
			if (vm.planningPeriods.length == 0)
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
			var newEndDate = moment(vm.selectedSuggestion.endDate).format('YYYY-MM-DD');
			var firstPp = planningPeriodServiceNew.firstPlanningPeriod({ planningGroupId: planningGroupId, startDate: startDate, endDate: newEndDate, schedulePeriodType: vm.intervalType, lengthOfThePeriodType: vm.intervalRange });
			return firstPp.$promise.then(function (data) {
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
				return "SetupIntervalLength"
			}
		}

		function autoUpdateEndDate() {
			if (vm.intervalRange == (0 || null)) {
				var startDate = vm.selectedSuggestion.startDate;
				vm.selectedSuggestion = {
					startDate: moment(startDate).toDate(),
					endDate: null
				};
				vm.selectedIsValid = checkSelectedIsValid();
				return;
			}
			var startDate = vm.selectedSuggestion.startDate;
			vm.selectedSuggestion = {
				startDate: moment(startDate).toDate(),
				endDate: moment(startDate).add(vm.intervalRange, vm.intervalType.toLowerCase()).subtract(1, 'day').toDate()
			};
			vm.selectedIsValid = checkSelectedIsValid();
			return;
		}

		function openModifyModal(type) {
			if (type == 'Day')
				vm.typeIsWrong = true;
			return vm.modifyLastPpModal = true;
		}

		function youAreGoingToChangeThisPlanningPeriodMessage() {
			if (!vm.originLastPp.startDate && !vm.selectedSuggestion.startDate)
				return;
			return vm.textForChangeThisPpMeg = $translate.instant("YouAreGoingToChangeThisPlanningPeriodFrom")
				.replace("{0}", moment(vm.selectedSuggestion.startDate).format('LL'))
				.replace("{1}", moment(vm.selectedSuggestion.endDate).format('LL'));
		}

		function isSelectedChanged() {
			vm.selectedIsValid = checkSelectedIsValid();
			autoUpdateEndDate();
		}

		function checkSelectedIsValid() {
			if (!!vm.selectedSuggestion.startDate && !!vm.selectedSuggestion.endDate && (isValidMaxWeeks() || isValidMaxMonths()))
				return true;
			return false
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
			var changeEndDateForLastPlanningPeriod = planningPeriodServiceNew.changeEndDateForLastPlanningPeriod({ planningGroupId: planningGroupId, startDate: startDate, schedulePeriodType: vm.intervalType, lengthOfThePeriodType: vm.intervalRange, endDate: newEndDate });
			return changeEndDateForLastPlanningPeriod.$promise.then(function (data) {
				vm.planningPeriods = data.sort(localeLanguageSortingService.localeSort('-EndDate'));
				vm.selectedIsValid = undefined;
				return getLastPp();
			});
		}

		function changeEndDateForLastPp(pp) {
			if (planningGroupId == null)
				return;
			vm.planningPeriods = [];
			var newEndDate = moment(pp.endDate).format('YYYY-MM-DD');
			var changeEndDateForLastPlanningPeriod = planningPeriodServiceNew.changeEndDateForLastPlanningPeriod({ planningGroupId: planningGroupId, startDate: null, schedulePeriodType: vm.intervalType, lengthOfThePeriodType: vm.intervalRange, endDate: newEndDate });
			return changeEndDateForLastPlanningPeriod.$promise.then(function (data) {
				vm.planningPeriods = data.sort(localeLanguageSortingService.localeSort('-EndDate'));
				vm.selectedIsValid = undefined;
				return getLastPp();
			});
		}

		function getPpInfo(p) {
			vm.confirmDeletePpModal = true;
			vm.textForDeletePp = $translate.instant("AreYouSureYouWantToDeleteThePlanningPeriod")
				.replace("{0}", moment(p.StartDate).format('L'))
				.replace("{1}", moment(p.EndDate).format('L'));
		}

		function deleteLastPp() {
			if (planningGroupId == null)
				return;
			vm.confirmDeletePpModal = false;
			var deletePlanningPeriod = planningPeriodServiceNew.deleteLastPlanningPeriod({ planningGroupId: planningGroupId });
			return deletePlanningPeriod.$promise.then(function (data) {
				return vm.planningPeriods = data.sort(localeLanguageSortingService.localeSort('-EndDate'));
			});
		}

		function startNextPlanningPeriod() {
			if (planningGroupId == null)
				return;
			var nextPlanningPeriod = planningPeriodServiceNew.nextPlanningPeriod({ planningGroupId: planningGroupId });
			return nextPlanningPeriod.$promise.then(function (data) {
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

		function isValidMaxWeeks() {
			if (vm.intervalType == 'Week') {
				return isValidDaysNumber(vm.intervalRange, 8);
			}
			return false;
		}

		function isValidMaxMonths() {
			if (vm.intervalType == 'Month') {
				return isValidDaysNumber(vm.intervalRange, 2);
			}
			return false;
		}
	}
})();
