'use strict';
describe('IntradayCtrl', function () {
	var $httpBackend,
		$controller,
		scope;

	var skillAreas = [];
	var skills = [];
	var skillAreaInfo;
	var monitorData;

	beforeEach(module('wfm.intraday'));

	beforeEach(function () {
		skills = [
		{
			Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
			Name: "skill x"
		}];

		skillAreas = [
		{
			Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
			Name: "my skill area 1",
			Skills: skills
		},
		{
			Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4",
			Name: "my skill area 2",
			Skills: skills
		}];

		skillAreaInfo = {
			HasPermissionToModifySkillArea: true,
			SkillAreas: skillAreas
		};

		monitorData = {
			ForecastedCalls: 100,
			OfferedCalls: 50,
			LatestStatsTime: '1901-01-01 13:00',
			ForecastedActualCallsDiff: 50
		};
	});
	
	beforeEach(inject(function (_$httpBackend_, _$controller_, _$rootScope_) {
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		scope = _$rootScope_.$new();

		$httpBackend.whenGET("../api/intraday/skillarea")
			.respond(function() {
			return [200, skillAreaInfo];
		});

		$httpBackend.whenGET("../api/intraday/skills")
			.respond(function() {
			return [200, skills];
		});

		$httpBackend.whenDELETE("../api/intraday/skillarea/836cebb6-cee8-41a1-bb62-729f4b3a63f4")
			.respond(200, {});

		$httpBackend.whenGET("../api/intraday/monitorskillarea/fa9b5393-ef48-40d1-b7cc-09e797589f81")
			.respond(function() {
			return [200, monitorData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskill/5f15b334-22d1-4bc1-8e41-72359805d30f")
			.respond(function() {
			return [200, monitorData];
		});
	}));

	var createController = function() {
		$controller('IntradayCtrl', {
			$scope: scope
		});
		scope.$digest();
		$httpBackend.flush();
	};

	it('should display list of skill areas', function () {
		createController();

		expect(scope.skillAreas[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(scope.skillAreas[0].Name).toEqual("my skill area 1");
		expect(scope.skillAreas[0].Skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(scope.skillAreas[0].Skills[0].Name).toEqual("skill x");
	});

	it('should display list of skills', function () {
		createController();

		expect(scope.skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(scope.skills[0].Name).toEqual("skill x");
	});

	it('should delete selected skill area', function() {
		createController();
		scope.deleteSkillArea(scope.skillAreas[1]);

		expect(scope.skillAreas.length).toEqual(2);
		
		$httpBackend.flush();

		expect(scope.selectedItem).toEqual(null);
		expect(scope.skillAreas.length).toEqual(1);
	});

	it('should monitor first skill if no skill areas', function() {
		skillAreaInfo.SkillAreas = [];
		createController();

		scope.skillSelected(scope.skills[0]);
		$httpBackend.flush();

		expect(scope.selectedItem).toEqual(scope.skills[0]);
		expect(scope.forecastedCalls).toEqual(monitorData.ForecastedCalls);
		expect(scope.offeredCalls).toEqual(monitorData.OfferedCalls);
		expect(scope.latestStatsTime).toEqual(monitorData.LatestStatsTime);
		expect(scope.difference).toEqual(monitorData.ForecastedActualCallsDiff);
	});

	it('should monitor first skill area if there are any', function () {
		createController();

		scope.skillAreaSelected(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.selectedItem).toEqual(scope.skillAreas[0]);
		expect(scope.forecastedCalls).toEqual(monitorData.ForecastedCalls);
		expect(scope.offeredCalls).toEqual(monitorData.OfferedCalls);
		expect(scope.latestStatsTime).toEqual(monitorData.LatestStatsTime);
		expect(scope.difference).toEqual(monitorData.ForecastedActualCallsDiff);
	});

	it('should have permission to modify skill area', function() {
		createController();

		expect(scope.HasPermissionToModifySkillArea).toEqual(true);
	});

	it('should show friendly message if no data for skill area', function () {
		createController();
		scope.skillAreaSelected(scope.skillAreas[0]);
		monitorData.LatestStatsTime = '0000-01-01';
		$httpBackend.flush();

		expect(scope.HasMonitorData).toEqual(false);
	});
});