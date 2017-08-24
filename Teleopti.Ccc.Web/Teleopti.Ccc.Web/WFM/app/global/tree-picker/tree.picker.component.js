(function () {
  angular
  .module('wfm.treePicker', [])
  .component('treePicker', {
    templateUrl: 'app/global/tree-picker/tree-picker.html',
    controller: TreePickerController,
    bindings: {
      data: '<',
      outputData: '<',
      node: '<',
      options: '<'
    }
  })

  TreePickerController.inject = [];

  function TreePickerController() {

    var ctrl = this;

    function traverseTree(tree) {
      if (tree.nodes != null) {
        tree.nodes.forEach(function (node) {
          node.parent = tree
        })
        if (nodeHasChildren(tree))
        traverseNodes(tree.nodes, traverseTree)
      }
    }

    ctrl.$onInit = function () {
      if (ctrl.node == null) {
        ctrl.treeCollection = Object.assign(ctrl.data);

        ctrl.treeCollection.nodes.forEach(function (tree) {
          traverseTree(tree)
        });
        traverseNodes(ctrl.treeCollection.nodes, buildNode);
      } else {
        ctrl.treeCollection = ctrl.node;
      }
    }

    function buildNode(node) {
      if (ctrl.options.orgPicker) {
        node.clickedNode = clickedNode;
      } else if (ctrl.options.topSelectOnly) {
        node.clickedNode = clickedNodeForTopSelect;
      } else {
        node.clickedNode = clickedNodeForSingleSelect;
      }
      node.isSelectedInUI = false;
      if (nodeHasChildren(node)) {
        node.isOpenInUI = false;
        node.openNode = openNode;
      }
    }

    function clickedNode(node) {
      toggleNode(node);
      toggleParent(node);

      if (node.nodes != null)
      traverseNodes(node.nodes, toggleNode)
    }

    function clickedNodeForTopSelect(node) {
      toggleNode(node);
      if (node.parent == null || node.isSelectedInUI)
      toggleParentForTopSelect(node);
      else
      traverseNodes(node.nodes, removeNode);
    }

    var root;
    function clickedNodeForSingleSelect(node) {
      if (node.parent == null) { // we are clicking on a root
        if (root != null) { //swap root when clicking new root
          swapRootWhenClickingNew(node);
        } else { //select first root
          clickedNode(node)
          root = node;
        }
      } else { // we are not clicking on a root
        if (root == null) { //find root on first time click
          root = findMyRoot(node);
          clickedNode(node);
        } else { //we have clicked something else before this
          var tempRoot = findMyRoot(node);
          if (tempRoot === root) { //we clicked something else in this branch before this
            clickedNodeIsInSameBranch(node);
          } else { //Its not the same branch as last click
            traverseNodes(root.nodes, removeNode);
            removeNode(root);
            clickedNode(node);
            root = findMyRoot(node);
          }
        }
      }
    }

    function swapRootWhenClickingNew(node) {
      removeNode(root);
      traverseNodes(root.nodes, removeNode);
      addNode(node);
      traverseNodes(node.nodes, addNode);
      root = node;
    }

    function clickedNodeIsInSameBranch(node) {
      if (node.nodes.length){ //its not a leaf
        toggleNode(node)
        if (node.nodes.some(isAnySiblingsSelected)) {
          traverseNodes(node.nodes, removeNode)
        } else {
          addNode(root);
          traverseNodes(node.nodes, addNode)
        }
      } else { //it is a leaf
        if (node.parent.nodes.some(isAnySiblingsSelected)) {
          clickedNode(node)
        } else {
          reverseTraverseNodes(node, addNode);
        }
      }
    }

    function toggleParentForTopSelect(node) {
      if (node.parent == null) {
        if (!node.isSelectedInUI) {
          traverseNodes(node.nodes, removeNode)
        }
      } else {
        toggleParent(node)
      }
    }

    function toggleParent(node) {
      if (node.parent != null) {
        var found = false;
        for (var i = 0; i < node.parent.nodes.length && !found; i++) {
          if (node.parent.nodes[i].isSelectedInUI) {
            addNode(node.parent)
            found = true;
          }
        }
        if (!found) {
          removeNode(node.parent)
        }
        toggleParent(node.parent)
      }
    }

    function toggleNode(node) {
      if (isNodeSelected(node.id)) {
        removeNode(node);
      } else {
        addNode(node)
      }
    }

    //goes down from node
    function traverseNodes(nodes, callback) {
      nodes.forEach(function (node) {
        callback(node);
        if (nodeHasChildren(node))
        traverseNodes(node.nodes, callback);
      });
    }

    //goes up from node, skips root
    function reverseTraverseNodes(node, callback) {
      if ( node.parent == null) {
        return;
      } else {
        callback(node);
        return reverseTraverseNodes(node.parent, callback);
      }
    }

    function addNode(node) {
      if (!isNodeSelected(node.id)) {
        node.isSelectedInUI = true;
        ctrl.outputData.push(node.id);
      }
    }

    function removeNode(node) {
      node.isSelectedInUI = false;
      var index = ctrl.outputData.indexOf(node.id);
      ctrl.outputData.splice(index, 1);
    }

    function openNode(node) {
      node.isOpenInUI = !node.isOpenInUI;
    }

    function isNodeSelected(id) {
      return ctrl.outputData.indexOf(id) > -1;
    }

    function isAnySiblingsSelected(node) {
      return node.isSelectedInUI === true;
    }

    function findMyRoot(node) {
      if (node.parent == null){
        return node;
      } else{
        return findMyRoot(node.parent);
      }
    }

    function nodeHasChildren(node) {
      return node.nodes != null && node.nodes.length;
    }

  }

})();
