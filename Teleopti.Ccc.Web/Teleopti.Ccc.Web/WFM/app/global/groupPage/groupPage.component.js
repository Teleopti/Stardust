(function() {
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
				icon:'file-tree'
			},
			{
				title: 'GroupPages',
				icon:'folder-account'
			}
		];

		ctrl.selectedIndex = 0;

		ctrl.longestName = '';
		ctrl.selectedGroups = {
			mode: ctrl.mode,
			selectedGroupsIds: []
		};

		ctrl.openMenu = function (event) {
			ctrl.menuRef.open();
		};

		ctrl.$onInit = function() {
			ctrl.menuRef = createPanel();
		}

		ctrl.toggleGroup = function () {

		};

		ctrl.toggleSubGroup = function (childGroupCopy) {
			childGroupCopy.parent.toggle(childGroupCopy.id);
			childGroupCopy.origin.parent.toggle(childGroupCopy.id);

			var index = this.selectedGroups.selectedGroupsIds.indexOf(childGroupCopy.origin.id);
			this.selectedGroups.selectedGroupsIds.push(childGroupCopy.origin.id);
		};

		ctrl.collapseGroup = function (groupCopy) {
			var index = this.groupsInView.indexOf(groupCopy)
			if (groupCopy.collapsed) {
				var args = [index + 1, 0].concat(groupCopy.children)
				this.groupsInView.splice.apply(this.groupsInView, args)
			} else {
				this.orgsInView.splice(index + 1, groupCopy.children.length)
			}
			groupCopy.collapsed = !groupCopy.collapsed;
		}

		ctrl.$onChanges = function (changesObj) {
			if (!!changesObj.groupPages && !!changesObj.groupPages.currentValue && changesObj.groupPages.currentValue !== changesObj.groupPages.previousValue) {
				ctrl.setPickerData();
			}
		};

		ctrl.setPickerData = function() {
			populateGroupListAndNamemapAndFindLongestName(ctrl.groupPages[ctrl.mode]);
			ctrl.groupsInView = ctrl.searchForOrgsByName('');
		}

		ctrl.changeTab = function (tab) {
			ctrl.mode = tab.title;
			ctrl.setPickerData();
		}

		ctrl.searchForOrgsByName = function(searchText) {
			var textIsEmpty = (searchText === '');
			var ret = [];
			ctrl.groupList.forEach(function(group) {
				var slaveGroup = new ParentGroupCopy(group);
				slaveGroup.collapsed = textIsEmpty;
				var r = [slaveGroup];
				group.children.forEach(function(child) {
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
			this.collapsed = false;
			this.id = originalParentGroup.id;
			this.name = originalParentGroup.name;
			this.origin = originalParentGroup;
			this.children = [];
		}

		ParentGroupCopy.prototype.addChild = function (childCopy) {
			this.children.push(childCopy);
		};

		function ChildGroupCopy(originalChildGroup, parentGroupCopy) {
			this.id = originalChildGroup.id;
			this.name = originalChildGroup.name;
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