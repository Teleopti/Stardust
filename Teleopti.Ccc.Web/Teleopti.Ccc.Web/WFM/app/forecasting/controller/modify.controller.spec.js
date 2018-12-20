describe("ForecastModifyController", function () {
	var vm, $controller, $httpBackend, fakeBackend, skill, scenario, scenario2, forecastDays;

	sessionStorage.currentForecastWorkload = angular.toJson({
		ChartId: "123",
		SkillId: "xyz",
		Workload: {
			Id: "abc",
			Name: "workloadName",
			Accuracies: null
		},
		Accuracies: null,
		Id: "dfg",
		Name: "skillName"
	});

	beforeEach(function () {
		module("wfm.forecasting");

		inject(function (
			$rootScope,
			_$controller_,
			_$httpBackend_,
			_fakeForecastingBackend_
		) {
			scope = $rootScope.$new();
			$controller = _$controller_;
			$httpBackend = _$httpBackend_;
			fakeBackend = _fakeForecastingBackend_;

			vm = $controller("ForecastModifyController", { $scope: scope });

			skill = {
				IsPermittedToModifySkill: true,
				Skills: [
					{
						Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
						Workloads: [
							{
								Id: "b8a74a6c-3125-4c13-a19a-9f0800e35a1f",
								Name: "Channel Sales - Marketing",
								Accuracies: null
							}
						]
					}
				]
			};

			scenario = {
				Id: "e21d813c-238c-4c3f-9b49-9b5e015ab432",
				Name: "Default",
				DefaultScenario: true
			};

			scenario2 = {
				Id: "18b34548-3604-49fe-9584-9b5e015ab432",
				Name: "High",
				DefaultScenario: false
			};
		});
	});

	it("should handle stateParam data", function () {
		expect(vm.selectedWorkload.Id).toBe("dfg");
		expect(vm.selectedWorkload.Workload.Id).toBe("abc");
		expect(vm.selectedWorkload.Workload.Name).toBe("workloadName");
		expect(vm.selectedWorkload.Name).toBe("skillName");
	});

	it("should get workload with days", inject(function ($controller) {
		var tomorrow = moment()
			.add(1, "days");
		forecastDays = [
			{
				Date: tomorrow,
				AverageAfterTaskTime: 4.7,
				Tasks: 1135.2999999999997,
				TotalAverageAfterTaskTime: 4.7,
				TotalTasks: 1135.2999999999997,
				AverageTaskTime: 158.10038509999998,
				TotalAverageTaskTime: 158.10038509999998
			}
		];
		fakeBackend.withSkill(skill);
		fakeBackend.withForecastStatus(true);
		fakeBackend.withScenario(scenario);
		fakeBackend.withForecastData(forecastDays);
		$httpBackend.flush();

		expect(vm.isForecastRunning).toEqual(false);
		expect(vm.selectedWorkload.Days.length).toEqual(1);
		expect(vm.selectedScenario.Id).toEqual(scenario.Id);
	}));

	it("should set forecasting period to min and max of forecasted days", inject(function ($controller) {
		var tomorrow = moment()
			.add(1, "days");
		forecastDays = [
			{
				Date: tomorrow,
				AverageAfterTaskTime: 4.7,
				Tasks: 1135.2999999999997,
				TotalAverageAfterTaskTime: 4.7,
				TotalTasks: 1135.2999999999997,
				AverageTaskTime: 158.10038509999998,
				TotalAverageTaskTime: 158.10038509999998
			},
			{
				Date: tomorrow.add(2, "days"),
				AverageAfterTaskTime: 4.7,
				Tasks: 1135.2999999999997,
				TotalAverageAfterTaskTime: 4.7,
				TotalTasks: 1135.2999999999997,
				AverageTaskTime: 158.10038509999998,
				TotalAverageTaskTime: 158.10038509999998
			}
		];
		fakeBackend.withSkill(skill);
		fakeBackend.withForecastStatus(true);
		fakeBackend.withScenario(scenario);
		fakeBackend.withForecastData(forecastDays);
		$httpBackend.flush();

		var testStartDate = moment(vm.forecastPeriod.startDate).format('YYYY-MM-DD');
		var testEndDate = moment(vm.forecastPeriod.endDate).format('YYYY-MM-DD');
		expect(testStartDate).toEqual(tomorrow
			.format('YYYY-MM-DD'));
		expect(testEndDate).toEqual(moment()
			.add(3, "days")
			.format('YYYY-MM-DD')
		);
	}));
	
	it("should set forecasting period to next 6 months when no forecast", inject(function ($controller) {
		var tomorrow = moment()
			.add(1, "days");
		forecastDays = [];
		fakeBackend.withSkill(skill);
		fakeBackend.withForecastStatus(true);
		fakeBackend.withScenario(scenario);
		fakeBackend.withForecastData(forecastDays);
		$httpBackend.flush();

		var testStartDate = moment(vm.forecastPeriod.startDate).format('YYYY-MM-DD');
		var testEndDate = moment(vm.forecastPeriod.endDate).format('YYYY-MM-DD');
		expect(testStartDate).toEqual(
			tomorrow
				.format('YYYY-MM-DD'));
		expect(testEndDate).toEqual(moment()
			.add(6, "months")
			.format('YYYY-MM-DD')
		);
	}));

	it("should keep user selected forecasting period", inject(function ($controller) {
		var tomorrow = moment()
			.add(1, "days");
		forecastDays = [
			{
				Date: tomorrow,
				AverageAfterTaskTime: 4.7,
				Tasks: 1135.2999999999997,
				TotalAverageAfterTaskTime: 4.7,
				TotalTasks: 1135.2999999999997,
				AverageTaskTime: 158.10038509999998,
				TotalAverageTaskTime: 158.10038509999998
			}
		];
		fakeBackend.withSkill(skill);
		fakeBackend.withForecastStatus(true);
		fakeBackend.withScenario(scenario);
		fakeBackend.withForecastData(forecastDays);
		$httpBackend.flush();

		vm.forecastPeriod = {
			startDate: new Date(2018, 10, 1),
			endDate: new Date(2018, 10, 7)
		};

		var startDateBeforeLoadingForecast = vm.forecastPeriod.startDate;
		var endDateBeforeLoadingForecast = vm.forecastPeriod.endDate;

		vm.getWorkloadForecastData(true);
		$httpBackend.flush();

		expect(startDateBeforeLoadingForecast)
			.toEqual(vm.forecastPeriod.startDate);
		expect(endDateBeforeLoadingForecast)
			.toEqual(vm.forecastPeriod.endDate);
	}));

	it("should get correct data for export", inject(function () {
		vm.selectedScenario = scenario;

		expect(vm.selectedWorkload.Id).toEqual("dfg");
		expect(vm.selectedScenario.Id).toEqual(
			"e21d813c-238c-4c3f-9b49-9b5e015ab432"
		);
	}));

	it("should be able to forecast clean scenario", inject(function () {
		vm.changeScenario(scenario2, true);

		expect(vm.selectedWorkload.Days.length).toEqual(0);
		expect(vm.selectedScenario.Id).toEqual(scenario2.Id);

		vm.forecastWorkload();
		//should complete forecast without issue
	}));
});