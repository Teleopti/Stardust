'use strict';

describe('SkillTypeService Tests ',
	function() {
		var $translate,
			targetSvc;

		beforeEach(function() {
			module('wfm.utilities');
		});

		beforeEach(inject(function(_$translate_, SkillTypeService) {
			$translate = _$translate_;
			targetSvc = SkillTypeService;
		}));

		it('Should get correct terms for phone type skill',
			function() {
				var skill = {
					SkillType: 'SkillTypeInboundTelephony'
				};

				var skillTexts = targetSvc.getSkillLabels(skill);

				expect(skillTexts.TotalTasks).toEqual('TotalCalls');
				expect(skillTexts.Tasks).toEqual('Calls');
				expect(skillTexts.TotalATW).toEqual('TotalACW');
				expect(skillTexts.ATW).toEqual('ACW');
				expect(skillTexts.TotalTaskTime).toEqual('TotalTalkTime');
				expect(skillTexts.TaskTime).toEqual('TalkTime');
			});

		it('Should get correct terms for chat type skill',
			function() {
				var skill = {
					SkillType: 'SkillTypeChat'
				};

				var skillTexts = targetSvc.getSkillLabels(skill);
				expect(skillTexts.TotalTasks).toEqual('TotalChats');
				expect(skillTexts.Tasks).toEqual('Chats');
				expect(skillTexts.TotalATW).toEqual('TotalACW');
				expect(skillTexts.ATW).toEqual('ACW');
				expect(skillTexts.TotalTaskTime).toEqual('TotalChatTime');
				expect(skillTexts.TaskTime).toEqual('ChatTime');
			});

		it('Should get correct terms for chat type skill',
			function() {
				var skill = {
					SkillType: 'SkillTypeEmail'
				};

				var skillTexts = targetSvc.getSkillLabels(skill);
				expect(skillTexts.TotalTasks).toEqual('TotalEmails');
				expect(skillTexts.Tasks).toEqual('Emails');
				expect(skillTexts.TotalATW).toEqual('TotalAEW');
				expect(skillTexts.ATW).toEqual('AEW');
				expect(skillTexts.TotalTaskTime).toEqual('TotalTaskTime');
				expect(skillTexts.TaskTime).toEqual('TaskTime');
			});

		var otherSkillTypes = [
			'SkillTypeRetail',
			'SkillTypeBackoffice',
			'SkillTypeProject',
			'SkillTypeFax',
			'SkillTypeTime'
		];
		angular.forEach(otherSkillTypes,
			function(skillType) {
				it('Should get correct terms for skill type "' + skillType + '"',
					function() {
						var skill = {
							SkillType: skillType
						};

						var skillTexts = targetSvc.getSkillLabels(skill);
						expect(skillTexts.TotalTasks).toEqual('TotalTasks');
						expect(skillTexts.Tasks).toEqual('Tasks');
						expect(skillTexts.TotalATW).toEqual('TotalATW');
						expect(skillTexts.ATW).toEqual('ATW');
						expect(skillTexts.TotalTaskTime).toEqual('TotalTaskTime');
						expect(skillTexts.TaskTime).toEqual('TaskTime');
					});
			});
	});
