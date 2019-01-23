'use strict';

fdescribe('skillPickerComponent', function() {
	var $componentController,
		$translate,
		skillIconService = {
			get: function() {}
		},
		mockedSkills,
		mockedSkillGroups,
		mockedItemToReturn,
		preselectedSkill,
		mockedSkillSelected,
		mockedSkillGroupSelected,
		mockedClearSkillSelection,
		mockedClearSkillGroupSelection,
		mockedPreselectedSkill,
		mockedPreselectedSkillGroup,
		preselectedSkillGroup,
		$templateCache,
		$compile,
		scope;

	beforeEach(function() {
		module('wfm.skillPicker');
		module('wfm.templates');
	});

	beforeEach(inject(function(_$componentController_, _$compile_, _$rootScope_) {
		//$templateCache = _$templateCache_;
		$compile = _$compile_;
		scope = _$rootScope_.$new();
		$componentController = _$componentController_;
		mockedSkills = [
			{
				Id: 'XYZ',
				Name: 'skill1'
			},
			{
				Id: 'ABC',
				Name: 'skill2'
			}
		];

		mockedSkillGroups = [
			{
				Name: 'SkillArea1',
				Id: '123',
				Skills: [
					{
						Id: 'XYZ',
						Name: 'skill1'
					}
				]
			},
			{
				Name: 'SkillArea2',
				Id: '321',
				Skills: [
					{
						Id: 'ABC',
						Name: 'skill2'
					}
				]
			}
		];

		preselectedSkill = { skillIds: ['XYZ'] };
		preselectedSkillGroup = { skillAreaId: '123' };

		mockedItemToReturn = function(item) {};

		mockedSkillSelected = function(skill) {};

		mockedSkillGroupSelected = function(skillGroup) {};

		mockedClearSkillSelection = function() {};

		mockedClearSkillGroupSelection = function() {};
	}));

	function createComponent(noSkillGroup = false) {
		return $componentController(
			'theSkillPicker',
			{
				$translate: $translate,
				skillIconService: skillIconService
			},
			{
				skills: mockedSkills,
				skillGroups: noSkillGroup ? [] : mockedSkillGroups,
				preselectedSkill: mockedPreselectedSkill,
				preselectedSkillGroup: mockedPreselectedSkillGroup,
				onSkillSelected: mockedSkillSelected,
				onSkillGroupSelected: mockedSkillGroupSelected,
				onClearSkillSelection: mockedClearSkillSelection,
				onClearSkillGroupSelection: mockedClearSkillGroupSelection
			}
		);
	}

	it('should not be able to select skill group if no skill group is created', inject(function() {
		var noSkillGroup = true;

		var ctrl = createComponent(noSkillGroup);

		scope.skillGroupCreated = function() {
			return ctrl.skillGroups && ctrl.skillGroups.length === 0;
		};

		scope.skillGroupNotCreated = scope.skillGroupCreated.call();

		var skillPickerElement = $compile(
			'<input ng-model="$ctrl.skillGroupPickerText" type="text" class="skill-group-picker" ng-disabled="skillGroupNotCreated">'
		)(scope);
		scope.$apply();

		var skillGroupElement = skillPickerElement['0'];

		expect(skillGroupElement.disabled).toBeTruthy();
	}));

	it('should clear skillgroup input when selecting other input', function() {
		var ctrl = createComponent();
		spyOn(ctrl, 'onSkillSelected');

		ctrl.selectedSkillGroup = ctrl.skillGroups[0];
		ctrl.skillGroupSelected(ctrl.skillGroups[0]);
		ctrl.selectedSkill = ctrl.skills[0];
		ctrl.skillSelected(ctrl.skills[0]);

		expect(ctrl.selectedSkill).toEqual(ctrl.skills[0]);
		expect(ctrl.skillGroupPickerText).toEqual('');
		expect(ctrl.onSkillSelected).toHaveBeenCalledWith({ skill: ctrl.skills[0] });
	});

	it('should be able to clear skill input', function() {
		var ctrl = createComponent();
		spyOn(ctrl, 'onClearSkillSelection');

		ctrl.selectedSkill = ctrl.skills[0];
		ctrl.skillSelected(ctrl.skills[0]);
		ctrl.clearSkillSelection();

		expect(ctrl.skillPickerText).toEqual('');
		expect(ctrl.onClearSkillSelection).toHaveBeenCalled();
	});

	it('should be able to clear skillArea input', function() {
		var ctrl = createComponent();
		spyOn(ctrl, 'onClearSkillGroupSelection');

		ctrl.selectedSkillGroup = ctrl.skillGroups[0];
		ctrl.skillGroupSelected(ctrl.skillGroups[0]);
		ctrl.clearSkillGroupSelection();

		expect(ctrl.skillGroupPickerText).toEqual('');
		expect(ctrl.onClearSkillGroupSelection).toHaveBeenCalled();
	});
});
