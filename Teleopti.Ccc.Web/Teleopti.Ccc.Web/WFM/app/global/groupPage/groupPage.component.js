(function () {
	'use strict';
	angular.module('wfm.groupPage')
		.component('groupPagePicker',
		{
			controller: GroupPagePickerController,
			templateUrl: 'app/global/groupPage/groupPagePicker.tpl.html',
			bindings: {
				onOpen: '&?',
				onClose: '&?',
				groupPages: '<',
				selectedGroups: '<',
				required: '<'
			}
		});

	GroupPagePickerController.$inject = ['$mdPanel', '$element', '$scope', '$translate'];

	function GroupPagePickerController($mdPanel, $element, $scope, $translate) {
		var ctrl = this;
		ctrl.isDataAvailable = false;
		ctrl.groupsInView = {
			BusinessHierarchy: [],
			GroupPages: []
		};
		ctrl.tabs = [
			{
				title: 'BusinessHierarchy',
				icon: 'file-tree'
			},
			{
				title: 'GroupPages',
				icon: 'folder-account'
			}
		];
		var tabIndexMap = { 'BusinessHierarchy': 0, 'GroupPages': 1 };

		function initSelectedGroups() {
			if (!ctrl.selectedGroups) {
				ctrl.selectedGroups = {};
			}
			ctrl.selectedGroups.mode = ctrl.selectedGroups.mode || 'BusinessHierarchy';
			ctrl.selectedGroups.groupIds = ctrl.selectedGroups.groupIds || [];
			ctrl.selectedGroups.groupPageId = ctrl.selectedGroups.groupPageId || '';
		}
		initSelectedGroups();

		var selectedIndex = !!ctrl.selectedGroups ? tabIndexMap[ctrl.selectedGroups.mode] : tabIndexMap[BusinessHierarchy];

		Object.defineProperty(ctrl, 'selectedIndex',
			{
				get: function () {
					return selectedIndex;
				},
				set: function (value) {
					selectedIndex = value;
					ctrl.changeTab(ctrl.tabs[selectedIndex]);
				}
			});

		ctrl.longestName = '';
		ctrl.searchText = '';

		ctrl.clearAll = function () {
			resetSelectedGroups();
		};

		ctrl.openMenu = function (event) {
			ctrl.menuRef.open();
		};

		ctrl.$onInit = function () {
			ctrl.menuRef = createPanel();
		}
		var rawData = {};

		ctrl.$onChanges = function (changesObj) {
			if (!!changesObj.groupPages && !!changesObj.groupPages.currentValue && changesObj.groupPages.currentValue !== changesObj.groupPages.previousValue) {
				rawData = changesObj.groupPages.currentValue;
				ctrl.setPickerData();
				removeNotExistTeamsFromSelection();
				updateSelectedGroupsInView();
				ctrl.isDataAvailable = true;
			}
			if (!!changesObj.selectedGroups && !!changesObj.selectedGroups.currentValue && !!changesObj.selectedGroups.previousValue && changesObj.selectedGroups.currentValue !== changesObj.selectedGroups.previousValue) {
				var currentValue = changesObj.selectedGroups.currentValue;
				var previousValue = changesObj.selectedGroups.previousValue;
				if (!!currentValue.mode && !!previousValue.mode && currentValue.mode != previousValue.mode) {
					selectedIndex = tabIndexMap[currentValue.mode];
					ctrl.setPickerData();
					resetSelectionInGroupsInView(previousValue.mode);
				}
				removeNotExistTeamsFromSelection();
				updateSelectedGroupsInView();
			}
		};
		ctrl.groupPagesSelectedText = function () {
			var text = '';
			switch (ctrl.selectedGroups.groupIds.length) {
				case 0:
					text = $translate.instant('SelectOrganization');
					break;
				default:
					var resource = "";
					switch (ctrl.selectedGroups.mode) {
						case "GroupPages":
							resource = "SeveralGroupsSelected";
							break;
						case "BusinessHierarchy":
							resource = "SeveralTeamsSelected";
							break;
					};
					text = $translate.instant(resource).replace('{0}', ctrl.selectedGroups.groupIds.length);
					break;
			}
			return text;
		};

		function updateSelectedGroupsInView() {
			var groupsInView = ctrl.groupsInView[ctrl.tabs[selectedIndex].title];
			if (!angular.isArray(ctrl.selectedGroups.groupIds) || !angular.isArray(groupsInView))
				return;
			for (var i = 0; i < groupsInView.length; i++) {
				var groupCopy = groupsInView[i];

				if (!groupCopy.origin.isParent) {
					continue;
				}

				groupCopy.selectedChildGroupIds.splice(0);

				for (var j = 0; j < groupCopy.children.length; j++) {
					var childGroup = groupCopy.children[j];
					if (isChildGroupSelected(childGroup)) {
						groupCopy.selectedChildGroupIds.push(childGroup.origin.id);
					}
				}
			}
		};
		function isChildGroupSelected(childGroupCopy) {
			var currentTab = ctrl.tabs[selectedIndex];
			if (currentTab.title === 'BusinessHierarchy' && ctrl.selectedGroups.groupIds.indexOf(childGroupCopy.origin.id) > -1)
				return true;
			if (currentTab.title === 'GroupPages' &&
				ctrl.selectedGroups.groupIds.indexOf(childGroupCopy.origin.id) > -1 &&
				ctrl.selectedGroups.groupPageId === childGroupCopy.origin.parent.id)
				return true;
			return false;
		}
		function isSelectionInCurrentTab() {
			var currentTab = ctrl.tabs[selectedIndex];
			return currentTab.title === ctrl.selectedGroups.mode;
		}

		ctrl.toggleGroup = function (parentGroupCopy) {
			if (!isSelectionInCurrentTab()) {
				resetSelectedGroups();
				ctrl.selectedGroups.mode = ctrl.tabs[selectedIndex].title;
			}
			parentGroupCopy.toggleAll();
			if (ctrl.selectedGroups.mode === "GroupPages"
				&& ctrl.selectedGroups.groupPageId !== parentGroupCopy.id) {
				if (!!ctrl.selectedGroups.groupPageId) {
					resetSelectedGroups();
				}

				ctrl.selectedGroups.groupPageId = parentGroupCopy.id;
			}
			if (parentGroupCopy.isChecked()) {

				parentGroupCopy.selectedChildGroupIds.forEach(function (id) {
					if (ctrl.selectedGroups.groupIds.indexOf(id) === -1) {
						ctrl.selectedGroups.groupIds.push(id);
					}
				});

			} else {
				parentGroupCopy.children.forEach(function (childGroupCopy) {
					var index = ctrl.selectedGroups.groupIds.indexOf(childGroupCopy.origin.id);
					if (index > -1) {
						ctrl.selectedGroups.groupIds.splice(index, 1);
					}
				});
			}
		};

		ctrl.toggleSubGroup = function (childGroupCopy) {
			if (!isSelectionInCurrentTab()) {
				resetSelectedGroups();
				ctrl.selectedGroups.mode = ctrl.tabs[selectedIndex].title;
			}

			if (ctrl.selectedGroups.mode === 'GroupPages'
				&& childGroupCopy.parent.id !== ctrl.selectedGroups.groupPageId) {
				resetSelectedGroups();
				ctrl.selectedGroups.groupPageId = childGroupCopy.parent.id;
			}
			childGroupCopy.parent.toggle(childGroupCopy.id);
			childGroupCopy.origin.parent.toggle(childGroupCopy.id);

			var index = ctrl.selectedGroups.groupIds.indexOf(childGroupCopy.origin.id);
			if (index > -1) {
				ctrl.selectedGroups.groupIds.splice(index, 1);
			} else {
				ctrl.selectedGroups.groupIds.push(childGroupCopy.origin.id);
			}
		};

		ctrl.collapseGroup = function (groupCopy) {
			var groupsInView = ctrl.groupsInView[ctrl.tabs[selectedIndex].title];
			var index = groupsInView.indexOf(groupCopy);
			if (groupCopy.collapsed) {
				var args = [index + 1, 0].concat(groupCopy.children);
				groupsInView.splice.apply(groupsInView, args);
				updateSelectedGroupsInView();
			} else {
				groupsInView.splice(index + 1, groupCopy.children.length)
			}
			groupCopy.collapsed = !groupCopy.collapsed;
		}

		ctrl.setPickerData = function () {
			populateGroupListAndNamemapAndFindLongestName(ctrl.groupPages[ctrl.tabs[selectedIndex].title]);
			ctrl.groupsInView[ctrl.tabs[selectedIndex].title] = ctrl.filterGroups('');
		}

		ctrl.changeTab = function (tab) {
			ctrl.setPickerData();
			ctrl.searchText = '';
			updateSelectedGroupsInView();
		};

		ctrl.onSearchTextChanged = function () {
			ctrl.groupsInView[ctrl.tabs[selectedIndex].title] = ctrl.filterGroups(ctrl.searchText);
			updateSelectedGroupsInView();
		};

		ctrl.filterGroups = function (searchText) {
			var textIsEmpty = (searchText === '');
			var results = [];
			var query = new RegExp(searchText, 'i');

			ctrl.groupList.forEach(function (group) {
				var parentGroupCopy = new ParentGroupCopy(group);
				parentGroupCopy.collapsed = textIsEmpty;
				var searchResultsInParentGroup = [parentGroupCopy];
				var parentGroupNameMatched = group.name.search(query) != -1
				group.children.forEach(function (child) {
					if (parentGroupNameMatched || textIsEmpty || child.name.search(query) != -1) {
						var childGroupCopy = new ChildGroupCopy(child, parentGroupCopy);
						parentGroupCopy.addChild(childGroupCopy);
						if (!parentGroupCopy.collapsed) {
							searchResultsInParentGroup.push(childGroupCopy);
						}
					}
				});
				if (parentGroupNameMatched || textIsEmpty || searchResultsInParentGroup.length > 1) {
					results = results.concat(searchResultsInParentGroup);
				}
			});

			return results;
		};

		ctrl.groupFocused = function (groupCopy) {
			groupCopy.isFocused = true;
		};

		ctrl.groupBlurred = function (groupCopy) {
			groupCopy.isFocused = false;
		};

		ctrl.$postLink = function () {
			setDefaultFocus();
		}
		$scope.$on("resetFocus", function () {
			setDefaultFocus();
		});

		function resetSelectionInGroupsInView(mode) {
			var parentGroupCopies = ctrl.groupsInView[mode].filter(function (g) {
				if (!g.origin.isParent)
					return false;
				if (ctrl.selectedGroups.mode === 'BusinessHierarchy')
					return g.isChecked() || g.isIndeterminate();
				return g.id === ctrl.selectedGroups.groupPageId;
			});

			parentGroupCopies.forEach(function (g) {
				g.clearAll();
			});
		}

		function resetSelectedGroups() {
			resetSelectionInGroupsInView(ctrl.selectedGroups.mode);

			ctrl.selectedGroups.groupIds.length = 0;
			ctrl.selectedGroups.groupPageId = '';
		}

		function removeNotExistTeamsFromSelection() {
			if (!ctrl.selectedGroups.groupIds || !ctrl.selectedGroups.groupIds.length) {
				return;
			}
			var ids = angular.copy(ctrl.selectedGroups.groupIds);
			ids.forEach(function (id) {
				if (!ctrl.nameMap[id]) {
					ctrl.selectedGroups.groupIds.splice(ctrl.selectedGroups.groupIds.indexOf(id), 1);
				}
			});
		}

		function ParentGroupCopy(originalParentGroup) {
			this.selectedChildGroupIds = [];
			this.collapsed = false;
			this.id = originalParentGroup.id;
			this.name = originalParentGroup.name;
			this.origin = originalParentGroup;
			this.children = [];
		}


		ParentGroupCopy.prototype.toggle = function (id) {
			var index = this.selectedChildGroupIds.indexOf(id);
			if (index > -1) {
				this.selectedChildGroupIds.splice(index, 1);
			} else {
				this.selectedChildGroupIds.push(id);
			}
		};
		ParentGroupCopy.prototype.toggleAll = function () {
			var isSelectedAll = this.isChecked();
			this.selectedChildGroupIds = [];
			if (isSelectedAll) {
				return;
			}
			this.children.forEach(function (childGroup) {
				this.selectedChildGroupIds.push(childGroup.id);
			}, this);
		};
		ParentGroupCopy.prototype.clearAll = function () {
			this.selectedChildGroupIds = [];

		};
		ParentGroupCopy.prototype.isIndeterminate = function () {
			return this.selectedChildGroupIds.length !== 0 && this.selectedChildGroupIds.length !== this.children.length;
		}

		ParentGroupCopy.prototype.isChecked = function () {
			return this.selectedChildGroupIds.length === this.children.length;
		};

		ParentGroupCopy.prototype.addChild = function (childCopy) {
			this.children.push(childCopy);
		};

		function ChildGroupCopy(originalChildGroup, parentGroupCopy) {
			this.id = originalChildGroup.id;
			this.name = originalChildGroup.name;
			this.parent = parentGroupCopy;
			this.origin = originalChildGroup;
		}

		ChildGroupCopy.prototype.isChecked = function () {
			return this.parent.selectedChildGroupIds.indexOf(this.id) > -1;
		};

		function ParentGroup(id, name) {
			this.selectedChildGroupIds = [];
			this.id = id;
			this.name = name;
			this.children = [];
			this.isParent = true;
			this.toggle = function (id) {
				var index = this.selectedChildGroupIds.indexOf(id)
				if (index > -1) {
					this.selectedChildGroupIds.splice(index, 1)
				} else {
					this.selectedChildGroupIds.push(id)
				}
			};
		}

		function ChildGroup(id, name, parentGroup) {
			this.id = id;
			this.name = name;
			this.parent = parentGroup;
			this.isParent = false;
		}

		function populateGroupListAndNamemapAndFindLongestName(groupPages) {
			ctrl.nameMap = {};
			ctrl.groupList = groupPages.map(function (rawGroup) {
				var parentGroup = new ParentGroup(rawGroup.Id, rawGroup.Name);

				ctrl.nameMap[parentGroup.id] = parentGroup.name;

				if (parentGroup.name.length > ctrl.longestName.length) {
					ctrl.longestName = parentGroup.name;
				}

				rawGroup.Children.forEach(function (child) {
					var childGroup = new ChildGroup(child.Id, child.Name, parentGroup);
					parentGroup.children.push(childGroup);

					ctrl.nameMap[childGroup.id] = childGroup.name;

					if (childGroup.name.length > ctrl.longestName.length) {
						ctrl.longestName = childGroup.name;
					}
				});

				return parentGroup;
			});
		}

		function setDefaultFocus() {
			var element = $element.find("md-select-value");
			element.focus();
		}

		function repaintPanel() {
			$scope.$broadcast('$md-resize');
			updateSelectedGroupsInView();
		}

		function createPanel() {
			var menuPosition = $mdPanel.newPanelPosition().relativeTo($element)
				.addPanelPosition($mdPanel.xPosition.ALIGN_START, $mdPanel.yPosition.BELOW);

			return $mdPanel.create({
				contentElement: $element.find('group-pages-panel'),
				clickOutsideToClose: true,
				escapeToClose: true,
				zIndex: 40,
				trapFocus: true,
				attachTo: angular.element(document.body), // must-have for text inputs on ie11
				position: menuPosition,
				onOpenComplete: function () {
					repaintPanel();

					if (ctrl.onOpen)
						ctrl.onOpen();
					document.querySelector(".group-page-picker-menu md-tab-item.md-active").focus();
				},
				onRemoving: (ctrl.onClose ? function () { ctrl.onClose() } : angular.noop),
				onCloseSuccess: function () {
					setDefaultFocus();
				},
			});
		}
	}
})();