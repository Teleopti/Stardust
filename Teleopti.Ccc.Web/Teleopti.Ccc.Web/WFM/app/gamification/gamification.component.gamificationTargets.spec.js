describe('gamification, ', function () {
	var attachedElements = [];
	var $compile,
	    $rootScope,
	    $timeout,
	    $q,
	    $material,
	    $document;

	var times4FetchSettingList = 0;

	var mySites = [
		{ position: 0, id: '1', name: 'Washington DC' },
		{ position: 1, id: '2', name: 'Toronto' },
		{ position: 2, id: '3', name: 'London' },
		{ position: 3, id: '4', name: 'Paris' },
		{ position: 4, id: '5', name: 'Berlin' },
		{ position: 5, id: '6', name: 'Madrid' },
		{ position: 6, id: '7', name: 'Amsterdam' },
		{ position: 7, id: '8', name: 'Stockholm' },
		{ position: 8, id: '9', name: 'Geneva' },
		{ position: 9, id: '10', name: 'Rome' }
	];

	var dataService = new FakeGDataSvc();

	var n, ids, settingId;
	var selectedRows;

	beforeEach(function () {
		module('wfm.templates', 'externalModules', 'wfm.gamification', 'ngMaterial', 'ngMaterial-mock');

		module(function ($provide) {
			$provide.service('GamificationDataService', function () { return dataService; });
		});

		inject(function ($injector) {
			$compile = $injector.get('$compile');
			$rootScope = $injector.get('$rootScope');
			$q = $injector.get('$q');
			$material = $injector.get('$material');
			$document = $injector.get('$document');
			$timeout = $injector.get('$timeout');
		});

		times4FetchSettingList = 0;
	});

	function setupSelectedList() {
		var cmp = setupComponent();
		var scope = cmp.isolateScope();
		var ctrl = scope.$ctrl;

		ctrl.onAppliedSettingChange = function (teamIds, newValue) {
			ids = teamIds;
			settingId = newValue;
		}

		openSelectFor(cmp);
		selectOption(0);
		selectOption(1);
		selectOption(2);
		closeSelect();

		var numRows = cmp.find('gamification-target-row').length;
		var expected = 6;
		expect(numRows).toBe(expected);

		n = 3;
		for (var i = 0; i < n; i++) selectRow(cmp, i);

		// Select a row, and then un-select it.
		selectRow(cmp, n + 1);
		selectRow(cmp, n + 1);

		selectedRows = cmp[0].querySelectorAll('gamification-target-row[is-selected="true"]');
		expect(selectedRows.length).toBe(n);

		var row = angular.element(selectedRows[0]);
		removeAllSelectMenusInDom();
		openSelectFor(row);

		// Note that the first option is 'None'.
		selectOption(3);

		expectSelectClosed();
		return scope;
	};

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

		openSelectFor(cmp);
		selectOption(0);
		selectOption(1);
		closeSelect();

		var numRows = cmp.find('gamification-target-row').length;
		expect(numRows).toBe(3);
	});

	it('should check the main checkbox when all the table rows are selected', function () {
		var cmp = setupComponent();

		openSelectFor(cmp);
		selectOption(0);
		selectOption(1);
		closeSelect();

		var numRows = cmp.find('gamification-target-row').length;
		var expected = 3;
		expect(numRows).toBe(expected);

		for (var i = 0; i < expected; i++) {
			selectRow(cmp, i);
		}

		var mainCheckbox = cmp.find('header').find('md-checkbox');
		var criterion1 = mainCheckbox.hasClass('md-checked');
		var criterion2 = mainCheckbox.attr('aria-checked') === 'true';
		expect(criterion1).toBe(true);
		expect(criterion2).toBe(true);
	});

	it('should filter teams by name', function () {
		var cmp = setupComponent();

		openSelectFor(cmp);
		selectOption(0);
		selectOption(1);
		selectOption(2);
		selectOption(3);
		selectOption(4);
		closeSelect();

		var numRows = cmp.find('gamification-target-row').length;
		expect(numRows).toBe(11);

		var textInput = angular.element(cmp[0].querySelector('.filter > div > input'));

		// Case 1
		textInput.val('Berlin');
		textInput.triggerHandler('input');

		// so as to bypass the delay in ng-model-options
		$timeout.flush();

		numRows = cmp.find('gamification-target-row').length;
		expect(numRows).toBe(5);

		// Case 2
		textInput.val('Berlin/Team 5');
		textInput.triggerHandler('input');
		$timeout.flush();

		numRows = cmp.find('gamification-target-row').length;
		expect(numRows).toBe(1);
	});

	it('should be called with the changed data', function () {
		setupSelectedList();
		var expected = 'setting2';
		expect(ids.length).toBe(n);
		expect(ids[0]).toBe('site1team1');
		expect(ids[1]).toBe('site2team1');
		expect(ids[2]).toBe('site2team2');
		expect(settingId).toBe(expected);
	});

	it('should update the applied settings in the table', function () {
		setupSelectedList();
		var expected = 'Setting 2';
		selectedRows.forEach(function (row) {
			var selectedText = row.querySelector('md-select-value span div').innerText;
			expect(selectedText).toBe(expected);
		});
	});

	it('should update applied setting when active Targets tab', function () {
		var scope = setupSelectedList();
		expect(scope.$ctrl.teamsResult.current[0].appliedSettingId).toBe('setting2');

		var list = [
			{ id: 'default', name: 'Default' },
			{ id: 'setting1', name: 'Setting 1' },
			{ id: 'setting3', name: 'Setting 3' }
		];
		dataService.setSettingList(list);
		scope.$apply();

		scope.$broadcast('gamification.selectTargetsTab');
		scope.$apply();

		expect(scope.$ctrl.teamsResult.current[0].appliedSettingId).toBe('00000000-0000-0000-0000-000000000000');
	});

	function insertStyle(parentNode) {
		var node = $document[0].createElement('style');
		node.type = 'text/css';
		var css = 'gamification-target-row { display: block; min-height: 50px; }';
		node.appendChild($document[0].createTextNode(css));
		parentNode.appendChild(node);
	}

	function removeAllSelectMenusInDom() {
		var children = $document[0].querySelectorAll('body > .md-select-menu-container');
		for (var i = 0; i < children.length; i++) {
			angular.element(children[i]).remove();
		}
	}

	function setupComponent(attrs) {
		var scope = $rootScope.$new();
		var template = '<gamification-targets ' + (attrs || '') + '>' + '</gamification-targets>';
		var el = $compile(template)(scope);
		scope.$apply();

		attachedElements.push(el);
		$document[0].body.appendChild(el[0]);
		insertStyle(el[0]);
		initialse(el, scope);
		return el;
	}

	function openSelectFor(el) {
		el = el.find('md-select');
		try {
			el.triggerHandler('click');
			$material.flushInterimElement();
			el.triggerHandler('blur');
		} catch (e) { }
	}

	function closeSelect() {
		var backdrop = $document.find('md-backdrop');
		if (!backdrop.length) throw Error('Attempted to close select with no backdrop present');
		$document.find('md-backdrop').triggerHandler('click');
		$material.flushInterimElement();
		expectSelectClosed();
	}

	function expectSelectOpen() {
		var menu = angular.element($document[0].querySelector('body > .md-select-menu-container'));

		if (!(menu.hasClass('md-active') && menu.attr('aria-hidden') == 'false')) {
			throw Error('Expected select to be open');
		}
	}

	function expectSelectClosed() {
		var menu = angular.element($document[0].querySelector('.md-select-menu-container'));

		if (menu.length) {
			if (menu.hasClass('md-active') || menu.attr('aria-hidden') == 'false') {
				throw Error('Expected site picker to be closed');
			}
		}
	}

	function selectOption(index) {
		expectSelectOpen();
		var openMenu = $document.find('md-select-menu');
		var opt = openMenu.find('md-option')[index].querySelector('div');

		if (!opt) throw Error('Could not find option at index: ' + index);

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

	function FakeGDataSvc() {
		var settingList = [
			{ id: 'default', name: 'Default' },
			{ id: 'setting1', name: 'Setting 1' },
			{ id: 'setting2', name: 'Setting 2' },
			{ id: 'setting3', name: 'Setting 3' }
		];
		this.fetchSites = function () { return $q(function (resolve, reject) { resolve(mySites); }); };

		this.fetchTeams = function (siteIds) {
			return $q(function (resolve, reject) {
				var teams = [];
				siteIds.forEach(function (siteId) {
					var n = parseInt(siteId);
					for (var i = 0; i < n; i++) {
						teams.push({
							id: 'site' + siteId + 'team' + (i + 1),
							name: mySites[n - 1].name + '/Team ' + (i + 1)
						});
					}
				});
				resolve(teams);
			});
		};

		this.setSettingList = function (input) {
			settingList = input;
		}

		this.fetchSettingList = function () {
			times4FetchSettingList++;
			return $q(function (resolve, reject) {
				resolve(settingList);
			});
		};
	}
});