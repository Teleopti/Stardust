describe('<gamification-import>', function () {
	var attachedElements = [];
	var $compile,
		$rootScope,
		$timeout,
		$q,
		$material,
		$document;

	beforeEach(function () {
		module('wfm.templates', 'externalModules', 'wfm.gamification', 'ngMaterial', 'ngMaterial-mock');

		module(function ($provide) {
			$provide.service('GamificationDataService', function () { return new DataService(); });
		});

		inject(function ($injector) {
			$compile = $injector.get('$compile');
			$rootScope = $injector.get('$rootScope');
			$q = $injector.get('$q');
			$material = $injector.get('$material');
			$document = $injector.get('$document');
			$timeout = $injector.get('$timeout');

		});
	});

	afterEach(function () {
		var body = $document[0].body;
		var children = body.querySelectorAll('.md-select-menu-container');
		for (var i = 0; i < children.length; i++) {
			angular.element(children[i]).remove();
		}
	});

	afterEach(function () {
		attachedElements.forEach(function (element) {
			var elementScope = element.scope();

			elementScope && elementScope.$destroy();
			element.remove();
		});
		attachedElements = [];

		$document.find('md-select-menu').remove();
		$document.find('md-backdrop').remove();
	});

	it('should render', function () {
		var cmp = setupComponent();
		var downloadBtn = cmp[0].querySelector('button#downloadTmpl');
		expect(downloadBtn).not.toBe(null);
	});

	// it('should download template when clicking Download button', function () { });

	it('should list all import jobs', function () {
		var cmp = setupComponent();
		var list = cmp.find('import-job');
		expect(list.length).toBe(3);
		expect(list[0].querySelector('.job').innerText).toBe('Job 0');
		expect(list[1].querySelector('.job').innerText).toBe('Job 1');
		expect(list[2].querySelector('.job').innerText).toBe('Job 2');
	});

	function setupComponent(attrs, scope) {
		var el;

		var template = '<gamification-import ' + (attrs || '') + '>' + '</gamification-import>';

		el = $compile(template)(scope || $rootScope);

		if (scope) {
			scope.$apply();
		} else {
			$rootScope.$digest();
		}

		attachedElements.push(el);
		$document[0].body.appendChild(el[0]);
		// insertStyle(el[0]);
		// initialse(el, scope || $rootScope);
		return el;
	}

	function DataService() {
		this.fetchJobs = function () {
			return $q(function (resolve, reject) {
				resolve([
					{ id: 0, name: 'Job 0' },
					{ id: 1, name: 'Job 1' },
					{ id: 2, name: 'Job 2' }
				]);
			});
		};
	}
});