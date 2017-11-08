describe('gamification, ', function () {
	var attachedElements = [];
	var $compile,
	    $rootScope,
	    $q,
	    $material,
	    $document;

	beforeEach(function () {
		module('wfm.templates', 'externalModules', 'wfm.gamification', 'ngMaterial', 'ngMaterial-mock');

		module(function ($provide) {
			$provide.service('GamificationDataService', function () { return new FakeGDataSvc(); });
		});

		inject(function ($injector) {
			$compile = $injector.get('$compile');
			$rootScope = $injector.get('$rootScope');
			$q = $injector.get('$q');
			$material = $injector.get('$material');
			$document = $injector.get('$document');
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

	describe('in the "set gamification targets" view, ', function () {
		it('should render', function () {
			var target = setupComponent();
			expect(target.find('gamification-targets-table').length).toBe(1);
		});

		it('should fetch sites', function () {
			var cmp = setupComponent();
			var numSites = cmp.find('md-option').length;
			expect(numSites).toBe(10);
		});

		it('should fetch teams on the selected sites', function () {
			var cmp = setupComponent();

			openSitePickerFor(cmp);
			selectSite(0);
			selectSite(1);
			closeSitePicker();

			var numRows = cmp.find('gamification-target-row').length;
			expect(numRows).toBe(3);
		});

		it('should check the main checkbox when all the table rows are selected', function () {
			var cmp = setupComponent();

			openSitePickerFor(cmp);
			selectSite(0);
			selectSite(1);
			closeSitePicker();

			var numRows = cmp.find('gamification-target-row').length;
			var expected = 3;
			expect(numRows).toBe(expected);

			for (var i = 0; i < expected; i++) {
				selectRow(cmp, i);
			}

			var header = cmp.find('header');
			var criterion1 = header.find('md-checkbox').hasClass('md-checked');
			var criterion2 = header.find('md-checkbox').attr('aria-checked') === 'true';
			expect(criterion1).toBe(true);
			expect(criterion2).toBe(true);
		});

		function insertStyle(parentNode) {
			var node = $document[0].createElement('style');
			node.type = 'text/css';
			var css = 'gamification-target-row { display: block; min-height: 50px; }';
			node.appendChild($document[0].createTextNode(css));
			parentNode.appendChild(node);
		}

		function setupComponent(attrs, scope) {
			var el;

			var template = '<gamification-targets ' + (attrs || '') + '>' + '</gamification-targets>';

			el = $compile(template)(scope || $rootScope);

			if (scope) {
				scope.$apply();
			} else {
				$rootScope.$digest();
			}


			attachedElements.push(el);
			$document[0].body.appendChild(el[0]);
			insertStyle(el[0]);
			initialse(el, scope || $rootScope);
			return el;
		}

		function openSitePickerFor(el) {
			el = el.find('md-select');
			try {
				el.triggerHandler('click');
				$material.flushInterimElement();
				el.triggerHandler('blur');
			} catch (e) { }
		}

		function closeSitePicker() {
			var backdrop = $document.find('md-backdrop');
			if (!backdrop.length) throw Error('Attempted to close site picker with no backdrop present');
			$document.find('md-backdrop').triggerHandler('click');
			$material.flushInterimElement();
			expectSitePickerClosed();
		}

		function expectSitePickerOpen() {
			var menu = angular.element($document[0].querySelector('.md-select-menu-container'));

			if (!(menu.hasClass('md-active') && menu.attr('aria-hidden') == 'false')) {
				throw Error('Expected site picker to be open');
			}
		}

		function expectSitePickerClosed() {
			var menu = angular.element($document[0].querySelector('.md-select-menu-container'));

			if (menu.length) {
				if (menu.hasClass('md-active') || menu.attr('aria-hidden') == 'false') {
					throw Error('Expected site picker to be closed');
				}
			}
		}

		function selectSite(index) {
			expectSitePickerOpen();
			var openMenu = $document.find('md-select-menu');
			var opt = openMenu.find('md-option')[index].querySelector('div');

			if (!opt) throw Error('Could not find site at index: ' + index);

			angular.element(openMenu).triggerHandler({
				type: 'click',
				target: angular.element(opt),
				currentTarget: openMenu[0]
			});
		}

		function selectRow(component, index) {
			var row = component.find('gamification-target-row')[index];
			if (!row) throw Error('Could not find row at index: ' + index);
			var div = row.querySelector('.team');
			if (!div) throw Error('Could not find element of `team` class');
			angular.element(div).triggerHandler('click');
		}

		function increaseRepeatContainerHeight(el) {
			var container = el[0].querySelector('md-virtual-repeat-container');
			container.style.height = '400px';
			container.style.display = 'block';
		}

		function initialse(el, scope) {
			increaseRepeatContainerHeight(el);
			scope.$broadcast('gamification.selectTargetsTab');
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

		this.fetchTeams = function (siteIds) {
			return $q(function (resolve, reject) {
				var teams = [];
				siteIds.forEach(function (siteId) {
					var n = parseInt(siteId[siteId.length - 1]);
					for (var i = 0; i < n; i++) {
						teams.push({
							teamId: siteId + 'team' + (i + 1),
							teamName: 'Site ' + n + '/Team ' + (i + 1)
						});
					}
				});
				resolve(teams);
			});
		};
	}
});