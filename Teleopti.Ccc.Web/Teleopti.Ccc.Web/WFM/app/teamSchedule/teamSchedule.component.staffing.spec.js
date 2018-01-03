﻿describe('[teamSchedule staffing info component tests]', function () {

	var $componentController, $q, $compile, $rootScope, $document, _staffingInfoService, toggles;
	beforeEach(module('wfm.templates', 'wfm.teamSchedule', 'ngMaterial', 'ngMaterial-mock'));
	beforeEach(function () {
		module(function ($provide) {
			toggles = new FakeToggles();
			_staffingInfoService = setUpStaffingInfoService();
			$provide.service('TeamScheduleSkillService', setUpTeamScheduleSkillService);
			$provide.service('StaffingInfoService', function () { return _staffingInfoService; });
			$provide.service('teamsToggles', function () { return toggles; });
		});
	});
	beforeEach(inject(function ($injector) {
		$componentController = $injector.get('$componentController');
		$q = $injector.get('$q');
		$compile = $injector.get('$compile');
		$rootScope = $injector.get('$rootScope');
		$document = $injector.get('$document');
		$httpBackend = $injector.get('$httpBackend');
		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
	}));


	it('should view no available data by default',
		function () {
			var panel = setupComponent();
			var chart = panel[0].querySelector('#staffingChart');
			var noDataPanel = panel[0].querySelectorAll(".nodata");
			expect(chart).toEqual(null);
			expect(noDataPanel.length).toEqual(1);
		});

	it('should view chart if data available',
		function () {
			var scope = $rootScope.$new();

			var panel = setupComponent('selected-date="2017-09-27"', scope);
			var ctrl = panel.isolateScope().vm;
			ctrl.setSkill({ Id: 'XYZ' });
			scope.$apply();
			var chart = panel[0].querySelectorAll('#staffingChart');
			var noDataPanel = panel[0].querySelectorAll(".nodata");
			expect(chart.length).toEqual(1);
			expect(noDataPanel.length).toEqual(0);
		});

	it('should view no available data when no skill selected',
		function () {
			var scope = $rootScope.$new();
			var panel = setupComponent('selected-date="2017-09-27"', scope);
			scope.$apply();

			var chart = panel[0].querySelector('#staffingChart');
			var noDataPanel = panel[0].querySelectorAll(".nodata");
			expect(chart).toEqual(null);
			expect(noDataPanel.length).toEqual(1);
		});

	it('should view no available data when check skill then uncheck skill',
		function () {
			var scope = $rootScope.$new();

			var panel = setupComponent('selected-date="2017-09-27"', scope);
			var ctrl = panel.isolateScope().vm;
			ctrl.setSkill({ Id: 'XYZ' });
			scope.$apply();

			ctrl.setSkill();
			scope.$apply();

			ctrl.useShrinkageForStaffing();
			scope.$apply();

			var chart = panel[0].querySelector('#staffingChart');
			var noDataPanel = panel[0].querySelectorAll(".nodata");
			expect(chart).toEqual(null);
			expect(noDataPanel.length).toEqual(1);
		});

	it('should load chart data depend on the skill preselection', function () {
		var scope = $rootScope.$new();
		scope.preselectedSkills = { skillIds: ['XYZ'] };
		setupComponent('selected-date="2017-09-27" preselected-skills="preselectedSkills"', scope);
		expect(_staffingInfoService.selectedSkill.Id).toEqual('XYZ');
		expect(!!_staffingInfoService.selectedSkillGroup).toEqual(false);
	});

	it('should load chart data depend on the skill group preselection', function () {
		var scope = $rootScope.$new();
		scope.preselectedSkills = { skillAreaId: 'SkillArea1' };
		setupComponent('selected-date="2017-09-27" preselected-skills="preselectedSkills"', scope);
		expect(_staffingInfoService.selectedSkillGroup.Id).toEqual('SkillArea1');
		expect(!!_staffingInfoService.selectedSkill).toEqual(false);
	});


	it("should show skills for the preselected skill group", function () {
		enableShowSkillsToggle();
		var scope = $rootScope.$new();
		scope.preselectedSkills = { skillAreaId: 'SkillArea1' };
		var panel = setupComponent('selected-date="2017-09-27" preselected-skills="preselectedSkills"', scope);
		var skillsRow = panel[0].querySelectorAll('.skills-row');
		var skills = panel[0].querySelectorAll('.skill');
		expect(skillsRow.length).toEqual(1);
		expect(skills.length).toEqual(3);
		expect(skills[0].innerText.trim()).toEqual('skill1');
		expect(skills[1].innerText.trim()).toEqual('skill2');
		expect(skills[2].innerText.trim()).toEqual('skill3');
	});

	it("should change skills after change the selected skill group", function () {
		enableShowSkillsToggle();
		var scope = $rootScope.$new();
		scope.preselectedSkills = { skillAreaId: 'SkillArea1' };
		var panel = setupComponent('selected-date="2017-09-27" preselected-skills="preselectedSkills"', scope);
		var ctrl = panel.isolateScope().vm;

		ctrl.setSkill(skillGroups[1]);
		scope.$apply();

		var skillsRow = panel[0].querySelectorAll('.skills-row');
		var skills = panel[0].querySelectorAll('.skill');
		expect(skillsRow.length).toEqual(1);
		expect(skills.length).toEqual(1);
		expect(skills[0].innerText.trim()).toEqual('skill1');
	});

	it("should hide skills if no selected skill group", function () {
		enableShowSkillsToggle();
		var scope = $rootScope.$new();
		scope.preselectedSkills = { skillAreaId: 'SkillArea1' };
		var panel = setupComponent('selected-date="2017-09-27" preselected-skills="preselectedSkills"', scope);
		var ctrl = panel.isolateScope().vm;

		ctrl.setSkill(null);
		scope.$apply();
		var skillsRow = panel[0].querySelectorAll('.skills-row');
		expect(skillsRow.length).toEqual(0);
	});

	it("should hide skills when select skill", function () {
		enableShowSkillsToggle();
		var scope = $rootScope.$new();
		scope.preselectedSkills = { skillAreaId: 'SkillArea1' };
		var panel = setupComponent('selected-date="2017-09-27" preselected-skills="preselectedSkills"', scope);
		var ctrl = panel.isolateScope().vm;

		ctrl.setSkill({ Id: 'XYZ' });
		scope.$apply();
		var skillsRow = panel[0].querySelectorAll('.skills-row');
		expect(skillsRow.length).toEqual(0);
	});

	function enableShowSkillsToggle() {
		toggles.set({
			WfmTeamSchedule_ShowSkillsForSelectedSkillGroupInStaffingInfo_47202: true
		});
	}

	function setupComponent(attrs, scope) {
		var el;
		var template = '' +
			'<staffing-info ' + (attrs || '') + '>' +
			'</staffing-info>';

		el = $compile(template)(scope || $rootScope);
		if (scope) {
			scope.$apply();
		} else {
			$rootScope.$digest();
		}
		return el;
	}

	var skillGroups = [{
		Name: 'SkillArea1',
		Id: 'SkillArea1',
		Skills: [
			{
				Id: 'skill1',
				Name: 'skill1'
			},
			{
				Id: 'skill2',
				Name: 'skill2'
			},
			{
				Id: 'skill3',
				Name: 'skill3'
			}
		]
	},
	{
		Name: 'SkillArea2',
		Id: '321',
		Skills: [
			{
				Id: 'skill1',
				Name: 'skill1'
			}
		]
	}];
	function setUpTeamScheduleSkillService() {
		return {
			getAllSkills: function () {
				return {
					then: function (callback) {
						callback([
							{
								Id: 'XYZ',
								Name: 'skill1'
							},
							{
								Id: 'ABC',
								Name: 'skill2'
							}
						]);
					}
				};
			},
			getAllSkillGroups: function () {
				return {
					then: function (callback) {
						callback({
							SkillAreas: skillGroups
						});
					}
				};
			},

		};


	}

	function setUpStaffingInfoService() {
		return {
			selectedSkill: null,
			selectedSkillGroup: null,
			getStaffingByDate: function (selectedSkill, selectedSkillGroup) {
				this.selectedSkill = selectedSkill;
				this.selectedSkillGroup = selectedSkillGroup;
				return {
					then: function (callback) {
						callback({
							"DataSeries": {
								"Date": "2015-06-01T00:00:00",
								"Time":
								[
									"2015-06-01T08:00:00", "2015-06-01T08:15:00", "2015-06-01T08:30:00", "2015-06-01T08:45:00",
									"2015-06-01T09:00:00", "2015-06-01T09:15:00", "2015-06-01T09:30:00", "2015-06-01T09:45:00",
									"2015-06-01T10:00:00", "2015-06-01T10:15:00", "2015-06-01T10:30:00", "2015-06-01T10:45:00",
									"2015-06-01T11:00:00", "2015-06-01T11:15:00", "2015-06-01T11:30:00", "2015-06-01T11:45:00",
									"2015-06-01T12:00:00", "2015-06-01T12:15:00", "2015-06-01T12:30:00", "2015-06-01T12:45:00",
									"2015-06-01T13:00:00", "2015-06-01T13:15:00", "2015-06-01T13:30:00", "2015-06-01T13:45:00",
									"2015-06-01T14:00:00", "2015-06-01T14:15:00", "2015-06-01T14:30:00", "2015-06-01T14:45:00",
									"2015-06-01T15:00:00", "2015-06-01T15:15:00", "2015-06-01T15:30:00", "2015-06-01T15:45:00",
									"2015-06-01T16:00:00", "2015-06-01T16:15:00", "2015-06-01T16:30:00", "2015-06-01T16:45:00",
									"2015-06-01T17:00:00", "2015-06-01T17:15:00", "2015-06-01T17:30:00", "2015-06-01T17:45:00"
								],
								"ForecastedStaffing":
								[
									5.913, 7.152, 8.394, 9.192, 10.849, 12.803, 14.322, 15.536, 16.056, 16.152, 16.29, 16.317, 15.467, 14.658,
									14.043, 13.668, 13.447, 13.34, 13.338, 13.205, 13.111, 13.024, 13.023, 12.512, 12.234, 11.964, 12.315, 12.106,
									11.922, 11.299, 11.0, 10.37, 9.429, 8.274, 7.594, 6.616, 5.667, 5.089, 4.687, 4.396
								],
								"UpdatedForecastedStaffing": null,
								"ActualStaffing": null,
								"ScheduledStaffing": [],
								"AbsoluteDifference": [
									-5.913, -7.152, -8.394, -9.192, -10.849, -12.803, -14.322, -15.536, -16.056, -16.152, -16.29, -16.317, -15.467,
									-14.658, -14.043, -13.668, -13.447, -13.34, -13.338, -13.205, -13.111, -13.024, -13.023, -12.512, -12.234,
									-11.964, -12.315, -12.106, -11.922, -11.299, -11.0, -10.37, -9.429, -8.274, -7.594, -6.616, -5.667, -5.089,
									-4.687, -4.396
								]
							},
							"StaffingHasData": true
						});
					}
				};
			}
		}
	}

	function FakeToggles() {
		var _toggles = {};
		this.set = function (toggles) {
			_toggles = toggles;
		}
		this.all = function () {
			return _toggles;
		}
	}



});