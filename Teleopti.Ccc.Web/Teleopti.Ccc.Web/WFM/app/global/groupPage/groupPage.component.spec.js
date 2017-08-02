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

	xit('should sync selection of multiple child groups in one parent group', function () {
		var picker = setupComponent('group-pages="groupPages"', testScope());
		openPanel(picker);
		expectPanelOpen();
		var tabContents = $document.find('group-pages-panel').find('md-tab-content');
		var children = tabContents[0].querySelectorAll('.child');
		var groups = tabContents[0].querySelectorAll('.group');
		expect(groups.length).toEqual(2);
		angular.element(groups[0].querySelector('.group-toggle')).triggerHandler('click');
		angular.element(children[0].querySelector('checkbox')).triggerHandler('click');
		

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
							Name: 'team1'
						},
						{
							Id: 'team2',
							Name: 'team2'
						}
					]
				},
				{
					Id: 'site2',
					Name: 'site2',
					Children: [
						{
							Id: 'team1',
							Name: 'team1'
						},
						{
							Id: 'team2',
							Name: 'team2'
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