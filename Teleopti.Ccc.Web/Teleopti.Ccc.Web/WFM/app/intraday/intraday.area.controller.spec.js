'use strict';
fdescribe('IntradayAreaCtrl', function () {
	var $httpBackend,
	$controller,
	$filter,
	scope,
	$translate,
	$interval,
	NoticeService;

	var skillAreas = [];
	var skills = [];
	var skillsWithFirstUnsupported = [];
	var skillAreaInfo;
	var trafficAndPerformanceData;
	var trafficAndPerformanceDataTomorrow;
	var performanceData;
	var performanceDataTomorrow;
	var staffingData;
	var staffingDataTomorrow;
	var emptyStaffingData;
	var timeData;
	var isUnsupportedSkillTest = false;

	beforeEach(function() {
		module('wfm.intraday');

		module(function ($provide) {
			$provide.service('Toggle',
				function() {
					return {
						Wfm_Intraday_OptimalStaffing_40921: true,
						Wfm_Intraday_ScheduledStaffing_41476: true,
						Wfm_Intraday_ESL_41827: true,
						WFM_Intraday_Show_For_Other_Days_43504: false,
						togglesLoaded: {
							then: function(cb) { cb(); }
						}
					}
				});
		});
	});

	beforeEach(function () {

		skills = [
			{
				Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
				Name: "skill x",
				DoDisplayData: true,
				ShowAbandonRate: true,
				ShowReforecastedAgents: true
			},
			{
				Id: "502632DC-7A0C-434D-8A75-3153D5160787",
				Name: "skill y",
				DoDisplayData: true,
				ShowAbandonRate: false,
				ShowReforecastedAgents: false
			}
		];

		skillsWithFirstUnsupported = [
			{
				Id: "63b0ac3d-06b5-42d7-bb0a-d7b2fd272196",
				Name: "Unsupported skill1",
				DoDisplayData: false
			},
			{
				Id: "22b0ac3d-06b5-42d7-bb0a-d7b2fd272196",
				Name: "Supported skill1",
				DoDisplayData: true
			}
		];

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
			},
			{
				Id: "3f43f39b-f01b-47e7-b59f-35fc09fd5e41",
				Name: "my unsupported skill area 1",
				Skills: skillsWithFirstUnsupported,
				UnsupportedSkills: skillsWithFirstUnsupported
			}
		];

		skillAreaInfo = {
			HasPermissionToModifySkillArea: true,
			SkillAreas: skillAreas
		};

		trafficAndPerformanceData = {
			LatestActualIntervalEnd: "0001-01-01T16:00:00",
			LatestActualIntervalStart: "0001-01-01T15:45:00",
			IncomingTrafficHasData: true,
			DataSeries: {
				AbandonedRate: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				AverageHandleTime: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				AverageSpeedOfAnswer: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				ForecastedAverageHandleTime: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 199.8660714285, 201.3520471464],
				ForecastedCalls: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0112, 0.3224, 0.5169, 0.7337, 0.9672],
				CalculatedCalls: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				ServiceLevel: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				Time: ["0001-01-01T00:00:00", "0001-01-01T00:15:00", "0001-01-01T00:30:00", "0001-01-01T00:45:00"]
			},
			Summary: {
				AbandonRate: 0.05594855305466238,
				AbandonedCalls: 87,
				AnsweredCalls: 1468,
				AnsweredCallsWithinSL: 1305,
				AverageHandleTime: 245.63987138263664,
				AverageSpeedOfAnswer: 22,
				ForecastedActualCallsDiff: 11.686895162492235,
				ForecastedActualHandleTimeDiff: 26.25497332067074,
				ForecastedAverageHandleTime: 194.55857058299338,
				ForecastedCalls: 1392.2850999999998,
				ForecastedHandleTime: 270880.99889999995,
				HandleTime: 381970,
				CalculatedCalls: 1555,
				ServiceLevel: 0.8392282958199357,
				SpeedOfAnswer: 32296
			}
		};
		trafficAndPerformanceDataTomorrow = {
			LatestActualIntervalEnd: "0001-01-02T16:00:00",
			LatestActualIntervalStart: "0001-01-02T15:45:00",
			IncomingTrafficHasData: true,
			DataSeries: {
				AbandonedRate: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				AverageHandleTime: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				AverageSpeedOfAnswer: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				ForecastedAverageHandleTime: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 199.8660714285, 201.3520471464],
				ForecastedCalls: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0112, 0.3224, 0.5169, 0.7337, 0.9672],
				CalculatedCalls: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				ServiceLevel: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				Time: ["0001-01-02T00:00:00", "0001-01-02T00:15:00", "0001-01-02T00:30:00", "0001-01-02T00:45:00"]
			},
			Summary: {
				AbandonRate: 0.05594855305466238,
				AbandonedCalls: 87,
				AnsweredCalls: 1468,
				AnsweredCallsWithinSL: 1305,
				AverageHandleTime: 245.63987138263664,
				AverageSpeedOfAnswer: 22,
				ForecastedActualCallsDiff: 11.686895162492235,
				ForecastedActualHandleTimeDiff: 26.25497332067074,
				ForecastedAverageHandleTime: 194.55857058299338,
				ForecastedCalls: 1392.2850999999998,
				ForecastedHandleTime: 270880.99889999995,
				HandleTime: 381970,
				CalculatedCalls: 1337,
				ServiceLevel: 0.90,
				SpeedOfAnswer: 32296
			}
		};

		performanceData = {
			LatestActualIntervalEnd: "0001-01-01T16:00:00",
			LatestActualIntervalStart: "0001-01-01T15:45:00",
			PerformanceHasData: true,
			DataSeries: {
				AbandonedRate: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				AverageSpeedOfAnswer: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				EstimatedServiceLevels: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				ServiceLevel: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				Time: ["0001-01-01T00:00:00", "0001-01-01T00:15:00", "0001-01-01T00:30:00", "0001-01-01T00:45:00"]
			},
			Summary: {
				AbandonRate: 0.05594855305466238,
				AverageSpeedOfAnswer: 22,
				ServiceLevel: 0.8392282958199357,
				EstimatedServiceLevel: 0.8735783757893
			}
		};

		performanceDataTomorrow = {
			LatestActualIntervalEnd: "0001-01-01T16:00:00",
			LatestActualIntervalStart: "0001-01-01T15:45:00",
			PerformanceHasData: true,
			DataSeries: {
				AbandonedRate: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				AverageSpeedOfAnswer: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				EstimatedServiceLevels: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				ServiceLevel: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
				Time: ["0001-01-01T00:00:00", "0001-01-01T00:15:00", "0001-01-01T00:30:00", "0001-01-01T00:45:00"]
			},
			Summary: {
				AbandonRate: 0.05594855305466238,
				AverageSpeedOfAnswer: 22,
				ServiceLevel: 0.90,
				EstimatedServiceLevel: 0.8735783757893
			}
		};

		timeData = {
			latestIntervalTime: {
				StartTime: {},
				EndTime: {}
			}
		};
		staffingData = {
			StaffingHasData: true,
			DataSeries: {
				ForecastedStaffing: [1, 2, 3],
				UpdatedForecastedStaffing: [2, 3, 4],
				ActualStaffing: [2, 3, 4],
				Time: ["2016-08-30T00:00:00", "2016-08-30T00:15:00"],
				ScheduledStaffing: [1, 2, 3]
			}
		};

		staffingDataTomorrow = {
			StaffingHasData: true,
			DataSeries: {
				ForecastedStaffing: [3, 2, 1],
				UpdatedForecastedStaffing: [2, 3, 4],
				ActualStaffing: [2, 3, 4],
				Time: ["2016-08-30T00:00:00", "2016-08-30T00:15:00"],
				ScheduledStaffing: [1, 2, 3]
			}
		};

		emptyStaffingData = {
			DataSeries: null
		};
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$rootScope_, _$filter_, _$translate_, _$interval_, _NoticeService_) {
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		$filter = _$filter_;
		$translate = _$translate_;
		scope = _$rootScope_.$new();
		$interval = _$interval_;
		NoticeService = _NoticeService_;

		$httpBackend.whenGET("../api/intraday/skillarea")
		.respond(function () {
			return [200, skillAreaInfo];
		});

		$httpBackend.whenGET("../api/intraday/skills")
		.respond(function () {
			if (isUnsupportedSkillTest) {
				return [200, skillsWithFirstUnsupported];
			} else {
				return [200, skills];
			}
		});

		$httpBackend.whenDELETE("../api/intraday/skillarea/836cebb6-cee8-41a1-bb62-729f4b3a63f4")
		.respond(200, {});

		$httpBackend.whenGET("../api/intraday/monitorskillareastatistics/fa9b5393-ef48-40d1-b7cc-09e797589f81")
		.respond(function () {
			return [200, trafficAndPerformanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstatistics/5f15b334-22d1-4bc1-8e41-72359805d30f")
		.respond(function () {
			return [200, trafficAndPerformanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstatistics/" + skills[0].Id + "/0")
		.respond(function () {
			return [200, trafficAndPerformanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstatistics/" + skills[0].Id + "/1")
		.respond(function () {
			return [200, trafficAndPerformanceDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstatistics/" + skills[1].Id + "/0")
		.respond(function () {
			return [200, trafficAndPerformanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstatistics/" + skills[1].Id + "/1")
		.respond(function () {
			return [200, trafficAndPerformanceDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastatistics/" + skillAreas[0].Id + "/0")
		.respond(function () {
			return [200, trafficAndPerformanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastatistics/" + skillAreas[0].Id + "/1")
		.respond(function () {
			return [200, trafficAndPerformanceDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastatisticsbydate?dateUtc=0001-01-01T14:45:00.000Z&id=" + skillAreas[0].Id)
		.respond(function () {
			return [200, trafficAndPerformanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastatisticsbydate?dateUtc=0001-01-02T14:45:00.000Z&id=" + skillAreas[0].Id)
		.respond(function () {
			return [200, trafficAndPerformanceDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareaperformance/fa9b5393-ef48-40d1-b7cc-09e797589f81")
		.respond(function () {
			return [200, performanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareaperformance/" + skillAreas[0].Id + "/0")
		.respond(function () {
			return [200, performanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareaperformance/" + skillAreas[0].Id + "/1")
		.respond(function () {
			return [200, performanceDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillperformance/5f15b334-22d1-4bc1-8e41-72359805d30f")
		.respond(function () {
			return [200, performanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillperformance/" + skills[0].Id + "/0")
		.respond(function () {
			return [200, performanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillperformance/" + skills[0].Id + "/1")
		.respond(function () {
			return [200, performanceDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillperformance/" + skills[1].Id + "/0")
		.respond(function () {
			return [200, performanceData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillperformance/" + skills[1].Id + "/1")
		.respond(function () {
			return [200, performanceDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstaffing/5f15b334-22d1-4bc1-8e41-72359805d30f")
		.respond(function () {
			return [200, staffingData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstaffing/" + skills[0].Id + "/0")
		.respond(function () {
			return [200, staffingData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstaffing/" + skills[0].Id + "/1")
		.respond(function () {
			return [200, staffingDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstaffing/" + skills[1].Id + "/0")
		.respond(function () {
			return [200, staffingData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillstaffing/" + skills[1].Id + "/1")
		.respond(function () {
			return [200, staffingDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastaffing/" + skillAreas[0].Id)
		.respond(function () {
			return [200, staffingData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastaffing/" + skillAreas[0].Id + "/0")
		.respond(function () {
			return [200, staffingData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastaffing/" + skillAreas[0].Id + "/1")
		.respond(function () {
			return [200, staffingDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastaffing?DateTime=0001-01-01T14:45:00.000Z&SkillAreaId=" + skillAreas[0].Id + "&UseShrinkage=false")
		.respond(function () {
			return [200, staffingData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastaffing?DateTime=0001-01-02T14:45:00.000Z&SkillAreaId=" + skillAreas[0].Id + "&UseShrinkage=false")
		.respond(function () {
			return [200, staffingDataTomorrow];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastaffing/fa9b5393-ef48-40d1-b7cc-09e797589f81")
		.respond(function () {
			return [200, staffingData];
		});

		$httpBackend.whenGET("../api/intraday/monitorskillareastaffing/3f43f39b-f01b-47e7-b59f-35fc09fd5e41")
		.respond(function () {
			return [200, emptyStaffingData];
		});

		$httpBackend.whenGET("../api/intraday/lateststatisticstimeforskill/5f15b334-22d1-4bc1-8e41-72359805d30f")
		.respond(function () {
			return [200, timeData];
		});

		$httpBackend.whenGET("../api/intraday/lateststatisticstimeforskill/502632DC-7A0C-434D-8A75-3153D5160787")
		.respond(function () {
			return [200, timeData];
		});

		$httpBackend.whenGET("../api/intraday/lateststatisticstimeforskillarea/fa9b5393-ef48-40d1-b7cc-09e797589f81")
		.respond(function () {
			return [200, timeData];
		});

		$httpBackend.whenGET("../api/intraday/lateststatisticstimeforskillarea/3f43f39b-f01b-47e7-b59f-35fc09fd5e41")
		.respond(function () {
			return [200, timeData];
		});



	}));

	var createController = function (isNewlyCreatedSkillArea) {
		$controller('IntradayAreaCtrl', {
			$scope: scope,
			$translate: $translate
		});

		scope.onStateChanged(undefined, { name: 'intraday.area' }, { isNewSkillArea: isNewlyCreatedSkillArea });
		scope.$digest();
		$httpBackend.flush();
	};

	it('should stop polling when page is about to destroy', function () {
		createController(false);
		$interval(function () {
			scope.$emit('$destroy');
		}, 5000);
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should display list of skill areas', function () {
		createController(false);

		expect(scope.skillAreas[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(scope.skillAreas[0].Name).toEqual("my skill area 1");
		expect(scope.skillAreas[0].Skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(scope.skillAreas[0].Skills[0].Name).toEqual("skill x");
	});

	it('should display list of skills', function () {
		createController(false);

		expect(scope.skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(scope.skills[0].Name).toEqual("skill x");
	});

	it('should delete selected skill area', function () {
		createController(false);

		scope.deleteSkillArea(scope.skillAreas[1]);

		expect(scope.skillAreas.length).toEqual(3);
		$httpBackend.flush();
		expect(scope.selectedItem).toEqual(null);
		expect(scope.skillAreas.length).toEqual(2);
	});

	it('should monitor first skill if no skill areas', function () {
		skillAreaInfo.SkillAreas = [];
		createController(false);

		scope.skillSelected(scope.skills[0]);
		expect(scope.selectedItem).toEqual(scope.skills[0]);
	});


	it('should monitor first skill area if there are any', function () {
		createController(false);

		scope.skillAreaSelected(scope.skillAreas[0]);

		expect(scope.selectedItem).toEqual(scope.skillAreas[0]);
	});

	it('should have permission to modify skill area', function () {
		createController(false);

		expect(scope.HasPermissionToModifySkillArea).toEqual(true);
	});

	it('should poll data for skill when selecting that skill', function () {
		createController(false);
		scope.activeTab = 0;

		scope.selectedSkillChange(scope.skills[0]);
		$httpBackend.flush();
		expect(scope.viewObj.hasMonitorData).toEqual(true);
	});

	it('should poll data for skill area when selecting that area', function () {
		createController(false);
		scope.activeTab = 0;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.hasMonitorData).toEqual(true);
	});

	it('should only poll traffic skill data when traffic tab and skill is selected', function () {
		createController(false);
		scope.activeTab = 0;

		scope.selectedSkillChange(scope.skills[0]);
		$httpBackend.flush();

		expect(scope.viewObj.forecastedCallsObj.series.length).toBeGreaterThan(5);
	});

	it('should only poll performance skill data when performance tab and skill is selected', function () {
		createController(false);
		scope.activeTab = 1;

		scope.selectedSkillChange(scope.skills[0]);
		$httpBackend.flush();

		expect(scope.viewObj.serviceLevelObj.series.length).toBeGreaterThan(5);
	});

	it('should show estimated service level in performance tab and skill is selected', function () {
		createController(false);
		scope.activeTab = 1;

		scope.selectedSkillChange(scope.skills[0]);
		$httpBackend.flush();

		expect(scope.viewObj.estimatedServiceLevelObj.series.length).toBeGreaterThan(0);
		expect(scope.viewObj.summary.summaryEstimatedServiceLevel).toBeGreaterThan(0);

	});

	it('should only poll staffing skill data when staffing tab and skill is selected', function () {
		createController(false);
		scope.activeTab = 2;

		scope.selectedSkillChange(scope.skills[0]);
		$httpBackend.flush();

		expect(scope.viewObj.forecastedStaffing.series.length).toBeGreaterThan(3);
	});

	it('should only poll traffic skill area data when traffic tab and skill area is selected', function () {
		createController(false);
		scope.activeTab = 0;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.forecastedCallsObj.series.length).toBeGreaterThan(5);
	});

	it('should only poll performance skill area data when performance tab and skill area is selected', function () {
		createController(false);
		scope.activeTab = 1;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.serviceLevelObj.series.length).toBeGreaterThan(5);
	});

	it('should only poll staffing skill area data when staffing tab and skill area is selected', function () {
		createController(false);
		scope.activeTab = 2;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.forecastedStaffing.series.length).toBeGreaterThan(3);
	});

	it('should return no staffing when no supported skills in skill area', function () {
		createController(false);
		scope.activeTab = 2;

		scope.selectedSkillAreaChange(scope.skillAreas[2]);
		$httpBackend.flush();

		expect(scope.viewObj.forecastedStaffing.series.length).toEqual(0);
	});

	it('should show optimal staffing when toggle is enabled', function () {
		createController(false);
		scope.activeTab = 2;

		scope.toggles.showOptimalStaffing = true;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.actualStaffingSeries.length).toBeGreaterThan(3);
	});

	it('should not show optimal staffing when toggle is disabled', function () {
		createController(false);
		scope.activeTab = 2;

		scope.toggles.showOptimalStaffing = false;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.actualStaffingSeries.length).toEqual(1);
	});

	it('should show scheduled staffing when toggle is enabled', function () {
		createController(false);
		scope.activeTab = 2;

		scope.toggles.showScheduledStaffing = true;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.scheduledStaffing.length).toBeGreaterThan(3);
	});

	it('should not show scheduled staffing when toggle is disabled', function () {
		createController(false);
		scope.activeTab = 2;

		scope.toggles.showScheduledStaffing = false;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.scheduledStaffing.length).toEqual(1);
	});

	it('should show ESL in performance view when toggle is enabled', function () {
		createController(false);
		scope.activeTab = 1;

		scope.toggles.showEsl = true;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.estimatedServiceLevelObj.series.length).toBeGreaterThan(0);
		expect(scope.viewObj.summary.summaryEstimatedServiceLevel).toBeGreaterThan(0);
	});

	it('should show ESL in performance view when toggle is disabled', function () {
		createController(false);
		scope.activeTab = 1;

		scope.toggles.showEsl = false;

		scope.selectedSkillAreaChange(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.viewObj.estimatedServiceLevelObj.series.length).toEqual(1);
		expect(scope.viewObj.summary.summaryEstimatedServiceLevel).toEqual(undefined);
	});

	it('should monitor first skill that is supported', function () {
		skillAreaInfo.SkillAreas = [];
		isUnsupportedSkillTest = true;
		createController(false);

		scope.selectedSkillChange(skillsWithFirstUnsupported[0]);

		expect(scope.selectedItem).toEqual(scope.skills[1]);
	});

	it('should get traffic data corresponding to chosenOffset', function(){
		skillAreaInfo.SkillAreas = [];
		isUnsupportedSkillTest = false;

		createController(false);
		scope.activeTab = 0;
		scope.toggles.showOtherDay = true;
		scope.selectedSkillChange(skills[0]);
		
		scope.changeChosenOffset(1);
		$httpBackend.flush();

		expect(scope.chosenOffset.value).toEqual(1);
		expect(scope.viewObj.summary.summaryCalculatedCalls).toEqual('1,337.0');
	});

	it('should get performance data corresponding to chosenOffset', function(){
		skillAreaInfo.SkillAreas = [];
		isUnsupportedSkillTest = false;
		
		createController(false);
		scope.activeTab = 1;
		scope.toggles.showOtherDay = true;
		scope.selectedSkillChange(skills[0]);
		scope.changeChosenOffset(1);
		$httpBackend.flush();

		expect(scope.chosenOffset.value).toEqual(1);
		expect(scope.viewObj.summary.summaryServiceLevel).toEqual('90.0');
	});

	it('should get staffing data corresponding to chosenOffset', function(){
		skillAreaInfo.SkillAreas = [];
		isUnsupportedSkillTest = false;

		createController(false);
		scope.activeTab = 2;
		scope.toggles.showOtherDay = true;
		scope.selectedSkillChange(skills[0]);
		scope.changeChosenOffset(1);
		$httpBackend.flush();

		expect(scope.chosenOffset.value).toEqual(1);
		expect(scope.viewObj.forecastedStaffing.series[1]).toEqual(3);

	});

	it('should get traffic data for skillarea corresponding to chosenOffset', function(){
		createController(false);
		scope.activeTab = 0;
		scope.toggles.showOtherDay = true;
		scope.skillAreaSelected(skillAreas[0]);
		scope.changeChosenOffset(1);
		$httpBackend.flush();

		expect(scope.chosenOffset.value).toEqual(1);
		expect(scope.viewObj.summary.summaryCalculatedCalls).toEqual('1,337.0');
	});

	it('should get performance data for skillarea corresponding to chosenOffset', function(){
		createController(false);
		scope.activeTab = 1;
		scope.toggles.showOtherDay = true;
		scope.skillAreaSelected(skillAreas[0]);
		scope.changeChosenOffset(1);
		$httpBackend.flush();

		expect(scope.chosenOffset.value).toEqual(1);
		expect(scope.viewObj.summary.summaryServiceLevel).toEqual('90.0');
	});

	it('should get staffing data for skillarea corresponding to chosenOffset', function(){
		createController(false);
		scope.activeTab = 2;
		scope.toggles.showOtherDay = true;
		scope.skillAreaSelected(skillAreas[0]);
		scope.changeChosenOffset(1);
		$httpBackend.flush();

		expect(scope.chosenOffset.value).toEqual(1);
		expect(scope.viewObj.forecastedStaffing.series[1]).toEqual(3);
		jasmine.clock().uninstall();
	});

	it('should get traffic data for skillarea corresponding to chosenOffset', function(){
		createController(false);
		scope.activeTab = 0;
		scope.toggles.showOtherDay = true;
		scope.skillAreaSelected(skillAreas[0]);

		var today = moment('0001-01-01T15:45:00').toDate();
		jasmine.clock().mockDate(today);
		scope.changeChosenOffset(1);
		$httpBackend.flush();
		expect(scope.chosenOffset.value).toEqual(1);
		expect(scope.viewObj.summary.summaryCalculatedCalls).toEqual('1,337.0');

		jasmine.clock().uninstall();
	});

	it('should get performance data for skillarea corresponding to chosenOffset', function(){
		createController(false);
		scope.activeTab = 1;
		scope.toggles.showOtherDay = true;
		scope.skillAreaSelected(skillAreas[0]);

		var today = moment('0001-01-01T15:45:00').toDate();
		jasmine.clock().mockDate(today);
		scope.changeChosenOffset(1);
		$httpBackend.flush();
		expect(scope.chosenOffset.value).toEqual(1);
		expect(scope.viewObj.summary.summaryServiceLevel).toEqual('90.0');

		jasmine.clock().uninstall();
	});

	it('should get staffing data for skillarea corresponding to chosenOffset', function(){
		createController(false);
		scope.activeTab = 2;
		scope.toggles.showOtherDay = true;
		scope.skillAreaSelected(skillAreas[0]);

		var today = moment('0001-01-01T15:45:00').toDate();
		jasmine.clock().mockDate(today);
		scope.changeChosenOffset(1);
		$httpBackend.flush();
		expect(scope.chosenOffset.value).toEqual(1);
		expect(scope.viewObj.forecastedStaffing.series[1]).toEqual(3);
		jasmine.clock().uninstall();

	});
	
	it('should not show abandon rate data when toggle is enabled and email-like skill chosen', function () {
		createController(false);
		scope.activeTab = 1;
		scope.toggles.otherSkillsLikeEmail = true;

		scope.selectedSkillChange(skills[1]);

		$httpBackend.flush();

		expect(scope.viewObj.abandonedRateObj.series.length).toEqual(1);
		expect(scope.viewObj.summary.summaryAbandonedRate).toEqual(undefined);
	});

	it('should not show reforcasted agents data when toggle is enabled and email-like skill chosen', function () {
		createController(false);
		scope.activeTab = 2;
		scope.toggles.otherSkillsLikeEmail = true;
		scope.selectedSkillChange(skills[1]);
		$httpBackend.flush();
		expect(scope.viewObj.forecastedStaffing.updatedSeries.length).toEqual(1);
	});
});
