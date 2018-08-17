(function() {
	'use strict';

	angular.module('wfm.forecasting').controller('r2ForecastRefactController', r2ForecastCtrl);

	r2ForecastCtrl.$inject = [
		'ForecastingService',
		'$state',
		'$translate',
		'NoticeService',
		'skillIconService',
		'Toggle'
	];

	function r2ForecastCtrl(forecastingService, $state, $translate, noticeSvc, skillIconService, toggleSvc) {
		var vm = this;

		vm.skills = [];
		vm.skilltypes = [];
		vm.goToModify = goToModify;
		vm.goToHistory = goToHistory;
		vm.getSkillIcon = skillIconService.get;

		function init() {
			vm.showHistory = toggleSvc.WFM_Forecast_Show_History_Data_76432;
			setReleaseNotification();
			getAllSkills();
		}

		function setReleaseNotification() {
			var message = $translate
				.instant('WFMReleaseNotificationWithoutOldModuleLink')
				.replace('{0}', $translate.instant('Forecast'))
				.replace('{1}', '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx" target="_blank">')
				.replace('{2}', '</a>');
			noticeSvc.info(message, null, true);
		}

		function getAllSkills() {
			vm.skills = [];
			forecastingService.skills.query().$promise.then(function(result) {
				result.Skills.forEach(function(skill) {
					skill.Workloads.forEach(function(workload) {
						var skillModel = {
							Workload: workload,
							SkillId: skill.Id,
							SkillType: {
								DoDisplayData: isSkillTypeSupported(skill.SkillType),
								SkillType: skill.SkillType
							},
							ChartId: 'chart' + workload.Id
						};
						vm.skills.push(skillModel);

						if (!vm.skilltypes.includes(skillModel.SkillType.SkillType)) {
							vm.skilltypes.push(skillModel.SkillType.SkillType);
						}
					});
				});
			});
		}

		function isSkillTypeSupported(skillType) {
			return skillType === 'SkillTypeInboundTelephony';
		}

		function goToModify(skill) {
			sessionStorage.currentForecastWorkload = angular.toJson(skill);
			$state.go('forecast-modify', { workloadId: skill.Workload.Id });
		}

		function goToHistory(skill) {
			sessionStorage.currentForecastWorkload = angular.toJson(skill);
			$state.go('forecast-history', { workloadId: skill.Workload.Id });
		}

		init();
	}
})();
