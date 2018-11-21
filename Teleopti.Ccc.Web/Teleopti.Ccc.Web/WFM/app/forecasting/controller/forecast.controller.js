(function () {
	"use strict";

	angular
		.module("wfm.forecasting")
		.controller("ForecastRefactController", ForecastCtrl);

	ForecastCtrl.$inject = [
		"ForecastingService",
		"$state",
		"$translate",
		"skillIconService",
		"Toggle"
	];

	function ForecastCtrl(
		forecastingService,
		$state,
		$translate,
		skillIconService,
		toggleSvc
	) {
		var vm = this;

		vm.skills = [];
		vm.skilltypes = [];
		vm.goToModify = goToModify;
		vm.goToHistory = goToHistory;
		vm.getSkillIcon = skillIconService.get;

		function init() {
			vm.showHistory = toggleSvc.WFM_Forecast_Show_History_Data_76432;
			getAllSkills();
		}

		function getAllSkills() {
			vm.skills = [];
			forecastingService.skills.query().$promise.then(function (result) {
				result.Skills.forEach(function (skill) {
					skill.Workloads.forEach(function (workload) {
						var skillModel = {
							Workload: workload,
							SkillId: skill.Id,
							SkillType: {
								DoDisplayData: isSkillTypeSupported(skill.SkillType),
								SkillType: skill.SkillType
							},
							ChartId: "chart" + workload.Id
						};
						vm.skills.push(skillModel);

						if (!vm.skilltypes.includes(skillModel.SkillType.SkillType)) {
							vm.skilltypes.push(skillModel.SkillType.SkillType);
						}
					});
				});
				if (vm.skills.length < 1) {
					$state.go("forecast-no-skills");
				}
			});
		}

		function isSkillTypeSupported(skillType) {
			return skillType === "SkillTypeInboundTelephony";
		}

		function goToModify(skill) {
			sessionStorage.currentForecastWorkload = angular.toJson(skill);
			$state.go("forecast-modify", { workloadId: skill.Workload.Id });
		}

		function goToHistory(skill) {
			sessionStorage.currentForecastWorkload = angular.toJson(skill);
			$state.go("forecast-history", { workloadId: skill.Workload.Id });
		}

		init();
	}
})();
