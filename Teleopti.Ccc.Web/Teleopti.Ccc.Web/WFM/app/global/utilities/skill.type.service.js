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
					TaskTime: $translate.instant('TalkTime'),
					OriginalTasks: $translate.instant('OriginalPhoneCalls'),
					ValidatedTasks: $translate.instant('ValidatedPhoneCalls')
				};
			} else if (skill.SkillType === 'SkillTypeChat') {
				return {
					TotalTasks: $translate.instant('TotalChats'),
					Tasks: $translate.instant('Chats'),
					TotalATW: $translate.instant('TotalACW'),
					ATW: $translate.instant('ACW'),
					TotalTaskTime: $translate.instant('TotalChatTime'),
					TaskTime: $translate.instant('ChatTime'),
					OriginalTasks: $translate.instant('OriginalChats'),
					ValidatedTasks: $translate.instant('ValidatedChats')


				};
			} else if (skill.SkillType === 'SkillTypeEmail') {
				return {
					TotalTasks: $translate.instant('TotalEmails'),
					Tasks: $translate.instant('Emails'),
					TotalATW: $translate.instant('TotalAEW'),
					ATW: $translate.instant('AEW'),
					TotalTaskTime: $translate.instant('TotalTaskTime'),
					TaskTime: $translate.instant('TaskTime'),
					OriginalTasks: $translate.instant('OriginalEmails'),
					ValidatedTasks: $translate.instant('ValidatedEmails')


				};
			} else {
				return {
					TotalTasks: $translate.instant('TotalTasks'),
					Tasks: $translate.instant('Tasks'),
					TotalATW: $translate.instant('TotalATW'),
					ATW: $translate.instant('ATW'),
					TotalTaskTime: $translate.instant('TotalTaskTime'),
					TaskTime: $translate.instant('TaskTime'),
					OriginalTasks: $translate.instant('OriginalTasks'),
					ValidatedTasks: $translate.instant('ValidatedTasks')


				};
			}
		};

		return {
			getSkillLabels: getSkillLabels
		};
	}

} )();
