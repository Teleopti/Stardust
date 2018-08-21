(function() {
    'use strict';
	angular
		.module('wfm.utilities')
		.service('SkillTypeService', skillTypeService);

	skillTypeService.$inject = ['$translate'];

	function skillTypeService($translate) {
		var getSkillLabels = function (skill) {
			if (skill.SkillType === 'SkillTypeInboundTelephony') {
				return {
					TotalTasks: $translate.instant('TotalCalls'),
					Tasks: $translate.instant('Calls'),
					TotalATW: $translate.instant('TotalACW'),
					ATW: $translate.instant('ACW'),
					TotalTaskTime: $translate.instant('TotalTalkTime'),
					TaskTime: $translate.instant('TalkTime')
				};
			} else if (skill.SkillType === 'SkillTypeChat') {
				return {
					TotalTasks: $translate.instant('TotalChats'),
					Tasks: $translate.instant('Chats'),
					TotalATW: $translate.instant('TotalACW'),
					ATW: $translate.instant('ACW'),
					TotalTaskTime: $translate.instant('TotalChatTime'),
					TaskTime: $translate.instant('ChatTime')
				};
			} else if (skill.SkillType === 'SkillTypeEmail') {
				return {
					TotalTasks: $translate.instant('TotalEmails'),
					Tasks: $translate.instant('Emails'),
					TotalATW: $translate.instant('TotalAEW'),
					ATW: $translate.instant('AEW'),
					TotalTaskTime: $translate.instant('TotalTaskTime'),
					TaskTime: $translate.instant('TaskTime')
				};
			} else {
				return {
					TotalTasks: $translate.instant('TotalTasks'),
					Tasks: $translate.instant('Tasks'),
					TotalATW: $translate.instant('TotalATW'),
					ATW: $translate.instant('ATW'),
					TotalTaskTime: $translate.instant('TotalTaskTime'),
					TaskTime: $translate.instant('TaskTime')
				};
			}
		};

		return {
			getSkillLabels: getSkillLabels
		};
	}

} )();
