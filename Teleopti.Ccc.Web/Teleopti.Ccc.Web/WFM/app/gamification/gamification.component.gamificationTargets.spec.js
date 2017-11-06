describe('gamification, ', function () {
	var $compile, $rootScope;

	beforeEach(function () {
		module('wfm.templates', 'externalModules', 'wfm.gamification', 'ngMaterial', 'ngMaterial-mock');

		inject(function ($injector) {
			$compile = $injector.get('$compile');
			$rootScope = $injector.get('$rootScope');
		});
	});

	describe('in the "set gamification targets" view, ', function () {
		it('should render', function () {
			var target = setupComponent('');
			expect(target.find('gamification-targets-table').length).toBe(1);
		});

		function setupComponent(attrs, scope) {
			var el;

			var template = '<gamification-targets ' + (attrs || '') + '>' + '</gamification-targets>';

			el = $compile(template)(scope || $rootScope);

			if (scope) {
				scope.$apply();
			} else {
				$rootScope.$digest();
			}

			// attachedElements.push(el);
			return el;
		}
	});
});