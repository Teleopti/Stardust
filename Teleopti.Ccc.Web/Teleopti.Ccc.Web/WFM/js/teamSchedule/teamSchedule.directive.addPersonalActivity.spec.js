describe("teamschedule add personal activity directive test", function () {

	var $compile,
		$rootScope,
		fakeActivityService;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		fakeActivityService = new FakeActivityService();
		module(function ($provide) {
			$provide.service('ActivityService', function () {
				return fakeActivityService;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
	}));

	it('add-personal-activity should render correctly', function () {

		var html = '<teamschedule-command-container><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();

		var target = $compile(html)(scope);

		scope.$apply();
		var result = target[0].querySelector('.add-personal-activity');
		expect(result).not.toBeNull();
	});

	it('add-personal-activity should get date from container', function () {

		var date = new Date('2016-06-15T00:00:00Z');
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;

		var target = $compile(html)(scope);

		scope.$apply();

		var result = angular.element(target[0].querySelector('.add-personal-activity'));

		expect(moment(result.scope().getDate()).format('YYYY-MM-DD')).toBe('2016-06-15');
	});

	it('should load activity list', function() {
		var date = new Date('2016-06-15T00:00:00Z');
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;

		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var target = $compile(html)(scope);
		scope.$apply();

		var result = target[0].querySelectorAll('.activity-selector .activity-option-item');

		expect(result.length).toBe(5);
	});

	function getAvailableActivities() {
		return [
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
	}

	function FakeActivityService() {
		var availableActivities = [];
		var targetActivity = null;
		var fakeResponse = { data: [] };

		this.fetchAvailableActivities = function () {
			return {
				then: function (cb) {
					cb(availableActivities);
				}
			};
		};

		this.addActivity = function (input) {
			targetActivity = input;
			return {
				then: (function (cb) {
					cb(fakeResponse);
				})
			};
		};

		this.getAddActivityCalledWith = function () {
			return targetActivity;
		};

		this.setAvailableActivities = function (activities) {
			availableActivities = activities;
		};
	}
});