describe("team schedule command container test", function() {

	var $compile,
		$rootScope;

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


	it('container should render correctly', function() {

		var html = '<teamschedule-command-container date="curDate"></teamschedule-command-container>';
		var scope = $rootScope.$new();

		scope.curDate = new Date();

		fakeActivityService.setAvailableActivities(getAvailableActivities());


		var target = $compile(html)(scope);

		scope.$apply();
		var result = target[0].querySelector('.teamschedule-command-container');
		expect(result).not.toBeNull();
	});


	function getAvailableActivities() {
		return [
				{
					"Id": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6",
					"Name": "E-mail"
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