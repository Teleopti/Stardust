'use strict';
describe('IntradayAreaController', function() {
	var $httpBackend,
		$controller,
		$filter,
		scope,
		$translate,
		$interval,
		NoticeService,
		currentUserInfo = new FakeCurrentUserInfo();

	var vm;
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

		module(function($provide) {
			$provide.service('Toggle', function() {
				return {
					Wfm_Intraday_OptimalStaffing_40921: true,
					Wfm_Intraday_ScheduledStaffing_41476: true,
					Wfm_Intraday_ESL_41827: true,
					WFM_Intraday_Show_For_Other_Days_43504: false,
					WFM_Remember_My_Selection_In_Intraday_47254: false,
					togglesLoaded: {
						then: function(cb) {
							cb();
						}
					}
				};
			});
		});

		module(function($provide) {
			$provide.service('CurrentUserInfo', function() {
				return currentUserInfo;
			});
		});
	});

	beforeEach(function() {
		skills = [
			{
				Id: '5f15b334-22d1-4bc1-8e41-72359805d30f',
				Name: 'skill x',
				DoDisplayData: true,
				ShowAbandonRate: true,
				ShowReforecastedAgents: true
			},
			{
				Id: '502632DC-7A0C-434D-8A75-3153D5160787',
				Name: 'skill y',
				DoDisplayData: true,
				ShowAbandonRate: false,
				ShowReforecastedAgents: false
			}
		];

		skillsWithFirstUnsupported = [
			{ Id: '63b0ac3d-06b5-42d7-bb0a-d7b2fd272196', Name: 'Unsupported skill1', DoDisplayData: false },
			{ Id: '22b0ac3d-06b5-42d7-bb0a-d7b2fd272196', Name: 'Supported skill1', DoDisplayData: true }
		];

		skillAreas = [
			{ Id: 'fa9b5393-ef48-40d1-b7cc-09e797589f81', Name: 'my skill area 1', Skills: skills },
			{ Id: '836cebb6-cee8-41a1-bb62-729f4b3a63f4', Name: 'my skill area 2', Skills: skills },
			{
				Id: '3f43f39b-f01b-47e7-b59f-35fc09fd5e41',
				Name: 'my unsupported skill area 1',
				Skills: skillsWithFirstUnsupported,
				UnsupportedSkills: skillsWithFirstUnsupported
			}
		];

		skillAreaInfo = { HasPermissionToModifySkillArea: true, SkillAreas: skillAreas };

		trafficAndPerformanceData = {
			LatestActualIntervalEnd: '0001-01-01T16:00:00',
			LatestActualIntervalStart: '0001-01-01T15:45:00',
			IncomingTrafficHasData: true,
			DataSeries: {
				AbandonedRate: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageHandleTime: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					170.8660714285,
					170.9112252
				],
				AverageSpeedOfAnswer: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ForecastedAverageHandleTime: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					199.8660714285,
					201.3520471464
				],
				ForecastedCalls: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0.0112,
					0.3224,
					0.5169,
					0.7337,
					0.9672
				],
				CalculatedCalls: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ServiceLevel: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				Time: ['0001-01-01T00:00:00', '0001-01-01T00:15:00', '0001-01-01T00:30:00', '0001-01-01T00:45:00']
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
			LatestActualIntervalEnd: '0001-01-02T16:00:00',
			LatestActualIntervalStart: '0001-01-02T15:45:00',
			IncomingTrafficHasData: true,
			DataSeries: {
				AbandonedRate: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageHandleTime: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageSpeedOfAnswer: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ForecastedAverageHandleTime: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					199.8660714285,
					201.3520471464
				],
				ForecastedCalls: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0.0112,
					0.3224,
					0.5169,
					0.7337,
					0.9672
				],
				CalculatedCalls: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ServiceLevel: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				Time: ['0001-01-02T00:00:00', '0001-01-02T00:15:00', '0001-01-02T00:30:00', '0001-01-02T00:45:00']
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
				ServiceLevel: 0.9,
				SpeedOfAnswer: 32296
			}
		};

		performanceData = {
			LatestActualIntervalEnd: '0001-01-01T16:00:00',
			LatestActualIntervalStart: '0001-01-01T15:45:00',
			PerformanceHasData: true,
			DataSeries: {
				AbandonedRate: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageSpeedOfAnswer: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				EstimatedServiceLevels: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ServiceLevel: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				Time: ['0001-01-01T00:00:00', '0001-01-01T00:15:00', '0001-01-01T00:30:00', '0001-01-01T00:45:00']
			},
			Summary: {
				AbandonRate: 0.05594855305466238,
				AverageSpeedOfAnswer: 22,
				ServiceLevel: 0.8392282958199357,
				EstimatedServiceLevel: 0.8735783757893
			}
		};

		performanceDataTomorrow = {
			LatestActualIntervalEnd: '0001-01-01T16:00:00',
			LatestActualIntervalStart: '0001-01-01T15:45:00',
			PerformanceHasData: true,
			DataSeries: {
				AbandonedRate: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageSpeedOfAnswer: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				EstimatedServiceLevels: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ServiceLevel: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				Time: ['0001-01-01T00:00:00', '0001-01-01T00:15:00', '0001-01-01T00:30:00', '0001-01-01T00:45:00']
			},
			Summary: {
				AbandonRate: 0.05594855305466238,
				AverageSpeedOfAnswer: 22,
				ServiceLevel: 0.9,
				EstimatedServiceLevel: 0.8735783757893
			}
		};

		timeData = { latestIntervalTime: { StartTime: {}, EndTime: {} } };
		staffingData = {
			StaffingHasData: true,
			DataSeries: {
				ForecastedStaffing: [1, 2, 3],
				UpdatedForecastedStaffing: [2, 3, 4],
				ActualStaffing: [2, 3, 4],
				Time: ['2016-08-30T00:00:00', '2016-08-30T00:15:00'],
				ScheduledStaffing: [1, 2, 3]
			}
		};

		staffingDataTomorrow = {
			StaffingHasData: true,
			DataSeries: {
				ForecastedStaffing: [3, 2, 1],
				UpdatedForecastedStaffing: [2, 3, 4],
				ActualStaffing: [2, 3, 4],
				Time: ['2016-08-30T00:00:00', '2016-08-30T00:15:00'],
				ScheduledStaffing: [1, 2, 3]
			}
		};

		emptyStaffingData = { DataSeries: null };
	});

	function initBackend() {
		skillAreas.forEach(function(item) {
			$httpBackend.whenGET('../api/intraday/monitorskillareaperformance/' + item.Id + '/0').respond(function() {
				return [200, performanceData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareaperformance/' + item.Id + '/1').respond(function() {
				return [200, performanceDataTomorrow];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareastatistics/' + item.Id + '/0').respond(function() {
				return [200, trafficAndPerformanceData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillareastatistics/' + item.Id + '/1').respond(function() {
				return [200, trafficAndPerformanceDataTomorrow];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id).respond(function() {
				return [200, staffingData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id + '/0').respond(function() {
				return [200, staffingData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id + '/1').respond(function() {
				return [200, staffingDataTomorrow];
			});
			$httpBackend
				.whenGET(
					'../api/intraday/monitorskillareastatisticsbydate?dateUtc=0001-01-01T14:45:00.000Z&id=' + item.Id
				)
				.respond(function() {
					return [200, trafficAndPerformanceData];
				});

			$httpBackend
				.whenGET(
					'../api/intraday/monitorskillareastatisticsbydate?dateUtc=0001-01-02T14:45:00.000Z&id=' + item.Id
				)
				.respond(function() {
					return [200, trafficAndPerformanceDataTomorrow];
				});
			$httpBackend
				.whenGET(
					'../api/intraday/monitorskillareastaffing?DateTime=0001-01-01T14:45:00.000Z&SkillAreaId=' +
						item.Id +
						'&UseShrinkage=false'
				)
				.respond(function() {
					return [200, staffingData];
				});

			$httpBackend
				.whenGET(
					'../api/intraday/monitorskillareastaffing?DateTime=0001-01-02T14:45:00.000Z&SkillAreaId=' +
						item.Id +
						'&UseShrinkage=false'
				)
				.respond(function() {
					return [200, staffingDataTomorrow];
				});
			$httpBackend.whenGET('../api/intraday/lateststatisticstimeforskillarea/' + item.Id).respond(function() {
				return [200, timeData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id).respond(function() {
				return [200, emptyStaffingData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id).respond(function() {
				return [200, staffingData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareaperformance/' + item.Id).respond(function() {
				return [200, performanceData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareastatistics/' + item.Id).respond(function() {
				return [200, trafficAndPerformanceData];
			});
			$httpBackend.whenDELETE('../api/skillgroup/delete/' + item.Id).respond(200, {});
		});
		skills.forEach(function(item) {
			$httpBackend.whenGET('../api/intraday/monitorskillstatistics/' + item.Id + '/0').respond(function() {
				return [200, trafficAndPerformanceData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillstatistics/' + item.Id + '/1').respond(function() {
				return [200, trafficAndPerformanceDataTomorrow];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillstaffing/' + item.Id + '/0').respond(function() {
				return [200, staffingData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillstaffing/' + item.Id + '/1').respond(function() {
				return [200, staffingDataTomorrow];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillperformance/' + item.Id + '/0').respond(function() {
				return [200, performanceData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillperformance/' + item.Id + '/1').respond(function() {
				return [200, performanceDataTomorrow];
			});
			$httpBackend.whenGET('../api/intraday/lateststatisticstimeforskill/' + item.Id).respond(function() {
				return [200, timeData];
			});
			$httpBackend.whenGET('../api/intraday/lateststatisticstimeforskill/' + item.Id).respond(function() {
				return [200, timeData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillstaffing/' + item.Id).respond(function() {
				return [200, staffingData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillperformance/' + item.Id).respond(function() {
				return [200, performanceData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillstatistics/' + item.Id).respond(function() {
				return [200, trafficAndPerformanceData];
			});
		});
	}

	beforeEach(
		inject(function(
			_$httpBackend_,
			_$controller_,
			_$rootScope_,
			_$filter_,
			_$translate_,
			_$interval_,
			_NoticeService_
		) {
			$controller = _$controller_;
			$httpBackend = _$httpBackend_;
			$filter = _$filter_;
			$translate = _$translate_;
			scope = _$rootScope_.$new();
			$interval = _$interval_;
			NoticeService = _NoticeService_;

			$httpBackend.whenGET('../api/skillgroup/skillgroups').respond(function() {
				return [200, skillAreaInfo];
			});

			$httpBackend.whenGET('../api/intraday/skills').respond(function() {
				if (isUnsupportedSkillTest) {
					return [200, skillsWithFirstUnsupported];
				} else {
					return [200, skills];
				}
			});

			initBackend();
		})
	);

	function FakeCurrentUserInfo() {
		this.CurrentUserInfo = function() {
			return { DefaultTimeZone: 'America/Denver' };
		};
	}

	var createController = function(isNewlyCreatedSkillArea) {
		vm = $controller('IntradayAreaController', { $scope: scope, $translate: $translate });

		vm.onStateChanged(undefined, { name: 'intraday.area' }, { isNewSkillArea: isNewlyCreatedSkillArea });
		scope.$digest();
		$httpBackend.flush();
	};

	it('should stop polling when page is about to destroy', function() {
		createController(false);
		$interval(function() {
			scope.$emit('$destroy');
		}, 5000);
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should display list of skill areas', function() {
		createController(false);

		expect(vm.skillAreas[0].Id).toEqual('fa9b5393-ef48-40d1-b7cc-09e797589f81');
		expect(vm.skillAreas[0].Name).toEqual('my skill area 1');
		expect(vm.skillAreas[0].Skills[0].Id).toEqual('5f15b334-22d1-4bc1-8e41-72359805d30f');
		expect(vm.skillAreas[0].Skills[0].Name).toEqual('skill x');
	});

	it('should display list of skills', function() {
		createController(false);

		expect(vm.skills[0].Id).toEqual('5f15b334-22d1-4bc1-8e41-72359805d30f');
		expect(vm.skills[0].Name).toEqual('skill x');
	});

	it('should delete selected skill area when toggle is disabled', function() {
		createController(false);

		vm.toggles['WFM_Modify_Skill_Groups_43727'] = false;
		vm.deleteSkillArea(vm.skillAreas[1]);

		expect(vm.skillAreas.length).toEqual(3);
		$httpBackend.flush();

		expect(vm.selectedItem).toEqual(null);
		expect(vm.skillAreas.length).toEqual(2);
	});

	// Removed due to new behaviour: Anders Sjöberg 2018-03-20
	// it('should monitor first skill if no skill areas', function() {
	// 	skillAreaInfo.SkillAreas = [];
	// 	createController(false);

	// 	vm.skillSelected(vm.skills[0]);
	// 	expect(vm.selectedItem).toEqual(vm.skills[0]);
	// });

	it('should monitor first skill area if there are any', function() {
		createController(false);

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);

		expect(vm.selectedItem).toEqual(vm.skillAreas[0]);
	});

	it('should have permission to modify skill area', function() {
		createController(false);

		expect(vm.HasPermissionToModifySkillArea).toEqual(true);
	});

	it('should poll data for skill when selecting that skill', function() {
		createController(false);
		vm.activeTab = 0;

		vm.selectSkillOrSkillArea(vm.skills[0]);
		$httpBackend.flush();
		expect(vm.viewObj.hasMonitorData).toEqual(true);
	});

	it('should poll data for skill area when selecting that area', function() {
		createController(false);
		vm.activeTab = 0;
		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.hasMonitorData).toEqual(true);
	});

	it('should only poll traffic skill data when traffic tab and skill is selected', function() {
		createController(false);
		vm.activeTab = 0;

		vm.selectSkillOrSkillArea(vm.skills[0]);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedCallsObj.series.length).toBeGreaterThan(5);
	});

	it('should only poll performance skill data when performance tab and skill is selected', function() {
		createController(false);
		vm.activeTab = 1;

		vm.selectSkillOrSkillArea(vm.skills[0]);
		$httpBackend.flush();

		expect(vm.viewObj.serviceLevelObj.series.length).toBeGreaterThan(5);
	});

	it('should show estimated service level in performance tab and skill is selected', function() {
		createController(false);
		vm.activeTab = 1;

		vm.selectSkillOrSkillArea(vm.skills[0]);
		$httpBackend.flush();

		expect(vm.viewObj.estimatedServiceLevelObj.series.length).toBeGreaterThan(0);
		expect(vm.viewObj.summary.summaryEstimatedServiceLevel).toBeGreaterThan(0);
	});

	it('should only poll staffing skill data when staffing tab and skill is selected', function() {
		createController(false);
		vm.activeTab = 2;

		vm.selectSkillOrSkillArea(vm.skills[0]);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedStaffing.series.length).toBeGreaterThan(3);
	});

	it('should only poll traffic skill area data when traffic tab and skill area is selected', function() {
		createController(false);
		vm.activeTab = 0;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedCallsObj.series.length).toBeGreaterThan(5);
	});

	it('should only poll performance skill area data when performance tab and skill area is selected', function() {
		createController(false);
		vm.activeTab = 1;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.serviceLevelObj.series.length).toBeGreaterThan(5);
	});

	it('should only poll staffing skill area data when staffing tab and skill area is selected', function() {
		createController(false);
		vm.activeTab = 2;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedStaffing.series.length).toBeGreaterThan(3);
	});

	// it('should return no staffing when no supported skills in skill area', function() {
	// 	isUnsupportedSkillTest = true;
	// 	createController(false);
	// 	vm.activeTab = 2;

	// 	vm.selectSkillOrSkillArea(vm.skillAreas[3]);
	// 	$httpBackend.flush();

	// 	expect(vm.viewObj.forecastedStaffing.series.length).toEqual(0);
	// });

	it('should show optimal staffing when toggle is enabled', function() {
		createController(false);
		vm.activeTab = 2;

		vm.toggles['Wfm_Intraday_OptimalStaffing_40921'] = true;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.actualStaffingSeries.length).toBeGreaterThan(3);
	});

	it('should not show optimal staffing when toggle is disabled', function() {
		createController(false);
		vm.activeTab = 2;

		vm.toggles['Wfm_Intraday_OptimalStaffing_40921'] = false;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.actualStaffingSeries.length).toEqual(1);
	});

	it('should show scheduled staffing when toggle is enabled', function() {
		createController(false);
		vm.activeTab = 2;

		vm.toggles['Wfm_Intraday_ScheduledStaffing_41476'] = true;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.scheduledStaffing.length).toBeGreaterThan(3);
	});

	it('should not show scheduled staffing when toggle is disabled', function() {
		createController(false);
		vm.activeTab = 2;

		vm.toggles['Wfm_Intraday_ScheduledStaffing_41476'] = false;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.scheduledStaffing.length).toEqual(1);
	});

	it('should show ESL in performance view when toggle is enabled', function() {
		createController(false);
		vm.activeTab = 1;

		vm.toggles['Wfm_Intraday_ESL_41827'] = true;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.estimatedServiceLevelObj.series.length).toBeGreaterThan(0);
		expect(vm.viewObj.summary.summaryEstimatedServiceLevel).toBeGreaterThan(0);
	});

	it('should show ESL in performance view when toggle is disabled', function() {
		createController(false);
		vm.activeTab = 1;

		vm.toggles['Wfm_Intraday_ESL_41827'] = false;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);
		$httpBackend.flush();

		expect(vm.viewObj.estimatedServiceLevelObj.series.length).toEqual(1);
		expect(vm.viewObj.summary.summaryEstimatedServiceLevel).toEqual(undefined);
	});

	// Removed due to new behaviour: Anders Sjöberg 2018-03-20
	// it('should monitor first skill that is supported', function() {
	// 	skillAreaInfo.SkillAreas = [];
	// 	isUnsupportedSkillTest = true;
	// 	createController(false);

	// 	vm.selectSkillOrSkillArea(skillsWithFirstUnsupported[0]);

	// 	expect(vm.selectedItem).toEqual(vm.skills[1]);
	// });

	it('should get traffic data corresponding to chosenOffset', function() {
		skillAreaInfo.SkillAreas = [];
		isUnsupportedSkillTest = false;

		createController(false);
		vm.activeTab = 0;
		vm.toggles['toggleSvc.WFM_Intraday_Show_For_Other_Days_43504'] = true;
		vm.selectSkillOrSkillArea(skills[0]);

		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryCalculatedCalls).toEqual('1,337.0');
	});

	it('should get performance data corresponding to chosenOffset', function() {
		skillAreaInfo.SkillAreas = [];
		isUnsupportedSkillTest = false;

		createController(false);
		vm.activeTab = 1;
		vm.toggles['toggleSvc.WFM_Intraday_Show_For_Other_Days_43504'] = true;
		vm.selectSkillOrSkillArea(skills[0]);
		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryServiceLevel).toEqual('90.0');
	});

	it('should get staffing data corresponding to chosenOffset', function() {
		skillAreaInfo.SkillAreas = [];
		isUnsupportedSkillTest = false;

		createController(false);
		vm.activeTab = 2;
		vm.toggles['toggleSvc.WFM_Intraday_Show_For_Other_Days_43504'] = true;
		vm.selectSkillOrSkillArea(skills[0]);
		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.forecastedStaffing.series[1]).toEqual(3);
	});

	it('should get traffic data for skillarea corresponding to chosenOffset', function() {
		createController(false);
		vm.activeTab = 0;
		vm.toggles['toggleSvc.WFM_Intraday_Show_For_Other_Days_43504'] = true;
		vm.selectSkillOrSkillArea(skillAreas[0]);
		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryCalculatedCalls).toEqual('1,337.0');
	});

	it('should get staffing data for skillarea corresponding to chosenOffset', function() {
		createController(false);
		vm.activeTab = 2;
		vm.toggles['toggleSvc.WFM_Intraday_Show_For_Other_Days_43504'] = true;
		vm.selectSkillOrSkillArea(skillAreas[0]);
		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.forecastedStaffing.series[1]).toEqual(3);
		jasmine.clock().uninstall();
	});

	it('should get traffic data for skillarea corresponding to chosenOffset', function() {
		createController(false);
		vm.activeTab = 0;
		vm.toggles['toggleSvc.WFM_Intraday_Show_For_Other_Days_43504'] = true;
		vm.selectSkillOrSkillArea(skillAreas[0]);

		var today = moment('0001-01-01T15:45:00').toDate();
		jasmine.clock().mockDate(today);
		vm.changeChosenOffset(1);
		$httpBackend.flush();
		expect(vm.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryCalculatedCalls).toEqual('1,337.0');

		jasmine.clock().uninstall();
	});

	it('should get performance data for skillarea corresponding to chosenOffset', function() {
		createController(false);
		vm.activeTab = 1;
		vm.toggles['toggleSvc.WFM_Intraday_Show_For_Other_Days_43504'] = true;
		vm.selectSkillOrSkillArea(skillAreas[0]);

		var today = moment('0001-01-01T15:45:00').toDate();
		jasmine.clock().mockDate(today);
		vm.changeChosenOffset(1);
		$httpBackend.flush();
		expect(vm.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryServiceLevel).toEqual('90.0');

		jasmine.clock().uninstall();
	});

	it('should get staffing data for skillarea corresponding to chosenOffset', function() {
		createController(false);
		vm.activeTab = 2;
		vm.toggles['toggleSvc.WFM_Intraday_Show_For_Other_Days_43504'] = true;
		vm.selectSkillOrSkillArea(skillAreas[0]);

		var today = moment('0001-01-01T15:45:00').toDate();
		jasmine.clock().mockDate(today);
		vm.changeChosenOffset(1);
		$httpBackend.flush();
		expect(vm.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.forecastedStaffing.series[1]).toEqual(3);
		jasmine.clock().uninstall();
	});

	it('should not show abandon rate data when toggle is enabled and email-like skill chosen', function() {
		createController(false);
		vm.activeTab = 1;
		vm.toggles['WFM_Intraday_SupportOtherSkillsLikeEmail_44026'] = true;

		vm.selectSkillOrSkillArea(skills[1]);

		$httpBackend.flush();

		expect(vm.viewObj.abandonedRateObj.series.length).toEqual(1);
		expect(vm.viewObj.summary.summaryAbandonedRate).toEqual(undefined);
	});

	it('should not show reforcasted agents data when toggle is enabled and email-like skill chosen', function() {
		createController(false);
		vm.activeTab = 2;
		vm.toggles['WFM_Intraday_SupportOtherSkillsLikeEmail_44026'] = true;
		vm.selectSkillOrSkillArea(skills[1]);
		$httpBackend.flush();
		expect(vm.viewObj.forecastedStaffing.updatedSeries.length).toEqual(1);
	});

	it('should display date in timezone for current user', function() {
		createController(false);
		vm.selectSkillOrSkillArea(skills[1]);
		var today = moment('2018-01-02T03:00:00').toDate();
		jasmine.clock().mockDate(today);
		$httpBackend.flush();
		var currentDateDenver = moment('2018-01-01T00:00:00')
			.format('dddd, LL')
			.toLowerCase();
		expect(vm.getLocalDate(0).toLowerCase()).toEqual(currentDateDenver);
		jasmine.clock().uninstall();
	});

	it('should round AHT data series for incomming traffic chart', function() {
		createController(false);
		vm.activeTab = 0;
		vm.selectSkillOrSkillArea(vm.skills[0]);
		$httpBackend.flush();

		var ahtSeries = vm.viewObj.actualAverageHandleTimeObj.series;
		var forcastedAhtSeries = vm.viewObj.forecastedAverageHandleTimeObj.series;
		expect(ahtSeries[ahtSeries.length - 1]).toEqual(170.9);
		expect(forcastedAhtSeries[forcastedAhtSeries.length - 1]).toEqual(201.4);
	});

	it('should remember selected tab when exiting if toggle is on', function() {
		createController(false);
		vm.toggles['WFM_Remember_My_Selection_In_Intraday_47254'] = true;
		vm.activeTab = 1;
		vm.saveState();
		vm.activeTab = 0;
		vm.loadState();

		expect(vm.activeTab).toEqual(1);
	});

	it('should remember selected day offset when exiting if toggle is on', function() {
		createController(false);
		vm.toggles['WFM_Remember_My_Selection_In_Intraday_47254'] = true;
		vm.changeChosenOffset(-5, true);
		vm.selectedItem = {};
		vm.saveState();
		vm.changeChosenOffset(0, true);
		vm.loadState();

		expect(vm.chosenOffset.value).toEqual(-5);
	});

	it('should NOT remember selected day offset when exiting if toggle is off', function() {
		createController(false);
		vm.toggles['WFM_Remember_My_Selection_In_Intraday_47254'] = false;
		vm.changeChosenOffset(-5, true);
		vm.selectedItem = {};
		vm.saveState();
		vm.changeChosenOffset(0, true);
		vm.loadState();

		expect(vm.chosenOffset.value).toEqual(0);
	});
});
