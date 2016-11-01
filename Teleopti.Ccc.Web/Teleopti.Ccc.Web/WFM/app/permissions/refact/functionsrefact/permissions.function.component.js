function PermissionsTreeController() {
  var ctrl = this;

  ctrl.toggleFunction = toggleFunction;
  ctrl.onSelect = onSelect;

  function toggleFunction(func) {
    func.IsSelected = !func.IsSelected;
    if (ctrl.parent != null) {
      ctrl.parent(func);
    }
  }

  function onSelect(func) {
    var parent = ctrl.functions.find(function(fn) {
      return fn.ChildFunctions != null && fn.ChildFunctions.indexOf(func) !== -1;
    });

    if (parent != null) {
      parent.IsSelected = true;
      ctrl.functions.filter(function(fn) { return fn !== parent }).forEach(function(fn) { fn.IsSelected = false });

      if (ctrl.parent != null) {
        ctrl.parent(parent);
      }
    }
  }

}

angular.module('wfm.permissions').component('permissionsTree', {
  templateUrl: 'app/permissions/refact/functionsrefact/permissions-function-tree.html',
  controller: PermissionsTreeController,
  bindings: {
    functions: '=',
    parent: '='
  }
});