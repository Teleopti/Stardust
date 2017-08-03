describe('<group-page-picker>', function () {
	var attachedElements = [];
	var $componentController, $q, $compile, $rootScope, $document, $mdPanel;

	beforeEach(function () {
		module('wfm.templates', 'wfm.groupPage', 'ngMaterial', 'ngMaterial-mock');

		inject(function ($injector) {
			$componentController = $injector.get('$componentController');
			$q = $injector.get('$q');
			$compile = $injector.get('$compile');
			$rootScope = $injector.get('$rootScope');
			$document = $injector.get('$document');
			$material = $injector.get('$material');
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
			var scope = element.scope();

			scope && scope.$destroy();
			element.remove();
		});
		attachedElements = [];

		$document.find('md-select-menu').remove();
		$document.find('md-backdrop').remove();
	});

	it('should display panel when clicking on groupPage input', function () {
		var picker = setupComponent('group-pages="groupPages"', testScope());
		openPanel(picker);
		expectPanelOpen();
	});

	it('should display two tabs and business hierarchy should be selected by default.', function () {
		var picker = setupComponent('group-pages="groupPages"', testScope());

		var tabs = picker.find('md-tab-item');
		var icon = tabs[0].querySelector('i');

		expect(tabs.length).toEqual(2);
		expect(icon.title).toEqual('BusinessHierarchy');
		expect(angular.element(tabs[0]).hasClass('md-active')).toEqual(true);
	});

	it('should switch views when tabs are selected', function () {
		var picker = setupComponent('group-pages="groupPages"', testScope());
		openPanel(picker);
		expectPanelOpen();

		var tabs = $document[0].querySelectorAll('group-pages-panel md-tab-item');
		var tab = tabs[1];
		angular.element(tab).triggerHandler('click');

		var icon = tab.querySelector('i');
		expect(icon.title).toEqual('GroupPages');
		expect(angular.element(tab).hasClass('md-active')).toEqual(true);
		var group = $document[0].querySelector('.group label[for="group-groupPage1"]');
		expect(group.innerText.trim()).toEqual('groupPage1');

		var groups = $document[0].querySelectorAll('.group');
		expect(groups.length).toEqual(4);

	});

	it('should render business hierarchy group list',function() {
		var picker = setupComponent('group-pages="groupPages"', testScope());
		openPanel(picker);
		expectPanelOpen();
		var tabContents = $document.find('group-pages-panel').find('md-tab-content');
		var groups = tabContents[0].querySelectorAll('.group');
		expect(groups.length).toEqual(2);
	});

	it('should render child group after expand the group', function () {
		var picker = setupComponent('group-pages="groupPages"', testScope());
		openPanel(picker);
		expectPanelOpen();
		var tabContents = $document.find('group-pages-panel').find('md-tab-content');
		var groups = tabContents[0].querySelectorAll('.group');
		expect(groups.length).toEqual(2);
		angular.element(groups[0].querySelector('.group-toggle')).triggerHandler('click');
		expect(tabContents[0].querySelectorAll('.child').length).toEqual(2);

	});

	it('should collapse all groups by default',function() {
		var picker = setupComponent('group-pages="groupPages"', testScope());
		openPanel(picker);
		expectPanelOpen();

		var groups = $document[0].querySelectorAll('.group .mdi-chevron-up');
		expect(groups.length).toEqual(0);

	});

	it('can toggle group',function() {
		var picker = setupComponent('group-pages="groupPages"', testScope());
		openPanel(picker);
		expectPanelOpen();

		var toggle = angular.element($document[0].querySelector('.group .group-toggle'));
		toggle.triggerHandler('click');
		toggle = angular.element($document[0].querySelector('.group .group-toggle'));
		expect(toggle.find('i').hasClass('mdi-chevron-up')).toEqual(true);

		var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups.length).toEqual(2);

		toggle = angular.element($document[0].querySelector('.group .group-toggle'));
		toggle.triggerHandler('click');
		toggle = angular.element($document[0].querySelector('.group .group-toggle'));
		expect(toggle.find('i').hasClass('mdi-chevron-down')).toEqual(true);

		childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups.length).toEqual(0);
	});

	describe('sync selection both on UI and bound data', function () {
		it('should check parent group when all child groups are checked', function () {
			var scope = testScope();
			var picker = setupComponent('group-pages="groupPages"', scope);
			openPanel(picker);
			expectPanelOpen();

			var parentGroup = $document[0].querySelector('.group'); 
			var toggle = angular.element($document[0].querySelector('.group .group-toggle'));
			toggle.triggerHandler('click');

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			childGroups[0].querySelector('.wfm-checkbox-toggle').click();
			childGroups[1].querySelector('.wfm-checkbox-toggle').click();

			expect(parentGroup.querySelector('.wfm-checkbox input').checked).toEqual(true);
			expect(childGroups[0].querySelector('input').checked).toEqual(true);
			expect(childGroups[1].querySelector('input').checked).toEqual(true);

			var ctrl = picker.isolateScope().$ctrl;
			expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
			expect(ctrl.selectedGroups.groupIds.length).toEqual(2);


			childGroups[0].querySelector('.wfm-checkbox-toggle').click();
			childGroups[1].querySelector('.wfm-checkbox-toggle').click();
			expect(childGroups[0].querySelector('input').checked).toEqual(false);
			expect(childGroups[1].querySelector('input').checked).toEqual(false);

			expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
			expect(ctrl.selectedGroups.groupIds.length).toEqual(0);

		});

		it('should display  indeterminate status for parent group when some of child groups are checked', function () {
			var scope = testScope();
			var picker = setupComponent('group-pages="groupPages"', scope);
			openPanel(picker);
			expectPanelOpen();

			var parentGroup = $document[0].querySelector('.group'); 
			var toggle = angular.element($document[0].querySelector('.group .group-toggle'));
			toggle.triggerHandler('click');

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			childGroups[0].querySelector('.wfm-checkbox-toggle').click();

			var ctrl = picker.isolateScope().$ctrl;
			expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
			expect(ctrl.selectedGroups.groupIds.length).toEqual(1);
			expect(angular.element(parentGroup.querySelector('.wfm-checkbox')).hasClass('indeterminate')).toEqual(true);

		});

		it('should check all child groups when an unchecked parent group is checked', function () {
			var scope = testScope();
			var picker = setupComponent('group-pages="groupPages"', scope);
			openPanel(picker);
			expectPanelOpen();

			var parentGroup = $document[0].querySelector('.group'); 
			var parentCheckGroup = parentGroup.querySelector('.wfm-checkbox input');
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');

			parentCheckGroup.click();

			var ctrl = picker.isolateScope().$ctrl;
			expect(parentCheckGroup.checked).toEqual(true);
			expect(ctrl.selectedGroups.groupIds.length).toEqual(2);
		});
	});


	function expectPanelOpen() {
		var panel = angular.element($document[0].querySelector('.md-panel-outer-wrapper'));

		if (!(panel.hasClass('md-panel-is-showing'))) {
			throw Error('Expected picker panel to be open');
		}
	}

	function openPanel(component) {
		component.find('md-select-value').triggerHandler('click');
		$material.flushInterimElement();
	}

	function clickTab(pickerPanel, tabIndex) {
		var tab = pickerPanel.find('md-tab-item')[tabIndex];
		if (!tab) throw Error('Could not find tab');
		angular.element(tab).triggerHandler('click');
	}

	function setupComponent(attrs, scope) {
		var el;
		var template = '' +
			'<group-page-picker ' + (attrs || '') + '>' +
			'</group-page-picker>';

		el = $compile(template)(scope || $rootScope);
		$rootScope.$digest();
		attachedElements.push(el);
		return el;
	}

	function testScope() {
		var scope = $rootScope.$new();
		scope.groupPages = {
			'BusinessHierarchy': [
				{
					Id: 'site1',
					Name: 'site1',
					Children: [
						{
							Id: 'team1',
							Name: 'site1 team1'
						},
						{
							Id: 'team2',
							Name: 'site1 team2'
						}
					]
				},
				{
					Id: 'site2',
					Name: 'site2',
					Children: [
						{
							Id: 'team1',
							Name: 'site2 team1'
						},
						{
							Id: 'team2',
							Name: 'site2 team2'
						}
					]
				}
			],
			'GroupPages': [
				{
				Id: 'groupPage1',
				Name: 'groupPage1',
				Children: [
					{
						Id: 'childGroup1_1',
						Name: 'childGroup1_1'
					},
					{
						Id: 'childGroup1_2',
						Name: 'childGroup1_2'
					}
				]
				},
				{
					Id: 'groupPage2',
					Name: 'groupPage2',
					Children: [
						{
							Id: 'groupPage2_1',
							Name: 'groupPage2_1'
						},
						{
							Id: 'groupPage2_2',
							Name: 'groupPage2_2'
						}
					]
				}

			]
		};
		return scope;
	}

	function FakeGroupPages() {

	}

});