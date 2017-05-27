describe('<organization-picker>', function() {
	var attachedElements = [];
	var $componentController, $q, $compile, $rootScope, $document, $material;

	beforeEach(function () {
		module('wfm.templates', 'wfm.organizationPicker', 'ngMaterial', 'ngMaterial-mock');

		inject(function($injector) {
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

	function setupPicker(attrs, scope, optCompileOpts) {
		var el;
		var template = '' +
			'<organization-picker ' + (attrs || '') + '>' +
			'</organization-picker>';

		el = $compile(template)(scope || $rootScope);
		$rootScope.$digest();
		attachedElements.push(el);

		return el;
	}

	function openPicker(picker) {
		picker.find('md-select-value').triggerHandler('click');
		$material.flushInterimElement();
	}

	function closePicker(picker) {
		try {
			picker.find('div').scope().$ctrl.menuRef.close();
		} catch (e) {
			throw e;
		}
		$material.flushInterimElement();
	}

	function clickSite(index) {
		expectPickerOpen();

		var openPanel = $document.find('orgpicker-menu');
		var site = openPanel[0].querySelectorAll('.site')[index];
		if (!site) throw Error('Could not find site');

		var siteCheckbox = site.querySelector('.wfm-checkbox input[type=checkbox]');
		siteCheckbox.click();
	}



	function expectPickerOpen() {
		var panel = angular.element($document[0].querySelector('.md-panel-outer-wrapper'));

		if (!(panel.hasClass('md-panel-is-showing'))) {
			throw Error('Expected picker panel to be open');
		}
	}

	it('should populate hierachy list correctly', function () {
		$rootScope.datasource = fakeDatasource;
		var picker = setupPicker('datasource="datasource()"');
		var target;

		try {
			target = picker.find('div').scope().$ctrl.groupList;
		} catch (e) {
			throw e;
		}

		expect(target.length).toEqual(2);
		expect(target[0].id).toEqual('site1');
		expect(target[1].id).toEqual('site2');
		expect(target[0].teams.length).toEqual(1);
		expect(target[1].teams.length).toEqual(2);
	});

	it('should have a default function for generating the selected text', function () {
		var target;
		$rootScope.datasource = fakeDatasource;
		var picker = setupPicker('datasource="datasource()"');

		target = picker[0].querySelector('.selected-text');
		expect(target.innerHTML).toBe('SelectOrganization');

		openPicker(picker);
		clickSite(0);
		expect(target.innerHTML).toBe('team1');
		clickSite(1);
		expect(target.innerHTML).toBe('SeveralTeamsSelected');
	});

	it('should trigger on-close when the picker is closed', function() {
		var changed = false;
		$rootScope.onClose = function() { changed = true; };
		$rootScope.datasource = fakeDatasource;
		$rootScope.selectedTeamIds = [];

		var picker = setupPicker('datasource="datasource()" on-close="onClose()" selected-team-ids="selectedTeamIds"');
		expect(changed).toBe(false);

		openPicker(picker);
		clickSite(1);
		closePicker(picker);
		expect($rootScope.selectedTeamIds.length).toBe(2);
		expect(changed).toBe(true);
	});

	it('should synchronise selected team IDs', function() {
		$rootScope.teamIds = [];
		$rootScope.fakeDatasource = fakeDatasource;
		var picker = setupPicker('selected-team-ids="teamIds" datasource="fakeDatasource()"');

		picker.find('md-select-value').triggerHandler('click');
		$material.flushInterimElement();
		clickSite(0);

		expect($rootScope.teamIds.length).toEqual(1);
		expect($rootScope.teamIds[0]).toEqual('team1');
	});

	it('should support the single mode', function() {
		$rootScope.teamIds = [];
		$rootScope.datasource = fakeDatasource;
		var p = setupPicker('single selected-team-ids="teamIds" datasource="datasource()"');

		openPicker(p);
		toggleSite(0);
		clickTeam(0);
		expect($rootScope.teamIds.length).toBe(1);
		expect($rootScope.teamIds[0]).toBe('team1');

		openPicker(p);
		toggleSite(0);
		toggleSite(1);
		clickTeam(0);
		expect($rootScope.teamIds.length).toBe(1);
		expect($rootScope.teamIds[0]).toBe('team2');
	});

	function toggleSite(index) {
		expectPickerOpen();

		var openPanel = $document.find('orgpicker-menu');
		var site = openPanel[0].querySelectorAll('.site')[index];

		if (!site) throw Error('Could not find site');
		site.click();
	}

	function clickTeam(index) {
		expectPickerOpen();

		var openPanel = $document.find('orgpicker-menu');
		var team = openPanel[0].querySelectorAll('.team')[index];

		if (!team) throw Error('Could not find team ' + index);

		team.click();
	}

	var data = {
		Children: [
			{
				Id: 'site1',
				Name: 'site1',
				Children: [
					{ Id: 'team1', Name: 'team1' }
				]
			},
			{
				Id: 'site2',
				Name: 'site2',
				Children: [
					{ Id: 'team2', Name: 'team2' },
					{ Id: 'team3', Name: 'team3' }
				]
			}
		],
		logonUserTeamId: 'logonUserTeamId'
	};

	function fakeDatasource() {
		return data;
	}
});