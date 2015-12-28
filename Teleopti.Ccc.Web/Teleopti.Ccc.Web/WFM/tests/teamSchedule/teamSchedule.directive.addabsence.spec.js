describe('team schedule add absence tests', function() {

	var compiler,
		rootScope,
		scope;

	beforeEach(function() {
		module('wfm.teamSchedule');
		module('wfm.templates');
		module('ui.bootstrap');
		module('wfm.notice');

		module(function ($provide) {
			$provide.service('Toggle', setupMockAllTrueToggleService);
		});

		inject(function ($rootScope, $httpBackend, $compile) {
			compiler = $compile;
			rootScope = $rootScope;
			scope = $rootScope.$new();
			$httpBackend.expectGET("../api/Absence/GetAvailableAbsences").respond(200, 'mock');
		});
	});


	it('should handle default start and end time attribute', function () {
		var background = {
			startDate: new Date('2015-01-01 10:00:00'),
			permissions: { IsAddIntradayAbsenceAvailable: false, IsAddFullDayAbsenceAvailable: false },
			getSelectedPersonIdList: {}
		};
		var addAbsence = compileAddAbsenceTag(background);

		var startDateString = addAbsence[0].querySelectorAll('team-schedule-datepicker input')[0].value,

			endDateString = addAbsence[0].querySelectorAll('team-schedule-datepicker input')[1].value,

			startTimeString = addAbsence[0].querySelectorAll('.timepicker input')[0].value
							+ addAbsence[0].querySelectorAll('.timepicker input')[1].value,

			endTimeString = addAbsence[0].querySelectorAll('.timepicker input')[2].value
						  + addAbsence[0].querySelectorAll('.timepicker input')[3].value;

		expect(startDateString).toBe('1/1/15');
		expect(startTimeString).toBe('1000');
		expect(endDateString).toBe('1/1/15');
		expect(endTimeString).toBe('1100');
	});

	it('should display full day absence check box with only full day absence permission', function () {
		var background = {
			startDate: new Date('2015-01-01 10:00:00'),
			permissions: { IsAddIntradayAbsenceAvailable: false, IsAddFullDayAbsenceAvailable: true },
			getSelectedPersonIdList: {}
		};
		var addAbsence = compileAddAbsenceTag(background);

		var checkBoxInput = addAbsence[0].querySelectorAll('.wfm-checkbox input#is-full-day');

		expect(checkBoxInput[0].value).toBe('on');
		expect(checkBoxInput[0].disabled).toBe(true);
	});

	it('should not display full day absence check box with only intraday absence permission', function () {
		var background = {
			startDate: new Date('2015-01-01 10:00:00'),
			permissions: { IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false},
			getSelectedPersonIdList: {}
		};
		var addAbsence = compileAddAbsenceTag(background);

		var checkBoxInput = addAbsence[0].querySelectorAll('.wfm-checkbox input#is-full-day');

		expect(checkBoxInput.length).toBe(0);
	});


	function compileAddAbsenceTag(params) {
		scope.vm = params;
		var addAbsenceTag = '<add-absence default-date-time="vm.startDate" permissions="vm.permissions" agent-id-list="vm.getSelectedPersonIdList()"/>';
		var addAbsence = compiler(addAbsenceTag)(scope);
		rootScope.$apply();
		return addAbsence;
	}

	function setupMockAllTrueToggleService() {
		return {
			WfmTeamSchedule_FindScheduleEasily_35611: true,
			WfmTeamSchedule_NoReadModel_35609: true,
			WfmTeamSchedule_SetAgentsPerPage_36230: true,
			WfmTeamSchedule_AbsenceReporting_35995: true,
			WfmPeople_AdvancedSearch_32973: true,
			WfmTeamSchedule_SwapShifts_36231: true
		};
	}

});