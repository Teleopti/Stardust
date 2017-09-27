(function () {
	'use strict';
	angular.module("wfm.teamSchedule").component("staffingInfo", {
		controller: StaffingInfoController,
		controllerAs: 'vm',
		templateUrl: 'app/teamSchedule/html/staffingInfo.html',
		bindings: {
			selectedDate: '<',
			preselectedSkills: '<'
		}
	});

	StaffingInfoController.$inject = ["TeamScheduleSkillService", 'StaffingInfoService', 'TeamScheduleChartService'];

	function StaffingInfoController(SkillService, StaffingInfoService, ChartService) {
		var vm = this;
		vm.preselectedSkills = {};
		vm.skills = [];
		vm.skillGroups = [];

		vm.skills = SkillService.getAllSkills();
		vm.skillGroups = SkillService.getAllSkillGroups();
		vm.isLoading = false;
		vm.staffingDataAvailable = false;

		vm.useShrinkage = false;
		var selectedSkill = null;
		var selectedSkillGroup = null;

		vm.useShrinkageForStaffing = function () {
			vm.useShrinkage = !vm.useShrinkage;
			return generateChart();
		}

		vm.setSkill = function (selectedItem) {
			if (!angular.isDefined(selectedItem)) {
				vm.staffingDataAvailable = false;
				return;
			}
			var isSelectedSkillArea = selectedItem.hasOwnProperty('Skills')
			if (isSelectedSkillArea) {
				selectedSkillGroup = selectedItem;
				selectedSkill = null;
			} else {
				selectedSkill = selectedItem;
				selectedSkillGroup = null;
			}
			generateChart();
		}

		function generateChart(skill, area) {
			vm.isLoading = true;
			var query = StaffingInfoService.getStaffingByDate(selectedSkill, selectedSkillGroup, vm.selectedDate, vm.useShrinkage);
			query.then(function (result) {
				vm.isLoading = false;
				if (staffingPrecheck(result.DataSeries)) {
					var staffingData = ChartService.prepareStaffingData(result);
					generateChartForView(staffingData);
				}
			});
		}

		function staffingPrecheck(data) {
			if (!angular.equals(data, {}) && data != null) {
				if (data.Time.length > 0 && data.ScheduledStaffing && data.ForecastedStaffing) {
					vm.staffingDataAvailable = true;
				}
			} else {
				vm.staffingDataAvailable = false;
			}
			return vm.staffingDataAvailable;
		}

		function generateChartForView(data) {
			c3.generate(ChartService.staffingChartConfig(data));
		}
	}
})();