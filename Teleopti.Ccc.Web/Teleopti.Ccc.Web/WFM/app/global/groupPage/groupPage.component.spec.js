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

		expect(tabs.length).toEqual(2);
		expect(tabs[0].innerText).toEqual('BusinessHierarchy');
		expect(angular.element(tabs[0]).hasClass('md-active')).toEqual(true);
	});

	it('should switch views when tabs are selected', function() {
		var picker = setupComponent('group-pages="groupPages"', testScope());
		clickTab(picker, 1);
		var tab = picker.find('md-tab-item')[1];
		expect(tab.innerText).toEqual('GroupPages');
		expect(angular.element(tab).hasClass('md-active')).toEqual(true);
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

	function clickParentGroup() {

	}

	function clickChildGroup() {

	}

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
			]
		};
		return scope;
	}

	function FakeGroupPages() {

	}

});