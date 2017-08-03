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
				selection: '<'
			}
		});

	GroupPagePickerController.$inject = ['$mdPanel', '$element', '$scope'];

	function GroupPagePickerController($mdPanel, $element, $scope) {
		var ctrl = this;
		ctrl.mode = 'BusinessHierarchy';
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

		var selectedIndex = 0;

		Object.defineProperty(ctrl, 'selectedIndex',
			{
				get: function () { return selectedIndex; },
				set: function (value) {
					selectedIndex = value;
					ctrl.changeTab(ctrl.tabs[selectedIndex]);
				}
			});

		ctrl.longestName = '';
		ctrl.selectedGroups = {
			mode: ctrl.mode,
			groupIds: []
		};

		ctrl.openMenu = function (event) {
			ctrl.menuRef.open();
		};

		ctrl.$onInit = function () {
			ctrl.menuRef = createPanel();
		}

		ctrl.toggleGroup = function (parentGroupCopy) {
			parentGroupCopy.toggleAll();
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
			var index = ctrl.groupsInView.indexOf(groupCopy)
			if (groupCopy.collapsed) {
				var args = [index + 1, 0].concat(groupCopy.children)
				ctrl.groupsInView.splice.apply(ctrl.groupsInView, args)
			} else {
				ctrl.groupsInView.splice(index + 1, groupCopy.children.length)
			}
			groupCopy.collapsed = !groupCopy.collapsed;
		}

		ctrl.$onChanges = function (changesObj) {
			if (!!changesObj.groupPages && !!changesObj.groupPages.currentValue && changesObj.groupPages.currentValue !== changesObj.groupPages.previousValue) {
				ctrl.setPickerData();
			}
		};

		ctrl.setPickerData = function () {
			populateGroupListAndNamemapAndFindLongestName(ctrl.groupPages[ctrl.mode]);
			ctrl.groupsInView = ctrl.searchForOrgsByName('');

		}

		ctrl.changeTab = function (tab) {
			ctrl.mode = tab.title;
			ctrl.setPickerData();
		}

		ctrl.searchForOrgsByName = function (searchText) {
			var textIsEmpty = (searchText === '');
			var ret = [];
			ctrl.groupList.forEach(function (group) {
				var slaveGroup = new ParentGroupCopy(group);
				slaveGroup.collapsed = textIsEmpty;
				var r = [slaveGroup];
				group.children.forEach(function (child) {
					var sc = new ChildGroupCopy(child, slaveGroup);
					slaveGroup.addChild(sc);
					if (!slaveGroup.collapsed) {
						r.push(sc);
					}
				});
				ret = ret.concat(r);
			});
			return ret;
		};

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
			this.children.forEach(function(childGroup) {
				this.selectedChildGroupIds.push(childGroup.id);
			}, this);
		};
		ParentGroupCopy.prototype.isIndeterminate =  function () {
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

		function ParentGroup(id, name) {
			var selectedChildGroupIds = [];
			this.id = id;
			this.name = name;
			this.children = [];
			this.isParent = true;
			this.toggle = function (id) {
				var index = selectedChildGroupIds.indexOf(id)
				if (index > -1) {
					selectedChildGroupIds.splice(index, 1)
				} else {
					selectedChildGroupIds.push(id)
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
					$scope.$broadcast('$md-resize');

					if (ctrl.onOpen)
						ctrl.onOpen();
				},
				onRemoving: (ctrl.onClose ? function () { ctrl.onClose() } : angular.noop),
				onCloseSuccess: function () {
					setDefaultFocus();
				},
			});
		}
	}
})();