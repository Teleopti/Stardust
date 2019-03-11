describe('<group-page-picker>', function () {
	var attachedElements = [];
	var $componentController, $q, $compile, $rootScope, $document, scope;

	beforeEach(function () {
		module('wfm.templates', 'wfm.groupPage', 'ngMaterial', 'ngMaterial-mock', 'wfm.searchFilter');

		inject(function ($injector) {
			$componentController = $injector.get('$componentController');
			$q = $injector.get('$q');
			$compile = $injector.get('$compile');
			$rootScope = $injector.get('$rootScope');
			$document = $injector.get('$document');
			$material = $injector.get('$material');

			scope = testScope();
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

	it('should display panel when clicking on groupPage input', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', testScope());
		openPanel(picker);
		expectPanelOpen();
	});

	it('should display two tabs and business hierarchy should be selected by default.', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', testScope());

		var tabs = picker.find('md-tab-item');
		var icon = tabs[0].querySelector('i');

		expect(tabs.length).toEqual(2);
		expect(icon.title).toEqual('BusinessHierarchy');
		expect(angular.element(tabs[0]).hasClass('md-active')).toEqual(true);
	});

	it('should switch views when tabs are selected', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();

		clickTab(1);

		expectTabOpen(1, 'GroupPages');

		var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
		expect(parentGroups[0].innerText.trim()).toEqual('groupPage1');

		var groups = $document[0].querySelectorAll('.group');
		expect(groups.length).toEqual(4);
	});

	it('should clear filter text when tabs are selected', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();

		var ctrl = picker.isolateScope().$ctrl;
		ctrl.searchText = "site1";
		picker.isolateScope().$apply(ctrl.onSearchTextChanged);

		clickTab(1);
		expectTabOpen(1, 'GroupPages');

		var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
		expect(parentGroups[0].innerText.trim()).toEqual('groupPage1');

		var groups = $document[0].querySelectorAll('md-tab-content.md-active .group');
		expect(groups.length).toEqual(2);
		expect(ctrl.searchText).toEqual("");
	});

	it('should render business hierarchy group list', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', testScope());
		openPanel(picker);
		expectPanelOpen();
		var tabContents = $document.find('group-pages-panel').find('md-tab-content');
		var groups = tabContents[0].querySelectorAll('.group');
		expect(groups.length).toEqual(2);
	});

	it('should render child group after expand the group', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', testScope());
		openPanel(picker);
		expectPanelOpen();
		var tabContents = $document.find('group-pages-panel').find('md-tab-content');
		var groups = tabContents[0].querySelectorAll('.group');
		expect(groups.length).toEqual(2);
		toggleGroupPage(0);
		expect(tabContents[0].querySelectorAll('.child').length).toEqual(2);

	});

	it('should collapse all groups by default', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', testScope());
		openPanel(picker);
		expectPanelOpen();

		var groups = $document[0].querySelectorAll('.group .mdi-chevron-up');
		expect(groups.length).toEqual(0);

	});

	it('can toggle group', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', testScope());
		openPanel(picker);
		expectPanelOpen();

		toggleGroupPage(0);
		var toggle = angular.element($document[0].querySelector('.group .group-toggle'));
		expect(toggle.find('i').hasClass('mdi-chevron-up')).toEqual(true);

		var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups.length).toEqual(2);

		toggleGroupPage(0);
		toggle = angular.element($document[0].querySelector('.group .group-toggle'));
		expect(toggle.find('i').hasClass('mdi-chevron-down')).toEqual(true);

		childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups.length).toEqual(0);
	});

	it('should clear selected groups when switching to group pages tab', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();

		toggleGroupPage(0);
		checkGroupById('site1team1');
		checkGroupById('site1team2');
		var toggle = angular.element($document[0].querySelector('.group .group-toggle'));
		expect(toggle.find('i').hasClass('mdi-chevron-up')).toEqual(true);

		var ctrl = picker.isolateScope().$ctrl;
		expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
		expect(ctrl.selectedGroups.groupIds.length).toEqual(2);

		clickTab(1);
		toggleGroupPage(0);
		checkGroupById('childGroup1_1');

		expect(ctrl.selectedGroups.mode).toEqual('GroupPages');
		expect(ctrl.selectedGroups.groupIds.length).toEqual(1);
	});

	it('should keep the selected groups when switching tab but did not select any groups', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();

		toggleGroupPage(0);
		checkGroupById('site1team1');
		checkGroupById('site1team2');
		var toggle = angular.element($document[0].querySelector('.group .group-toggle'));
		expect(toggle.find('i').hasClass('mdi-chevron-up')).toEqual(true);

		var ctrl = picker.isolateScope().$ctrl;
		expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
		expect(ctrl.selectedGroups.groupIds.length).toEqual(2);

		clickTab(1);

		expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
		expect(ctrl.selectedGroups.groupIds.length).toEqual(2);
	});

	it('should close panel when I click the Close button', function () {

		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();
		var closeButton = $document[0].querySelector('.selection-done');
		closeButton.click();

		expectPanelClosed();
	});

	it('should display teams are selected', function () {

		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();

		checkGroupPage(0);
		checkGroupPage(1);

		var selectValue = picker.find("md-select-value")[0].querySelector('.selected-text');
		expect(selectValue.innerText.trim()).toEqual("SeveralTeamsSelected");
	});

	it('should display groups are selected', function () {

		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();
		clickTab(1);
		checkGroupPage(0);

		var selectValue = picker.find("md-select-value")[0].querySelector('.selected-text');
		expect(selectValue.innerText.trim()).toEqual("SeveralGroupsSelected");
	});

	it('should be able to select group by clicking on group name', function () {
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();
		clickTab(1);
		checkGroupPageByClickingName(0);

		var selectValue = picker.find("md-select-value")[0].querySelector('.selected-text');
		expect(selectValue.innerText.trim()).toEqual("SeveralGroupsSelected");
	});

	it('should preselect teams ', function () {

		scope.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: ['site1team1', 'site2team1']
		};
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();

		var ctrl = picker.isolateScope().$ctrl;

		var tabs = $document[0].querySelectorAll('md-tab-item');
		expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
		expect(angular.element(tabs[0]).hasClass('md-active')).toEqual(true);

		toggleGroupPage(0);
		var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
		expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(false);
		toggleGroupPage(0);

		toggleGroupPage(1);
		childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
	});

	it('should preselect group pages', function () {

		scope.selectedGroups = {
			mode: 'GroupPages',
			groupPageId: 'groupPage1',
			groupIds: ['childGroup1_1']
		};
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();

		var ctrl = picker.isolateScope().$ctrl;
		var tabs = $document[0].querySelectorAll('md-tab-item');
		expect(ctrl.selectedGroups.mode).toEqual('GroupPages');
		expect(angular.element(tabs[1]).hasClass('md-active')).toEqual(true);

		toggleGroupPage(0);
		var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
		expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(false);
		toggleGroupPage(0);
	});

	it('should change the selection status when given selected groups are changed outside of the component', function () {
		scope.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: ['site1team1']
		};
		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();

		var ctrl = picker.isolateScope().$ctrl;
		var tabs = $document[0].querySelectorAll('md-tab-item');
		expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
		expect(angular.element(tabs[0]).hasClass('md-active')).toEqual(true);

		toggleGroupPage(0);
		var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
		expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(false);
		toggleGroupPage(0);

		clickTab(1);
		checkGroupPage(0);

		expect(ctrl.selectedGroups.mode).toEqual('GroupPages');
		expect(angular.element(tabs[1]).hasClass('md-active')).toEqual(true);

		toggleGroupPage(0);
		childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
		expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(true);

		toggleGroupPage(0);
		$document[0].querySelector('.group-page-picker-menu .selection-done').click();

		scope.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: ['site2team1']
		};
		scope.$apply();

		openPanel(picker);
		expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
		expect(angular.element(tabs[0]).hasClass('md-active')).toEqual(true);

		toggleGroupPage(1);
		childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);

	});

	it('should clear all selection when click clear button on GroupPage tab', function () {

		var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
		openPanel(picker);
		expectPanelOpen();

		clickTab(1);
		scope.$broadcast('$md-resize');

		checkGroupPage(0);

		expect(scope.selectedGroups.mode).toEqual('GroupPages');
		expect(scope.selectedGroups.groupIds.length).toEqual(2);
		expect(scope.selectedGroups.groupPageId).toEqual('groupPage1');

		var clearButton = $document[0].querySelector('.selection-clear');
		clearButton.click();
		expect(scope.selectedGroups.groupIds.length).toEqual(0);
		expect(scope.selectedGroups.groupPageId).toEqual('');
	});

	describe('when I am on the Group Pages tab,', function () {
		var picker;

		beforeEach(function () {
			picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			increaseRepeatContainerHeight(picker);
			openPanel(picker);
			expectPanelOpen();
			clickTab(1);
		});

		it('should reset selected groups when switching to another group page and current group page is selected', function () {
			checkGroupPage(0);

			expect(scope.selectedGroups.mode).toEqual('GroupPages');
			expect(scope.selectedGroups.groupIds.length).toEqual(2);
			expect(scope.selectedGroups.groupPageId).toEqual('groupPage1');

			checkGroupPage(1);

			expect(scope.selectedGroups.mode).toEqual('GroupPages');
			expect(scope.selectedGroups.groupIds.length).toEqual(2);
			expect(scope.selectedGroups.groupPageId).toEqual('groupPage2');

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(false);
			expect(parentGroups[1].querySelector('md-checkbox').checked).toEqual(true);

			toggleGroupPage(0);
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(false);
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(false);
			toggleGroupPage(0);

			toggleGroupPage(1);
			childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);

		});

		it('should reset selected groups when switching to another group value and current group page is selected', function () {
			checkGroupPage(0);

			expect(scope.selectedGroups.mode).toEqual('GroupPages');
			expect(scope.selectedGroups.groupIds.length).toEqual(2);
			expect(scope.selectedGroups.groupPageId).toEqual('groupPage1');

			findAndCheckChildGroup(1, 0);

			expect(scope.selectedGroups.mode).toEqual('GroupPages');
			expect(scope.selectedGroups.groupIds.length).toEqual(1);
			expect(scope.selectedGroups.groupPageId).toEqual('groupPage2');
		});

		it('should reset selected groups when switching to another group page and current group page is partially selected', function () {
			findAndCheckChildGroup(0, 0);

			expect(scope.selectedGroups.mode).toEqual('GroupPages');
			expect(scope.selectedGroups.groupIds.length).toEqual(1);
			expect(scope.selectedGroups.groupPageId).toEqual('groupPage1');

			toggleGroupPage(0);
			checkGroupPage(1);

			expect(scope.selectedGroups.mode).toEqual('GroupPages');
			expect(scope.selectedGroups.groupIds.length).toEqual(2);
			expect(scope.selectedGroups.groupPageId).toEqual('groupPage2');

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(false);
			expect(parentGroups[1].querySelector('md-checkbox').checked).toEqual(true);
		});

		it('should reset selected groups when switching to another group value and current group page is partially selected', function () {
			findAndCheckChildGroup(0, 0);

			expect(scope.selectedGroups.mode).toEqual('GroupPages');
			expect(scope.selectedGroups.groupIds.length).toEqual(1);
			expect(scope.selectedGroups.groupPageId).toEqual('groupPage1');

			toggleGroupPage(0);
			findAndCheckChildGroup(1, 0);

			expect(scope.selectedGroups.mode).toEqual('GroupPages');
			expect(scope.selectedGroups.groupIds.length).toEqual(1);
			expect(scope.selectedGroups.groupPageId).toEqual('groupPage2');
		});

		it('should filter groups when child group names are matched', function () {
			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'childGroup1_1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(1);
			expect(parentGroups[0].innerText.trim()).toEqual('groupPage1');
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(1);
			expect(childGroups[0].innerText.trim()).toEqual('childGroup1_1');
		});

		it('should filter groups when parent groups are matched', function () {
			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'groupPage1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(1);
			expect(parentGroups[0].innerText.trim()).toEqual("groupPage1");

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(2);
		});

		it('should display selection status when filtering result contains selected groups only', function () {
			findAndCheckChildGroup(0, 1);

			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'childGroup1_2';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(1);
			expect(parentGroups[0].innerText.trim()).toEqual('groupPage1');
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(1);
			expect(childGroups[0].innerText.trim()).toEqual('childGroup1_2');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);

			expect(ctrl.selectedGroups.groupIds.length).toEqual(1);
		});

		it('should display selection status when filtering result contains selected groups', function () {
			findAndCheckChildGroup(0, 1);

			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'groupPage1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(1);
			expect(parentGroups[0].innerText.trim()).toEqual('groupPage1');
			expect(angular.element(parentGroups[0].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(2);
			expect(childGroups[0].innerText.trim()).toEqual('childGroup1_1');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(false);
			expect(childGroups[1].innerText.trim()).toEqual('childGroup1_2');
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(true);

		});

		it('should display selection status when the whole parent group is selected and filtering result contains selected groups', function () {
			checkGroupPage(0);

			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'groupPage1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(1);
			expect(parentGroups[0].innerText.trim()).toEqual('groupPage1');
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(2);
			expect(childGroups[0].innerText.trim()).toEqual('childGroup1_1');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
			expect(childGroups[1].innerText.trim()).toEqual('childGroup1_2');
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(true);
		});

		it('should preserve selection status when select filtered group', function () {
			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'groupPage1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			checkGroupById('childGroup1_1');
			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(angular.element(parentGroups[0].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);

			ctrl.searchText = '';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);
			expect(angular.element(parentGroups[0].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);

			toggleGroupPage(0);
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(2);
			expect(childGroups[0].innerText.trim()).toEqual('childGroup1_1');
			expect(childGroups[1].innerText.trim()).toEqual('childGroup1_2');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(false);
		});

		it('should preserve selection status when whole site is checked', function () {
			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'groupPage1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			checkGroupById('childGroup1_1');
			checkGroupById('childGroup1_2');

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);

			ctrl.searchText = '';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);

		});

		it('should the indeterminate status be switched to checked '
			+ 'when initail filtered site is partially checked '
			+ 'and then changing filter to contain 1 group only', function () {
				var ctrl = picker.isolateScope().$ctrl;
				ctrl.searchText = 'groupPage1';
				picker.isolateScope().$apply(ctrl.onSearchTextChanged);

				checkGroupById('childGroup1_1');
				var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
				expect(angular.element(parentGroups[0].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);

				var ctrl = picker.isolateScope().$ctrl;
				ctrl.searchText = 'childGroup1_1';
				picker.isolateScope().$apply(ctrl.onSearchTextChanged);
				expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);
			});
	});

	describe("when I am on the Business Hierarchy tab", function () {

		it('should clear all selection when click clear button ', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();
			clickTab(0);
			checkGroupPage(1);

			expect(scope.selectedGroups.mode).toEqual('BusinessHierarchy');
			expect(scope.selectedGroups.groupIds.length).toEqual(2);
			expect(scope.selectedGroups.groupPageId).toEqual('');

			var clearButton = $document[0].querySelector('.selection-clear');
			clearButton.click();
			expect(scope.selectedGroups.groupIds.length).toEqual(0);
			expect(scope.selectedGroups.groupPageId).toEqual('');

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(false);

			toggleGroupPage(1);
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(false);
		});

		it('can select multiple sites',
			function () {

				var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
				openPanel(picker);
				expectPanelOpen();

				clickTab(0);
				checkGroupPage(0);
				checkGroupPage(1);

				var parentGroups = $document[0].querySelectorAll('.group');
				expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);
				expect(parentGroups[1].querySelector('md-checkbox').checked).toEqual(true);

			});

		it('can select teams from multiple sites', function () {
			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();

			findAndCheckChildGroup(0, 0);

			toggleGroupPage(0);

			findAndCheckChildGroup(1, 0);

			var parentGroups = $document[0].querySelectorAll('.group');

			expect(angular.element(parentGroups[0].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);
			expect(angular.element(parentGroups[1].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);

			toggleGroupPage(1);
			toggleGroupPage(0);
			childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(false);
		});

		it('should filter teams when team names are matched', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			increaseRepeatContainerHeight(picker);
			openPanel(picker);
			expectPanelOpen();

			clickTab(0);
			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'team1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(2);
			expect(parentGroups[0].innerText.trim()).toEqual('site1');
			expect(parentGroups[1].innerText.trim()).toEqual('site2');
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups[0].innerText.trim()).toEqual('site1 team1');
			expect(childGroups[1].innerText.trim()).toEqual('site2 team1');
		});

		it('should filter sites when site names are matched', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			increaseRepeatContainerHeight(picker);
			openPanel(picker);
			expectPanelOpen();

			clickTab(0);
			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'site1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(1);
			expect(parentGroups[0].innerText.trim()).toEqual('site1');
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(2);
			expect(childGroups[0].innerText.trim()).toEqual('site1 team1');
			expect(childGroups[1].innerText.trim()).toEqual('site1 team2');
		});

		it('should display selection status when filtering result contains selected teams only', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			increaseRepeatContainerHeight(picker);
			openPanel(picker);
			expectPanelOpen();

			clickTab(0);
			findAndCheckChildGroup(0, 1);

			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'site1 team2';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(1);
			expect(parentGroups[0].innerText.trim()).toEqual('site1');
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(1);
			expect(childGroups[0].innerText.trim()).toEqual('site1 team2');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);

		});
		it('should display selection status when filtering result contains selected teams', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			increaseRepeatContainerHeight(picker);
			openPanel(picker);
			expectPanelOpen();

			clickTab(0);
			findAndCheckChildGroup(0, 1);

			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'site1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(1);
			expect(parentGroups[0].innerText.trim()).toEqual('site1');
			expect(angular.element(parentGroups[0].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(2);
			expect(childGroups[0].innerText.trim()).toEqual('site1 team1');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(false);
			expect(childGroups[1].innerText.trim()).toEqual('site1 team2');
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(true);

		});
		it('should display selection status when the whole site is selected and filtering result contains selected teams', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();

			clickTab(0);
			checkGroupPage(0);

			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'site1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups.length).toEqual(1);
			expect(parentGroups[0].innerText.trim()).toEqual('site1');
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(2);
			expect(childGroups[0].innerText.trim()).toEqual('site1 team1');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
			expect(childGroups[1].innerText.trim()).toEqual('site1 team2');
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(true);
		});

		it('should preserve selection status when select filtered team', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();

			clickTab(0);

			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'site1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			checkGroupById('site1team1');
			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(angular.element(parentGroups[0].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);

			ctrl.searchText = '';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);
			expect(angular.element(parentGroups[0].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);

			toggleGroupPage(0);
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			expect(childGroups.length).toEqual(2);
			expect(childGroups[0].innerText.trim()).toEqual('site1 team1');
			expect(childGroups[1].innerText.trim()).toEqual('site1 team2');
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(false);
		});

		it('should preserve selection status when whole site is checked', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();

			clickTab(0);

			var ctrl = picker.isolateScope().$ctrl;
			ctrl.searchText = 'site1';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);

			checkGroupById('site1team1');
			checkGroupById('site1team2');

			var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);

			ctrl.searchText = '';
			picker.isolateScope().$apply(ctrl.onSearchTextChanged);
			expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);

		});
		it('should the indeterminate status be switched to checked '
			+ 'when initail filtered site is partially checked '
			+ 'and then changing filter to contain 1 team only', function () {

				var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
				openPanel(picker);
				expectPanelOpen();

				clickTab(0);
				var ctrl = picker.isolateScope().$ctrl;
				ctrl.searchText = 'site1';
				picker.isolateScope().$apply(ctrl.onSearchTextChanged);

				checkGroupById('site1team1');
				var parentGroups = $document[0].querySelectorAll('md-tab-content.md-active .group');
				expect(angular.element(parentGroups[0].querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);

				var ctrl = picker.isolateScope().$ctrl;
				ctrl.searchText = 'site1 team1';
				picker.isolateScope().$apply(ctrl.onSearchTextChanged);
				expect(parentGroups[0].querySelector('md-checkbox').checked).toEqual(true);
			});

		it('should remove not exists teams from selection when some of selected group ids was changed and some ids are not exists in avaliable groups', function () {
			scope.selectedGroups = { groupIds: ['site1team1'] };
			setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			scope.selectedGroups = { groupIds: ['site1team1', 'site1team3'] };
			scope.$apply();
			expect(scope.selectedGroups.groupIds).toEqual(['site1team1']);
		});

	});

	describe('should sync data, and', function () {
		it('should check parent group when all child groups are checked', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();
			toggleGroupPage(0);

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			childGroups[0].querySelector('md-checkbox').click();
			childGroups[1].querySelector('md-checkbox').click();

			var parentGroup = $document[0].querySelector('.group');
			expect(parentGroup.querySelector('md-checkbox').checked).toEqual(true);
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(true);

			var ctrl = picker.isolateScope().$ctrl;
			expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
			expect(ctrl.selectedGroups.groupIds.length).toEqual(2);
		});

		it('should uncheck parent group when all child groups are unchecked', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();
			toggleGroupPage(0);

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			childGroups[0].querySelector('md-checkbox').click();
			childGroups[1].querySelector('md-checkbox').click();

			var parentGroup = $document[0].querySelector('.group');
			expect(parentGroup.querySelector('md-checkbox').checked).toEqual(true);
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(true);

			var ctrl = picker.isolateScope().$ctrl;
			expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
			expect(ctrl.selectedGroups.groupIds.length).toEqual(2);


			childGroups[0].querySelector('md-checkbox').click();
			childGroups[1].querySelector('md-checkbox').click();
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(false);
			expect(childGroups[1].querySelector('md-checkbox').checked).toEqual(false);

			expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
			expect(ctrl.selectedGroups.groupIds.length).toEqual(0);

		});

		it('should display indeterminate status for parent group when some of child groups are checked', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();
			toggleGroupPage(0);

			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			childGroups[0].querySelector('md-checkbox').click();

			var parentGroup = $document[0].querySelector('.group');
			var ctrl = picker.isolateScope().$ctrl;
			expect(ctrl.selectedGroups.mode).toEqual('BusinessHierarchy');
			expect(ctrl.selectedGroups.groupIds.length).toEqual(1);
			expect(angular.element(parentGroup.querySelector('md-checkbox')).hasClass('indeterminate')).toEqual(true);

		});

		it('should check all child groups when an unchecked parent group is checked', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();

			var parentGroup = $document[0].querySelector('.group');
			var parentCheckGroup = parentGroup.querySelector('md-checkbox');

			parentCheckGroup.click();

			var ctrl = picker.isolateScope().$ctrl;
			expect(parentCheckGroup.checked).toEqual(true);
			expect(ctrl.selectedGroups.groupIds.length).toEqual(2);
		});

		it('should check all child groups when checking partially selected parent group', function () {
			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();

			clickTab(1);
			findAndCheckChildGroup(0, 1);
			var parentGroup = $document[0].querySelector('md-tab-content.md-active .group');
			var parentCheckGroup = parentGroup.querySelector('md-checkbox');
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			parentCheckGroup.click();
			var ctrl = picker.isolateScope().$ctrl;
			expect(parentCheckGroup.checked).toEqual(true);
			expect(ctrl.selectedGroups.groupIds.length).toEqual(2);
			expect(childGroups[0].querySelector('md-checkbox').checked).toEqual(true);
		});

		it('should check all when group page  is partially selected', function () {

			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();

			toggleGroupPage(0);
			var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
			childGroups[0].querySelector('md-checkbox').click();
			expect(scope.selectedGroups.groupIds.length).toEqual(1);
			checkGroupPage(0);
			expect(scope.selectedGroups.groupIds.length).toEqual(2);

		});

		it('should remove the not exists item from the selection when the group picker data reloads on same tab', function () {
			var picker = setupComponent('group-pages="groupPages" selected-groups="selectedGroups"', scope);
			openPanel(picker);
			expectPanelOpen();

			checkGroupPage(0);
			expect(scope.selectedGroups.groupIds.length).toEqual(2);

			scope.groupPages = {
				'BusinessHierarchy': [
					{
						Id: 'site1',
						Name: 'site1',
						Children: [
							{
								Id: 'site1team1',
								Name: 'site1 team1'
							}
						]
					}
				]
			};
			scope.$apply();
			expect(scope.selectedGroups.groupIds.length).toEqual(1);
		});


	});

	function increaseRepeatContainerHeight(picker) {
		var container = picker[0].querySelector('group-pages-panel md-virtual-repeat-container');
		container.style.height = '400px';
		container.style.display = 'block';
	}

	function expectTabOpen(index, title) {
		var tab = $document[0].querySelectorAll('group-pages-panel md-tab-item')[index];
		var icon = tab.querySelector('i');
		expect(icon.title).toEqual(title);
		expect(angular.element(tab).hasClass('md-active')).toEqual(true);
	}

	function expectPanelClosed() {
		var panel = $document[0].querySelector('.md-panel-outer-wrapper');
		expect(panel).toBe(null);
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

	function checkGroupById(id) {
		var selector = 'md-tab-content.md-active #group-' + id;
		var groups = $document[0].querySelectorAll(selector);
		expect(groups.length).toBe(1);
		groups[0].click();
	}

	function checkGroupPage(index) {
		expectPanelOpen();
		var groupPageCheckboxes = $document[0].querySelectorAll('md-tab-content.md-active .group md-checkbox');
		groupPageCheckboxes[index].click();
	}

	function checkGroupPageByClickingName(index) {
		expectPanelOpen();
		$document[0].querySelectorAll('md-tab-content.md-active .group span.md-checkbox-label')[index].click();
	}

	function findAndCheckChildGroup(groupPageIndex, childIndex) {
		toggleGroupPage(groupPageIndex);
		var childGroups = $document[0].querySelectorAll('md-tab-content.md-active .child');
		childGroups[childIndex].querySelector('md-checkbox').click();

		return childGroups;
	}

	function toggleGroupPage(pageIndex) {
		var toggles = angular.element($document[0].querySelectorAll('md-tab-content.md-active .group .group-toggle'));
		toggles[pageIndex].click();
	}

	function clickTab(tabIndex) {
		var tabs = $document[0].querySelectorAll('group-pages-panel md-tab-item');
		var tab = tabs[tabIndex];
		tab.click();
		scope.$broadcast('$md-resize');
	}

	function setupComponent(attrs, scope) {
		var el;
		var template = '' +
			'<group-page-picker ' + (attrs || '') + '>' +
			'</group-page-picker>';

		el = $compile(template)(scope || $rootScope);
		if (scope) {
			scope.$apply();
		} else {
			$rootScope.$digest();
		}

		attachedElements.push(el);
		return el;
	}

	function testScope() {
		var scope = $rootScope.$new();
		scope.selectedGroups = {};
		scope.groupPages = {
			'BusinessHierarchy': [
				{
					Id: 'site1',
					Name: 'site1',
					Children: [
						{
							Id: 'site1team1',
							Name: 'site1 team1'
						},
						{
							Id: 'site1team2',
							Name: 'site1 team2'
						}
					]
				},
				{
					Id: 'site2',
					Name: 'site2',
					Children: [
						{
							Id: 'site2team1',
							Name: 'site2 team1'
						},
						{
							Id: 'site2team2',
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
							Id: 'childGroup2_1',
							Name: 'childGroup2_1'
						},
						{
							Id: 'childGroup2_2',
							Name: 'childGroup2_2'
						}
					]
				}

			]
		};
		return scope;
	}
});