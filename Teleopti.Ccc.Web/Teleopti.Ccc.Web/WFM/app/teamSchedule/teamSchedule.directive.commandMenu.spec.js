describe("team schedule command menu directive test", function() {
	var $compile,
		$rootScope,
		$httpBackend,
		personSelectionSvc;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	var fakeShortCuts = {
		registerKeySequence: function() {}
	};

	beforeEach(function() {
		personSelectionSvc = new FakePersonSelection();
		module(function($provide) {
			$provide.value('ShortCuts', function() {
				return fakeShortCuts;
			}());
			$provide.value('keyCodes', function() {});
			$provide.service('PersonSelection', function () {
				return personSelectionSvc;
			});
		});

	});

	beforeEach(inject(function(_$rootScope_, _$compile_, _$httpBackend_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;

		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
	}));

	function FakePersonSelection(){
		var hasSelected = false;
		this.hasAgentSelected = function(value){
			hasSelected = value;
		}
		this.anyAgentChecked = function(){
			return hasSelected;
		}
		this.getTotalSelectedPersonAndProjectionCount = function () {
			return {
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
		};
		this.isAnyAgentSelected = function () {
			return false;
		};
		this.canSwapShifts = function () {
			return false;
		};
	}

	it('should not view menu without any permitted', function() {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {},
			permissions: {}
		};
		
		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuList = angular.element(element[0].querySelector('.wfm-list'));

		expect(menu.length).toBe(0);
		expect(menuList.length).toBe(0);
	});

	it('should view menu when add absence is permitted', function() {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				AbsenceReportingEnabled: true
			},
			permissions: {
				IsAddFullDayAbsenceAvailable: true,
				IsAddIntradayAbsenceAvailable: true
			}
		};
		
		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list  #menuItemAddAbsence'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when add activity is permitted', function() {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				AddActivityEnabled: true
			},
			permissions: {
				HasAddingActivityPermission: true
			}
		};
		
		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemAddActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when add personal activity is permitted', function() {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				AddPersonalActivityEnabled: true
			},
			permissions: {
				HasAddingPersonalActivityPermission: true
			}
		};
		
		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemAddPersonalActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when move activity is permitted', function() {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				MoveActivityEnabled: true
			},
			permissions: {
				HasMoveActivityPermission: true
			}
		};
		
		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when swap shift is permitted', function() {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				SwapShiftEnabled: true
			},
			permissions: {
				IsSwapShiftsAvailable: true
			}
		};
		
		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemSwapShifts'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when remove absence is permitted', function() {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				RemoveAbsenceEnabled: true
			},
			permissions: {
				IsRemoveAbsenceAvailable: true
			}
		};
		
		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveAbsence'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when remove activity is permitted', function() {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				RemoveActivityEnabled: true
			},
			permissions: {
				HasRemoveActivityPermission: true
			}
		};
		
		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemRemoveActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should view menu when undo schedule is permitted', function() {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				UndoScheduleEnabled: true
			},
			permissions: {
				
			}
		};
		
		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemUndo'));
		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should not show menu item when toggle is disabled', function () {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				MoveInvalidOverlappedActivityEnabled: false
			},
		};

		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveInvalidOverlappedActivity'));

		expect(menu.length).toBe(0);
		expect(menuListItem.length).toBe(0);
	});

	it('should view menu when move invalid overlapped activity is permitted', function () {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				MoveInvalidOverlappedActivityEnabled: true
			},
			permissions: {
				HasMoveInvalidOverlappedActivityPermission: true
			},
		};

		scope.getConfigurations = function() {
			return config;
		};

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveInvalidOverlappedActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should make Move Invalid Overlapped Activity command menu clickable when requirements are met', function () {
		var html = '<teamschedule-command-menu configurations="getConfigurations()"></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
		var config = {
			toggles: {
				MoveInvalidOverlappedActivityEnabled: true
			},
			permissions: {
				HasMoveInvalidOverlappedActivityPermission: true
			},
			validateWarningToggle: true
		};

		scope.getConfigurations = function() {
			return config;
		};

		personSelectionSvc.hasAgentSelected(true);

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveInvalidOverlappedActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
		expect(menuListItem[0].disabled).toBe(false);
	});
})