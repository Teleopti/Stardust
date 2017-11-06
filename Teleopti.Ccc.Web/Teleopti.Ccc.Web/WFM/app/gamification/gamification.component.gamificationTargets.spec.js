describe('gamification, ', function () {
	var $compile,
	    $rootScope,
	    $q;

	beforeEach(function () {
		module('wfm.templates', 'externalModules', 'wfm.gamification', 'ngMaterial', 'ngMaterial-mock');

		module(function ($provide) {
			$provide.service('GamificationDataService', function () { return new FakeGDataSvc(); });
		});

		inject(function ($injector) {
			$compile = $injector.get('$compile');
			$rootScope = $injector.get('$rootScope');
			$q = $injector.get('$q');
		});
	});

	describe('in the "set gamification targets" view, ', function () {
		it('should render', function () {
			var target = setupComponent('');
			expect(target.find('gamification-targets-table').length).toBe(1);
		});

		it('should fetch sites', function () {
			var target = setupComponent('');
			expect(target.find('md-option').length).toBe(10);
		});

		// it('should fetch teams on the selected sites', function () {});

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

	function FakeGDataSvc() {
		this.fetchSites = function () {
			return $q(function (resolve, reject) {
				var n = 10;
				var sites = [];
				for (var i = 0; i < n; i++) {
					sites.push({ position: i, id: 'site'+(i+1), name: 'Site '+(i+1) });
				}
				resolve(sites);
			});
		};
	}
});