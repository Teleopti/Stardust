describe("team schedule command menu directive test", function() {
	var $compile,
		$rootScope;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	var fakeShortCuts = {
		registerKeySequence: function() {}
	};

	beforeEach(function () {
		module(function ($provide) {
			$provide.value('$mdSidenav', function () { });
			$provide.value('$mdComponentRegistry', function () { });
			$provide.value('ShortCuts', function() {
				return fakeShortCuts;
			}());
			$provide.value('keyCodes', function () { });

		});

	});

	beforeEach(inject(function (_$rootScope_, _$compile_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;

	}));


	it('should view menu when adding activity is permitted', function () {

		var html = '<teamschedule-command configurations="getConfigurations()"></teamschedule-command>';
		var scope = $rootScope.$new();
		scope.vm = { toggleCurrentSidenav: function () { } };
		var config = {
			toggles: {AddActivityEnabled: true},
			permissions: { HasAddingActivityPermission: true, HasModifyAssignmentPermission:true}
		};
		scope.getConfigurations = function() {
			return config;
		}
		var element = $compile(html)(scope);

		scope.$apply();

		var menu = angular.element(element[0].querySelector('#scheduleContextMenuButton'));
		var menuList = angular.element(element[0].querySelector('.wfm-list'));

		expect(menu.length).toBe(1);
		expect(menuList.length).toBe(1);
	});
})