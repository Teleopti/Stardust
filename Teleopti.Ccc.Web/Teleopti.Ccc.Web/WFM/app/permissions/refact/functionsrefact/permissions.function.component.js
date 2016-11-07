function PermissionsTreeController() {
  var ctrl = this;

  ctrl.toggleFunction = toggleFunction;
  ctrl.onSelect = onSelect;
  ctrl.checkParent = checkParent;

  function toggleFunction(func) {
    func.IsSelected = !func.IsSelected;
    if (!func.IsSelected) {
      var childs = func.ChildFunctions;
      while (childs != null && childs.length > 0) {
        var next = [];
        childs.forEach(function (fn) {
          fn.IsSelected = false;
          if (fn.ChildFunctions != null) {
            next = next.concat(fn.ChildFunctions);
          }
        });
        childs = next;
      }
    }
    if (ctrl.parent != null) {
      ctrl.parent(func);
    }
  }

  function onSelect(func) {
    var parent = ctrl.functions.find(function (fn) {
      return fn.ChildFunctions != null && fn.ChildFunctions.indexOf(func) !== -1;
    });

    if (parent != null) {
      parent.IsSelected = true;
      ctrl.functions.filter(function (fn) { return fn !== parent }).forEach(function (fn) { fn.IsSelected = false });

      if (ctrl.parent != null) {
        ctrl.parent(parent);
      }
    }
  }

  function checkParent(func) {
    if(func.ChildFunctions.length > 0 ){
      if(func.IsSelected){
        func.multiDeselectModal = true;
      }
    }else{
      toggleFunction(func);
     func.multiDeselectModal = false;
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
