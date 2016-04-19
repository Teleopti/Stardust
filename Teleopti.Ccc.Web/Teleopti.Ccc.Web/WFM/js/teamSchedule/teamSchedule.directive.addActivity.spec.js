describe('team schedule add activity tests', function () {

	var $compile,
		$rootScope,
		fakeActivityService;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));
	beforeEach(function() {
		fakeActivityService = new FakeActivityService();
		module(function($provide) {
			$provide.service('ActivityService', function() {
				return fakeActivityService;
			});
			$provide.service('guidgenerator', function() {
				return {
					newGuid: function() {
						return "B4A88909-A1A0-4672-A7A3-14909B2C7673";
					}
				}
			});
			$provide.service('CommandCommon', function() {
				return new FakeCommandCommonService();
			})
		});
	});

	function FakeCommandCommonService() {		
		this.wrapPersonWriteProtectionCheck = function(skipDialogIfNormal, commandTitle, action) {
			return action;
		}
	}

	function FakeActivityService() {
		var availableActivities = [];
		var targetActivity = null;

		this.fetchAvailableActivities = function() {
			return {
				then: function(cb) {
					cb(availableActivities);
				}
			};
		};

		this.addActivity = function(input) {
			targetActivity = input;
			return {
				then: (function (cb) {
					cb(targetActivity);
				})
			};
		};

		this.getAddActivityCalledWith = function() {
			return targetActivity;
		};

		this.setAvailableActivities = function(activities) {
			availableActivities = activities;
		};
	}

	beforeEach(inject(function (_$rootScope_, _$compile_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;

	}));


	it('should render directive content correctly', function () {

		var html = '<add-activity-panel selected-agents="" selected-date=""></add-activity-panel>';
		var scope = $rootScope.$new();
		var element = $compile(html)(scope);

		scope.$apply();

		var form = element.find('form');
		var selectElements = element.find('select');
		var timeRangePicker = element.find('activity-time-range-picker');
		var applyButton;
		angular.forEach(element.find('button'), function (e) {
			var ae = angular.element(e);
			if (element.hasClass('form-submit')) applyButton = ae;
		});
	
		expect(form.length).toBe(1);
		expect(selectElements.length).toBe(1);
		expect(timeRangePicker.length).toBe(1);
		expect(applyButton).not.toBeNull();
	});

	it('should see available activities in select element', function () {
		var html = '<add-activity-panel selected-agents="" selected-date=""></add-activity-panel>';
		var scope = $rootScope.$new();
		var elements = $compile(html)(scope);
		var availableActivities = [
				{
					"Id": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6",
					"Name": "E-mail"
				},
				{
					"Id": "5c1409de-a0f1-4cd4-b383-9b5e015ab3c6",
					"Name": "Invoice"
				},
				{
					"Id": "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
					"Name": "Phone"
				},
				{
					"Id": "84db44f4-22a8-44c7-b376-a0a200da613e",
					"Name": "Sales"
				},
				{
					"Id": "35e33821-862f-461c-92db-9f0800a8d095",
					"Name": "Social Media"
				}
		];
		fakeActivityService.setAvailableActivities(availableActivities);
		scope.$apply();

		var selectOptions = elements.find('option');
		expect(selectOptions.length).toBe(6);
	});

	it('should see a disabled button when no activity selected', function() {
		var html = '<add-activity-panel selected-agents="" selected-date=""></add-activity-panel>';
		var scope = $rootScope.$new();
		var element = $compile(html)(scope);
		scope.$apply();

		var applyButton;
		angular.forEach(element.find('button'), function (e) {
			var element = angular.element(e);
			if (element.hasClass('form-submit')) applyButton = element;
		});

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	it('should see a disabled button when time range is invalid', function () {
		var html = '<add-activity-panel selected-agents="" selected-date="getSelectedDate()"></add-activity-panel>';
		var scope = $rootScope.$new();
		scope.getSelectedDate = function () {		
			return new Date('2016-01-01');
		};

		var element = $compile(html)(scope);
		scope.$apply();

		var innerScope = element.isolateScope().vm;

		innerScope.timeRange.startTime = new Date('2015-01-01 08:00:00');
		innerScope.timeRange.endTime = new Date('2015-01-01 02:00:00');
		scope.$apply();


		var applyButton;
		angular.forEach(element.find('button'), function (e) {
			var element = angular.element(e);
			if (element.hasClass('form-submit')) applyButton = element;
		});

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	it('should see a disabled button when anyone in selected is not allowed to add current activity', function () {
		var html = '<add-activity-panel selected-date="getSelectedDate()"></add-activity-panel>';
		var scope = $rootScope.$new();
		var selectedDate = new Date('2016-01-01');
		scope.getSelectedDate = function () {
			return selectedDate;
		};

		var element = $compile(html)(scope);
		scope.$apply();
		var innerScope = element.isolateScope().vm;
		innerScope.selectedAgents = [
			{
				personId: 'agent1',
				name: 'agent1',
				scheduleEndTime: moment('2016-01-01').endOf('day'),
			}, {
				personId: 'agent2',
				name: 'agent2',
				scheduleEndTime: '2016-01-02 05:00'
			}];
		innerScope.timeRange.startTime = new Date('2016-01-02 01:00');
		innerScope.timeRange.endTime = new Date('2016-01-02 02:00');
		innerScope.isNextDay = true;
		innerScope.selectedActivityId = "activity";


		var applyButton;
		angular.forEach(element.find('button'), function (e) {
			var element = angular.element(e);
			if (element.hasClass('form-submit')) applyButton = element;
		});

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
		expect(innerScope.isInputValid()).toBe(false);
		expect(innerScope.peopleNotAllowed()).toBe("agent1");

	});

	it('should call add activity when click apply with correct data', function () {
		var html = '<add-activity-panel selected-date="getSelectedDate()"></add-activity-panel>';
		
		var scope = $rootScope.$new();
		scope.getSelectedDate = function () {		
			return new Date('2016-01-01');
		};

		var availableActivities = [
				{
					"Id": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6",
					"Name": "E-mail"
				},
				{
					"Id": "5c1409de-a0f1-4cd4-b383-9b5e015ab3c6",
					"Name": "Invoice"
				},
				{
					"Id": "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
					"Name": "Phone"
				},
				{
					"Id": "84db44f4-22a8-44c7-b376-a0a200da613e",
					"Name": "Sales"
				},
				{
					"Id": "35e33821-862f-461c-92db-9f0800a8d095",
					"Name": "Social Media"
				}
		];
		fakeActivityService.setAvailableActivities(availableActivities);

		var element = $compile(html)(scope);
		scope.$apply();

		var innerScope = element.isolateScope().vm;
		innerScope.selectedAgents = [{personId: 'agent1' }, {personId: 'agent2' }];
		innerScope.timeRange.startTime = new Date('2015-01-01 02:00:00');
		innerScope.timeRange.endTime = new Date('2015-01-01 08:00:00');
		innerScope.selectedActivityId = '472e02c8-1a84-4064-9a3b-9b5e015ab3c6';
		scope.$apply();

		var applyButton;
		angular.forEach(element.find('button'), function (e) {
			var element = angular.element(e);
			if (element.hasClass('form-submit')) applyButton = element;
		});
		
		applyButton.triggerHandler('click');

		scope.$apply();

		var activityData = fakeActivityService.getAddActivityCalledWith();
	
		expect(activityData).not.toBeNull();
		expect(activityData.PersonIds.length).toEqual(2);
		expect(activityData.ActivityId).toEqual('472e02c8-1a84-4064-9a3b-9b5e015ab3c6');
		expect(activityData.StartTime).toEqual('2015-01-01 02:00');
		expect(activityData.EndTime).toEqual('2015-01-01 08:00');
		expect(activityData.BelongsToDate).toEqual(scope.getSelectedDate());
		expect(activityData.TrackedCommandInfo.TrackId).toEqual("B4A88909-A1A0-4672-A7A3-14909B2C7673");

	});

	it('should invoke afterAddActivity function after adding activity', function () {
		var html = '<add-activity-panel selected-agents="getSelectedAgents()" selected-date="getSelectedDate()" actions-after-activity-apply="callback()"></add-activity-panel>';

		var scope = $rootScope.$new();
		scope.getSelectedAgents = function () {
			return ['agent1', 'agent2'];
		};
		scope.getSelectedDate = function () {
			return new Date('2016-01-01');
		};

		var callbackCalled = false;

		scope.callback = function() {
			callbackCalled = true;
		}

		var availableActivities = [
				{
					"Id": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6",
					"Name": "E-mail"
				},
				{
					"Id": "5c1409de-a0f1-4cd4-b383-9b5e015ab3c6",
					"Name": "Invoice"
				},
				{
					"Id": "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
					"Name": "Phone"
				},
				{
					"Id": "84db44f4-22a8-44c7-b376-a0a200da613e",
					"Name": "Sales"
				},
				{
					"Id": "35e33821-862f-461c-92db-9f0800a8d095",
					"Name": "Social Media"
				}
		];
		fakeActivityService.setAvailableActivities(availableActivities);

		var element = $compile(html)(scope);
		scope.$apply();

		var innerScope = element.isolateScope().vm;

		innerScope.timeRange.startTime = new Date('2015-01-01 02:00:00');
		innerScope.timeRange.endTime = new Date('2015-01-01 08:00:00');
		innerScope.selectedActivityId = '472e02c8-1a84-4064-9a3b-9b5e015ab3c6';
		scope.$apply();

		var applyButton;
		angular.forEach(element.find('button'), function (e) {
			var ele = angular.element(e);
			if (ele.hasClass('form-submit')) applyButton = ele;
		});

		applyButton.triggerHandler('click');

		scope.$apply();

		expect(callbackCalled).toBeTruthy();

	});
});