﻿(function () {
	'use strict';
	angular.module("wfm.teamSchedule").component("staffingInfo", {
		controller: StaffingInfoController,
		controllerAs: 'vm',
		templateUrl: 'app/teamSchedule/html/staffingInfo.html',
		bindings: {
			selectedDate: '<',
			preselectedSkills: '<',
			useShrinkage: '<',
			chartHeight: '<',
			onSelectedSkillChanged: '&',
			onUseShrinkageChanged: '&'
		}
	});

	StaffingInfoController.$inject = ['$scope', '$document', '$timeout', '$q', 'TeamScheduleSkillService', 'StaffingInfoService', 'TeamScheduleChartService', 'teamsToggles', 'skillIconService','serviceDateFormatHelper'];

	function StaffingInfoController($scope, $document, $timeout, $q, SkillService, StaffingInfoService, ChartService, teamsToggles, skillIconService, serviceDateFormatHelper) {
		var vm = this;

		vm.skills = [];
		vm.skillGroups = [];

		vm.isLoading = false;
		var staffingDataAvailable = false;
		vm.loadedSkillsData = false;
		vm.selectedSkillGroup = null;
		vm.toggleShowSkillsForSelectedSkillGroup = teamsToggles.all().WfmTeamSchedule_ShowSkillsForSelectedSkillGroupInStaffingInfo_47202;
		var selectedSkill = null;


		var chart = null;
		vm.$onInit = function () {
			vm.preselectedSkills = vm.preselectedSkills || {};
			vm.useShrinkage = vm.useShrinkage || false;
			loadSkillsAndSkillGroups().then(function () {
				vm.loadedSkillsData = true;
				loadChartForPreselection();
			});
		}

		vm.$onChanges = function (changeObj) {
			if (staffingDataAvailable && changeObj.chartHeight && changeObj.chartHeight.currentValue !== changeObj.chartHeight.previousValue) {
				chart.resize({ height: changeObj.chartHeight.currentValue });
			}
		};

		vm.useShrinkageForStaffing = function () {
			vm.useShrinkage = !vm.useShrinkage;
			var useShrinkageChanged;
			vm.onUseShrinkageChanged && (useShrinkageChanged = vm.onUseShrinkageChanged()) && useShrinkageChanged(vm.useShrinkage);
			return generateChart();
		}
		vm.showStaffingInfo = function () {
			return staffingDataAvailable && (!!selectedSkill || !!vm.selectedSkillGroup);
		}

		vm.setSkill = function (selectedItem) {
			if (!vm.loadedSkillsData || 
				(selectedItem && ((!!vm.selectedSkillGroup && vm.selectedSkillGroup.Id == selectedItem.Id)
					|| (!!selectedSkill && selectedSkill.Id == selectedItem.Id))))
				return;
			selectedSkill = null;
			vm.selectedSkillGroup = null;
			if (!!selectedItem) {
				var isSelectedSkillArea = selectedItem.hasOwnProperty('Skills');
				if (isSelectedSkillArea) {
					vm.selectedSkillGroup = selectedItem;
				} else {
					selectedSkill = selectedItem;
				}
				generateChart();
			}

			vm.onSelectedSkillChanged && vm.onSelectedSkillChanged({skill:selectedSkill, skillGroup: vm.selectedSkillGroup});
		}

		vm.getSkillIcon = skillIconService.get;

		$scope.$on('teamSchedule.dateChanged',
			function () {
				generateChart();
			});
		$scope.$on('teamSchedule.command.scheduleChangedApplied',
			function () {
				generateChart();
			});

		vm.toggleSkills = function () {

			var $skillsContent = angular.element($document[0].querySelector('.skills-content'));
			if ($skillsContent.hasClass('nooverflow') && $skillsContent[0].scrollWidth > $skillsContent[0].offsetWidth) {
				$skillsContent.removeClass("nooverflow").addClass("wrap-all");
			} else {
				$skillsContent.addClass("nooverflow").removeClass('wrap-all');
			}
		}
		vm.isSkillGroupDetailToggleVisible = function () {
			var $skillsContent = angular.element($document[0].querySelector('.skills-content'))[0];
			var totalWidthForChildren = 0;
			angular.forEach($skillsContent.children,
				function (c) {
					totalWidthForChildren += c.offsetWidth;
				});
			return totalWidthForChildren > $skillsContent.offsetWidth;
		}

		vm.isSkillsToggled = function () {
			var $skillsContent = angular.element($document[0].querySelector('.skills-content'));
			return !$skillsContent.hasClass('nooverflow');
		}

		function loadSkillsAndSkillGroups() {

			var getSkillsPromise = SkillService.getAllSkills().then(function (data) {
				vm.skills = data;
			});

			var getSkillsGroupPromise = SkillService.getAllSkillGroups().then(function (data) {
				vm.skillGroups = data.SkillAreas;
			});
			return $q.all([getSkillsPromise, getSkillsGroupPromise]);
		}

		function loadChartForPreselection() {
			if (!vm.preselectedSkills) return;
			var skillId = (vm.preselectedSkills.skillIds || [])[0];
			selectedSkill = !!skillId ? { Id: skillId } : undefined;

			vm.selectedSkillGroup = !!vm.preselectedSkills.skillAreaId ?
				vm.skillGroups.filter(function (s) {
					return s.Id === vm.preselectedSkills.skillAreaId;
				})[0] : undefined;
			generateChart();
		}

		function generateChart() {
			if (!selectedSkill && !vm.selectedSkillGroup) return;
			vm.isLoading = true;
			var query = StaffingInfoService.getStaffingByDate(selectedSkill, vm.selectedSkillGroup, serviceDateFormatHelper.getDateOnly(vm.selectedDate), vm.useShrinkage);
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
				vm.config.size = { height: vm.chartHeight };
				chart = c3.generate(vm.config);
			});
		}
	}
})();