
define([
	'knockout'
], function (
	ko
    ) {

	return function (cascadeSelections) {
		//allow binding access to indeterminate property of checkbox.
		ko.bindingHandlers['prop'] = {
			update: function (element, valueAccessor, allBindingsAccessor) {
				var value = ko.utils.unwrapObservable(valueAccessor()) || {};
				for (var propName in value) {
					if (typeof propName == "string") {
						var propValue = ko.utils.unwrapObservable(value[propName]);

						element[propName] = propValue;
					}
				}
			}
		};

		function TreeNode(values) {
			var self = this;
			if (values !== undefined) {
				ko.mapping.fromJS(values, {
					children: { create: createNode }, parent: self
				}, self);
			}
			this.expanded = ko.observable(false);
			this.cascadeSelections = true;
			this.isInIndeterminateState = ko.observable(false);
			this.selected = ko.observable(false);
			this.collapsed = ko.computed(function () {
				return !self.expanded();
			});
			this.isLeaf = ko.computed(function () {
				if (self.children !== undefined) {
					return self.children().length === 0;
				}
				return true;
			});

			this.ChildStates = {
				UNSET: 0,
				ALLSELECTED: 1,
				SOMESELECTED: 2,
				NONESELECTED: 3
			};

			this.calculateNewStatusFromNode = function(existingState, nodeState) {
				
				if (existingState === self.ChildStates.NONESELECTED) {
					if (nodeState != self.ChildStates.NONESELECTED) {
						return self.ChildStates.SOMESELECTED;

					}
				} else {
					if (existingState == self.ChildStates.ALLSELECTED) {
						if (nodeState != self.ChildStates.ALLSELECTED) {
							return self.ChildStates.SOMESELECTED;
						}
					}
				}
				return nodeState;

			};

			this.getChildStates = function () {
				if (self.isLeaf()) {
					if (self.selected()) {
						return self.ChildStates.ALLSELECTED;
					} else {
						return self.ChildStates.NONESELECTED;
					}
				}
				var childNodes = self.children();
				var currentChildStatus = self.ChildStates.UNSET;
				for (var i = 0, len = childNodes.length; i < len; i++) {
					var childNodeState = childNodes[i].getChildStates();
					currentChildStatus = this.calculateNewStatusFromNode(currentChildStatus, childNodeState);

					if (currentChildStatus === self.ChildStates.SOMESELECTED) {
						return self.ChildStates.SOMESELECTED;
					}
				}
				return currentChildStatus;
			};


			this.calculateStateFromChildren = function () {
				var childStates = self.getChildStates();
				self.selected(childStates === this.ChildStates.ALLSELECTED);
				self.isInIndeterminateState(childStates === this.ChildStates.SOMESELECTED);


				if (self.parent !== undefined) {
					// RobTodo: this could be speed up by excluding self from the parents child search.
					self.parent.calculateStateFromChildren();
				}
			};

		
			this.cascadeSelectionState = function (selectedValue) {

				var childNodes = self.children();
				for (var i = 0, len = childNodes.length; i < len; i++) {

					childNodes[i].selected(selectedValue);
					childNodes[i].cascadeSelectionState(selectedValue);
				}

			};

			this.select = function () {
				if (self.cascadeSelections) {
					self.cascadeSelectionState(self.selected());

					if (self.parent !== undefined) {
						self.parent.calculateStateFromChildren();
					}
				}
				//allow default check action to proceed 
				return true;
			};
			this.toggle = function () {
				self.expanded(!self.expanded());
			};
		};

		var self = this;

		function createNode(options) {
			var treeNode = new TreeNode(options.data);
			treeNode.cascadeSelections = cascadeSelections;
			self.treeNodes.push(treeNode);
			treeNode.parent = options.parent; // set parent on tree node.
			return treeNode;
		};

		this.treeNodes = [];

		this.createNodeFromJson = function (values) {
			var treeNode = new TreeNode(values);
			treeNode.cascadeSelections = cascadeSelections;
			self.treeNodes.push(treeNode);
			return treeNode;
		};

		this.getSelectedLeafNodes = function() {
			var selectedLeafNodes = [];
			for (var i = 0, len = self.treeNodes.length; i < len; i++) {
				var node = self.treeNodes[i];
				if (cascadeSelections) {
					if (node.isLeaf() && node.selected()) {
						selectedLeafNodes.push(node);
					}
				}
				else if (node.selected())
				{
					selectedLeafNodes.push(node); 
				}
			}
			return selectedLeafNodes;
		};

		

	};
});