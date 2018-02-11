describe("team schedule command menu directive test", function () {
	var $compile,
		$rootScope,
		$httpBackend,
		personSelectionSvc,
		validateRulesService,
		permissions,
		toggles,
		overlappedWarningPersonId = ['12345'],
		noneOverlappedWarningPersonId = ['67890'];

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	var fakeShortCuts = {
		registerKeySequence: function () { }
	};

	beforeEach(function () {
		personSelectionSvc = new FakePersonSelection();
		validateRulesService = new FakeValidateRulesService();
		permissions = new FakePermissions();
		toggles = new FakeToggles();


		module(function ($provide) {
			$provide.value('ShortCuts', function () {
				return fakeShortCuts;
			}());
			$provide.value('keyCodes', function () { });
			$provide.service('PersonSelection', function () {
				return personSelectionSvc;
			});

			$provide.service('ValidateRulesService', function () {
				return validateRulesService;
			});

			$provide.service('teamsPermissions', function () { return permissions; });
			$provide.service('teamsToggles', function () { return toggles; });
		});

	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;

		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
	}));

	it('should view menu when add absence is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			IsAddFullDayAbsenceAvailable: true,
			IsAddIntradayAbsenceAvailable: true
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list  #menuItemAddAbsence'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when add activity is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			HasAddingActivityPermission: true
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemAddActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when add personal activity is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			HasAddingPersonalActivityPermission: true
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemAddPersonalActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when move activity is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			HasMoveActivityPermission: true
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view move activity menu when move overtime is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			HasMoveOvertimePermission: true
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when swap shift is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			IsSwapShiftsAvailable: true
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemSwapShifts'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when remove absence is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			IsRemoveAbsenceAvailable: true
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveAbsence'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when remove activity is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			HasRemoveActivityPermission: true
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when undo schedule is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		var element = $compile(html)(scope);
		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemUndo'));
		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view day off menu when toggle is on', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		var element = $compile(html)(scope);
		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItemForAddingDayOff = angular.element(element[0].querySelector('.wfm-list #menuItemAddDayOff span'));
		var menuListItemForRemovingDayOff = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveDayOff span'));
		expect(menu.length).toBe(1);
		expect(menuListItemForAddingDayOff[0].innerText).toBe('AddDayOff');
		expect(menuListItemForRemovingDayOff[0].innerText).toBe('RemoveDayOff');
	});

	it('should add day off menu item unclickable unless at least one person is selected', function () {
		var date = "2018-01-16";
		var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: date
		};

		var element = $compile(html)(scope);
		scope.$apply();

		var menuListItemForAddingDayOff = angular.element(element[0].querySelector('.wfm-list #menuItemAddDayOff'));
		expect(menuListItemForAddingDayOff[0].disabled).toEqual(true);

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				SelectedDayOffs: [],
				SelectedAbsences: []
			}];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		scope.$apply();

		menuListItemForAddingDayOff = angular.element(element[0].querySelector('.wfm-list #menuItemAddDayOff'));
		expect(menuListItemForAddingDayOff[0].disabled).toEqual(false);
	});

	function commonTestsInDifferentLocale() {
		it('should remove day off menu item unclickable unless day off on view date is selected', function () {
			var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
			var scope = $rootScope.$new();
			var date = "2018-01-16";
			scope.vm = {
				toggleCurrentSidenav: function () { },
				selectedDate: new Date(date)
			};
			var element = $compile(html)(scope);
			scope.$apply();

			var menuListItemForRemovingDayOff = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveDayOff'));
			expect(menuListItemForRemovingDayOff[0].disabled).toEqual(true);


			var selectedAgents = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					SelectedDayOffs: [{ Date: '2018-01-15' }]
				}];
			personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
			scope.$apply();

			menuListItemForRemovingDayOff = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveDayOff'));
			expect(menuListItemForRemovingDayOff[0].disabled).toEqual(true);


			var selectedAgents = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					SelectedDayOffs: [{ Date: date }]
				}];
			personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
			scope.$apply();

			menuListItemForRemovingDayOff = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveDayOff'));
			expect(menuListItemForRemovingDayOff[0].disabled).toEqual(false);


		});
	}

	describe('in locale ar-AE', function () {
		beforeAll(function () {
			moment.locale('ar-AE');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	describe('in locale fa-IR', function () {
		beforeAll(function () {
			moment.locale('fa-IR');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	it('should not view remove shift menu unless toggle is on', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		var element = $compile(html)(scope);
		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift span'));
		expect(menu.length).toBe(1);
		expect(menuListItemForRemoveShift[0].innerText).toBe('RemoveShift');

		element.isolateScope().vm.toggles.WfmTeamSchedule_RemoveShift_46322 = false;
		scope.$apply();
		menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift span'));
		expect(menuListItemForRemoveShift.length).toBe(0);
	});

	it('should not view removing shift menu when user does not have permission and toggle is on', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};
		permissions.set({
			HasRemoveShiftPermission: false
		});

		var element = $compile(html)(scope);
		scope.$apply();

		var menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift'));
		expect(menuListItemForRemoveShift.length).toEqual(0);
	});

	it('should remove shift menu item inactive when only full day absence is selected', function () {
		var date = "2018-02-01";
		var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: date
		};

		var element = $compile(html)(scope);
		scope.$apply();

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				IsFullDayAbsence: true,
				SelectedDayOffs: [],
				SelectedAbsences: ['absence']
			}];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		scope.$apply();

		var menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift'));
		expect(menuListItemForRemoveShift[0].disabled).toEqual(true);
	});

	it("should remove shift menu item unclickable when only one person with day off is selected", function () {
		var date = "2018-01-16";
		var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: date
		};
		var element = $compile(html)(scope);
		scope.$apply();

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				SelectedDayOffs: [{ Date: date }]
			}];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		scope.$apply();

		menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift'));
		expect(menuListItemForRemoveShift[0].disabled).toEqual(true);
	});

	it("should remove shift menu item unclickable when having day off and partial shift being selected", function () {
		var date = "2018-01-16";
		var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: date
		};
		var element = $compile(html)(scope);
		scope.$apply();

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				SelectedDayOffs: [{ Date: date }]
			},
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: false,
				SelectedActivities: ['activity1'],
				SelectedDayOffs:[]
			}
		];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		scope.$apply();
		
		menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift'));
		expect(menuListItemForRemoveShift[0].disabled).toEqual(true);
	});

	it("should remove shift menu item unclickable when having full day absence and partial shift being selected", function () {
		var date = "2018-01-16";
		var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: date
		};
		var element = $compile(html)(scope);
		scope.$apply();

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				IsFullDayAbsence: true,
				SelectedAbsences: ["absence"],
				SelectedDayOffs: []
			},
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: false,
				SelectedActivities: ['activity1'],
				SelectedDayOffs: []
			}
		];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		scope.$apply();

		menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift'));
		expect(menuListItemForRemoveShift[0].disabled).toEqual(true);
	});

	it("should remove shift menu item unclickable when checked person has empty schedule", function () {
		var date = "2018-01-16";
		var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: date
		};
		var element = $compile(html)(scope);
		scope.$apply();

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				IsFullDayAbsence: false,
				SelectedAbsences: [],
				SelectedDayOffs: [],
				SelectedActivities: []
			}
		];

		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		scope.$apply();

		menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift'));
		expect(menuListItemForRemoveShift[0].disabled).toEqual(true);
	});

	it("should remove shift menu item unclickable when checked person only has overtime activity", function () {
		var date = "2018-01-16";
		var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: date
		};
		var element = $compile(html)(scope);
		scope.$apply();

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				IsFullDayAbsence: false,
				SelectedAbsences: [],
				SelectedDayOffs: [],
				SelectedActivities: [{ isOvertime: true}]
			}
		];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		scope.$apply();

		menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift'));
		expect(menuListItemForRemoveShift[0].disabled).toEqual(true);

	});

	it('should remove shift menu item clickable when one person with shift is checked', function () {
		var date = "2018-02-01";
		var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: date
		};
		var element = $compile(html)(scope);
		scope.$apply();

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				SelectedDayOffs: [],
				SelectedActivities: ['activites']
			}
		];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		scope.$apply();

		var menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift'));
		expect(menuListItemForRemoveShift[0].disabled).toEqual(false);
	});

	it("should remove shift menu item clickable when having shift with intraday absence", function () {
		var date = "2018-01-16";
		var html = '<teamschedule-command-menu selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: date
		};
		var element = $compile(html)(scope);
		scope.$apply();

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				IsFullDayAbsence: false,
				SelectedAbsences: ["absence"],
				SelectedDayOffs: [],
				SelectedActivities: ['activity1']
			}
		];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		scope.$apply();

		menuListItemForRemoveShift = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveShift'));
		expect(menuListItemForRemoveShift[0].disabled).toEqual(false);
	});



	it('should view menu when move invalid overlapped activity is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			HasMoveInvalidOverlappedActivityPermission: true
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveInvalidOverlappedActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should make Move Invalid Overlapped Activity command menu clickable when requirements are met', function () {
		var html = '<teamschedule-command-menu validate-warning-enabled="validateWarningEnabled"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			HasMoveInvalidOverlappedActivityPermission: true
		});

		scope.validateWarningEnabled = true;

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				SelectedDayOffs: [],
				SelectedActivities: ['activites']
			}
		];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		personSelectionSvc.fakeSetCheckedPersonIds(overlappedWarningPersonId);

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveInvalidOverlappedActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
		expect(menuListItem[0].disabled).toBe(false);
	});

	it('should make Move Invalid Overlapped Activity command menu clickable when there exits overlap warnings', function () {
		var html = '<teamschedule-command-menu validate-warning-enabled="validateWarningEnabled""></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};

		permissions.set({
			HasMoveInvalidOverlappedActivityPermission: true
		});

		scope.validateWarningEnabled = true;

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				SelectedDayOffs: [],
				SelectedActivities: ['activites']
			}
		];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		personSelectionSvc.fakeSetCheckedPersonIds(overlappedWarningPersonId);

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveInvalidOverlappedActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
		expect(menuListItem[0].disabled).toBe(false);
	});

	it('should not view active Move Shift command menu when no shift is selected', function () {
		var html = '<teamschedule-command-menu  selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		var date = "2018-01-16";
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: new Date(date)
		};
		permissions.set({
			HasMoveActivityPermission: true
		});

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				SelectedDayOffs: [],
				SelectedActivities: ['activites']
			}
		];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		personSelectionSvc.setSelectedPersonAndProjectionCount({
			CheckedPersonCount: 1,
			SelectedActivityInfo: {
				PersonCount: 0,
				ActivityCount: 0
			},
			SelectedAbsenceInfo: {
				PersonCount: 0,
				AbsenceCount: 0
			}
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveShift'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
		expect(menuListItem[0].disabled).toBe(true);
	});

	it('should not view active Edit Shift Category command menu when no shift is selected', function () {
		var html = '<teamschedule-command-menu  selected-date="vm.selectedDate"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		var date = "2018-01-16";
		scope.vm = {
			toggleCurrentSidenav: function () { },
			selectedDate: new Date(date)
		};
		permissions.set({
			HasEditShiftCategoryPermission: true
		});

		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				SelectedDayOffs: [],
				SelectedActivities: ['activites']
			}
		];
		personSelectionSvc.setFakeCheckedPersonInfoList(selectedAgents);
		personSelectionSvc.setSelectedPersonAndProjectionCount({
			CheckedPersonCount: 1,
			SelectedActivityInfo: {
				PersonCount: 0,
				ActivityCount: 0
			},
			SelectedAbsenceInfo: {
				PersonCount: 0,
				AbsenceCount: 0
			}
		});

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemEditShiftCategory'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
		expect(menuListItem[0].disabled).toBe(true);
	});

	// Don't check this, as it performs badly when select all agents on every page is set.
	xit('should make Move Invalid Overlapped Activity command menu clickable when there is none overlap warning', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function () { }
		};


		personSelectionSvc.hasAgentSelected(true);
		personSelectionSvc.fakeSetCheckedPersonIds(noneOverlappedWarningPersonId);

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveInvalidOverlappedActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
		expect(menuListItem[0].disabled).toBe(true);
	});

	function FakePersonSelection() {
		var personIds = [];
		var hasSelected = false;
		var fakePersonList = [];
		var totalSelectedPersonAndProjectionCount = {
			CheckedPersonCount: 0,
			SelectedActivityInfo: {
				PersonCount: 0,
				ActivityCount: 0
			},
			SelectedAbsenceInfo: {
				PersonCount: 0,
				AbsenceCount: 0
			}
		};
		this.anyAgentChecked = function () {
			return fakePersonList.filter(function (p) {
				return p.Checked;
			}).length > 0;
		}
		this.setSelectedPersonAndProjectionCount = function (input) {
			totalSelectedPersonAndProjectionCount = input;
		}
		this.getTotalSelectedPersonAndProjectionCount = function () {
			return totalSelectedPersonAndProjectionCount;
		};
		this.isAnyAgentSelected = function () {
			return false;
		};
		this.canSwapShifts = function () {
			return false;
		};
		this.fakeSetCheckedPersonIds = function (data) {
			personIds = data;
		};
		this.getCheckedPersonIds = function () {
			return personIds;
		};
		this.getCheckedPersonInfoList = function () {
			return fakePersonList;
		};
		this.setFakeCheckedPersonInfoList = function (input) {
			fakePersonList = input;
		}

	}

	function FakeToggles() {
		var _toggles = {
			WfmTeamSchedule_AddNDeleteDayOff_40555: true,
			WfmTeamSchedule_RemoveShift_46322: true
		};
		this.set = function (toggles) {
			_toggles = toggles;
		}
		this.all = function () {
			return _toggles;
		}
	}

	function FakePermissions() {
		var _permissions = {
			HasRemoveShiftPermission: true
			};
		this.set = function (permissions) {
			_permissions = permissions;
		}
		this.all = function () {
			return _permissions;
		}
	}

	function FakeValidateRulesService() {
		var warningDict = {
			"12345": {
				"isLoaded": true,
				"warnings": [
					{
						"RuleType": "NotOverwriteLayerRuleName",
						"Content": "OverwriteLayerWarnings"
					}]
			},
			"67890": {
				"isLoaded": true,
				"warnings": [
					{

					}]
			},
		};

		this.checkValidationForPerson = function (personId, filteredRuleType) {
			if (!warningDict[personId]) return [];
			var result = warningDict[personId].warnings.filter(function (w) {
				if (filteredRuleType)
					return filteredRuleType == w.RuleType;
				return currentEnabledTypes[w.RuleType];
			}).map(function (w) {
				return w.Content;
			});

			return result;
		};
	}
});