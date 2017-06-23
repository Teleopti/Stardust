describe("team schedule command menu directive test", function() {
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
		registerKeySequence: function() {}
	};

	beforeEach(function() {
		personSelectionSvc = new FakePersonSelection();
		validateRulesService = new FakeValidateRulesService();
		permissions = new FakePermissions();
		toggles = new FakeToggles();


		module(function($provide) {
			$provide.value('ShortCuts', function() {
				return fakeShortCuts;
			}());
			$provide.value('keyCodes', function() {});
			$provide.service('PersonSelection', function () {
				return personSelectionSvc;
			});

			$provide.service('ValidateRulesService', function () {
				return validateRulesService;
			});

			$provide.service('teamsPermissions', function() { return permissions; });
			$provide.service('teamsToggles', function() { return toggles; });
		});

	});

	beforeEach(inject(function(_$rootScope_, _$compile_, _$httpBackend_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;

		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
	}));

	it('should not view menu without any permitted', function() {
		var html = '<teamschedule-command-menu></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};
					
		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuList = angular.element(element[0].querySelector('.wfm-list'));

		expect(menu.length).toBe(0);
		expect(menuList.length).toBe(0);
	});

	it('should view menu when add absence is permitted', function() {
		var html = '<teamschedule-command-menu></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
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

	it('should view menu when add activity is permitted', function() {
		var html = '<teamschedule-command-menu></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
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

	it('should view menu when add personal activity is permitted', function() {
		var html = '<teamschedule-command-menu></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
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

	it('should view menu when move activity is permitted', function() {
		var html = '<teamschedule-command-menu></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
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

	it('should view menu when swap shift is permitted', function() {
		var html = '<teamschedule-command-menu></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};

		toggles.set({
			SwapShiftEnabled: true
		});

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

	it('should view menu when remove absence is permitted', function() {
		var html = '<teamschedule-command-menu></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};

		toggles.set({
			RemoveAbsenceEnabled: true
		});

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

	it('should view menu when remove activity is permitted', function() {
		var html = '<teamschedule-command-menu></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};

		toggles.set({
			RemoveActivityEnabled: true
		});

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

	it('should view menu when undo schedule is permitted', function() {
		var html = '<teamschedule-command-menu></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};

		toggles.set({
			UndoScheduleEnabled: true
		});		

		var element = $compile(html)(scope);
		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemUndo'));
		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
	});

	it('should not show menu item when toggle is disabled', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};

		toggles.set({
			MoveInvalidOverlappedActivityEnabled: false
		});		

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveInvalidOverlappedActivity'));

		expect(menu.length).toBe(0);
		expect(menuListItem.length).toBe(0);
	});

	it('should view menu when move invalid overlapped activity is permitted', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
		};

		toggles.set({
			MoveInvalidOverlappedActivityEnabled: true
		});

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
			toggleCurrentSidenav: function() {}
		};

		toggles.set({
			MoveInvalidOverlappedActivityEnabled: true
		});

		permissions.set({
			HasMoveInvalidOverlappedActivityPermission: true
		});
		
		scope.validateWarningEnabled = true;

		personSelectionSvc.hasAgentSelected(true);
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
			toggleCurrentSidenav: function() {}
		};

		toggles.set({
			MoveInvalidOverlappedActivityEnabled: true
		});

		permissions.set({
			HasMoveInvalidOverlappedActivityPermission: true
		});

		scope.validateWarningEnabled = true;

		personSelectionSvc.hasAgentSelected(true);		
		personSelectionSvc.fakeSetCheckedPersonIds(overlappedWarningPersonId);

		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuListItem = angular.element(element[0].querySelector('.wfm-list #menuItemMoveInvalidOverlappedActivity'));

		expect(menu.length).toBe(1);
		expect(menuListItem.length).toBe(1);
		expect(menuListItem[0].disabled).toBe(false);
	});

	// Don't check this, as it performs badly when select all agents on every page is set.
	xit('should make Move Invalid Overlapped Activity command menu clickable when there is none overlap warning', function () {
		var html = '<teamschedule-command-menu></teamschedule-command-menu>';
		var scope = $rootScope.$new();
		scope.vm = {
			toggleCurrentSidenav: function() {}
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

	function FakePersonSelection(){
		var personIds = [];
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
		this.fakeSetCheckedPersonIds = function(data){
			personIds = data;
		};
		this.getCheckedPersonIds = function (){
			return personIds;
		};
	}

	function FakeToggles() {
		var _toggles = {};
		this.set = function(toggles) {
			_toggles = toggles;
		}
		this.all = function() {
			return _toggles;
		}
	}

	function FakePermissions() {
		var _permissions ={};
		this.set = function (permissions) {
			_permissions = permissions;
		}
		this.all = function () {
			return _permissions;
		}
	}
	
	function FakeValidateRulesService(){
		var warningDict = {
			"12345" : {
				"isLoaded": true,
				"warnings" : [
				{
					"RuleType": "NotOverwriteLayerRuleName",
					"Content": "OverwriteLayerWarnings"
				}]
			},			
			"67890": {
				"isLoaded": true,
				"warnings" : [
				{
					
				}]
			},
		};

		this.checkValidationForPerson = function(personId, filteredRuleType){
			 if (!warningDict[personId]) return [];
			 var result = warningDict[personId].warnings.filter(function(w) {
			 	if(filteredRuleType)
			 		return filteredRuleType == w.RuleType;
			 	return currentEnabledTypes[w.RuleType];
			 }).map(function(w) {
			 	return w.Content;
			 });

			 return result;
		};
	}
});