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
		});
	});

	function FakeActivityService() {
		var availableActivities = [];
		this.fetchAvailableActivities = function () {
			
			return {
				then: function(cb) {
					cb(availableActivities);
				}
		};
		}

		this.setAvailableActivities = function(activities) {
			availableActivities = activities;
		}
	}

	beforeEach(inject(function (_$rootScope_, _$compile_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;

	}));



	it('should render directive content correctly', function () {

		var html = '<add-activity-panel selected-agents="" selected-date=""></add-activity-panel>';
		var scope = $rootScope.$new();
		var elements = $compile(html)(scope);

		scope.$apply();

		var selectElements = elements.find('select');

		var timeRangePicker = elements.find('time-range-picker');

		var applyButton = elements.find('button');

		expect(selectElements.length).toBe(1);
		expect(timeRangePicker.length).toBe(1);
		expect(applyButton.length).toBe(1);
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

		var applyButton = element.find('button');
		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	//it('should see a diabled button when time range is invalid', function() {});

});