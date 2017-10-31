(function () {
	'use strict';
	angular.module("wfm.teamSchedule").component("staffingInfo", {
		controller: StaffingInfoController,
		controllerAs: 'vm',
		templateUrl: 'app/teamSchedule/html/staffingInfo.html',
		bindings: {
			selectedDate: '<',
			preselectedSkills: '<',
			chartHeight: '<'
		}
	});

	StaffingInfoController.$inject = ['$scope', '$timeout', 'TeamScheduleSkillService', 'StaffingInfoService', 'TeamScheduleChartService'];

	function StaffingInfoController($scope, $timeout, SkillService, StaffingInfoService, ChartService) {
		var vm = this;
		vm.preselectedSkills = {};
		vm.skills = [];
		vm.skillGroups = [];

		vm.isLoading = false;
		var staffingDataAvailable = false;

		vm.useShrinkage = false;
		var selectedSkill = null;
		var selectedSkillGroup = null;
		
		vm.$onChanges = function (changeObj) {
			
		};

		vm.useShrinkageForStaffing = function () {
			vm.useShrinkage = !vm.useShrinkage;
			return generateChart();
		}
		vm.showStaffingInfo = function () {
			return staffingDataAvailable && (!!selectedSkill || !!selectedSkillGroup);
		}
		vm.setSkill = function (selectedItem) {
			selectedSkill = null;
			selectedSkillGroup = null;
			if (!angular.isDefined(selectedItem)) {
				return;
			}
			var isSelectedSkillArea = selectedItem.hasOwnProperty('Skills');
			if (isSelectedSkillArea) {
				selectedSkillGroup = selectedItem;
			} else {
				selectedSkill = selectedItem;
			}
			generateChart();
		}

		SkillService.getAllSkills().then(function (data) {
			vm.skills = data;
		});

		SkillService.getAllSkillGroups().then(function (data) {
			vm.skillGroups = data.SkillAreas;
		});

		$scope.$on('teamSchedule.dateChanged',
			function () {
				generateChart();
			});
		$scope.$on('teamSchedule.command.scheduleChangedApplied',
			function () {
				generateChart();
			});

		function generateChart() {
			if (!selectedSkill && !selectedSkillGroup) return;
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
			staffingDataAvailable = false;
			if (!angular.equals(data, {}) && data != null) {
				if (data.Time.length > 0 && data.ScheduledStaffing && data.ForecastedStaffing) {
					staffingDataAvailable = true;
				}
			}
			return staffingDataAvailable;
		}

		function generateChartForView(data) {
			$timeout(function () {
				vm.config = ChartService.staffingChartConfig(data);
				vm.config.size = {height:vm.chartHeight};
				c3.generate(vm.config);
			});
		}
	}
})();