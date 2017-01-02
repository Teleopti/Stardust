function PermissionsTreeController(permissionsDataService, NoticeService, $translate) {
  var ctrl = this;

  ctrl.toggleFunction = toggleFunction;
  ctrl.onSelect = onSelect;
  ctrl.checkParent = checkParent;
  // ctrl.selectedFunctions = [];
  function toggleFunction(func) {
    // ctrl.selectedFunctions = [];
    console.log('selected role', ctrl.selectedRole);


    if (!ctrl.selectedRole){
      // Fixa notice här...
      return;
    }

    //Fixa testerna
    if (ctrl.selectedRole != null && ctrl.selectedRole.BuiltIn) {
      NoticeService.warning($translate.instant('ChangesAreDisabled'), 5000, true);
      return;
    }
    //Fix me
    if (ctrl.selectedRole != null && ctrl.selectedRole.IsMyRole) {
      NoticeService.warning($translate.instant('CanNotModifyMyRole'), 5000, true);
      return;
    }

    ctrl.select(func);

    // if (ctrl.isSelected(func)) {
    //   var parents = permissionsDataService
    //     .findParentFunctions(ctrl.functions, func)
    //     .map(function(fn) { return fn.FunctionId; });
    //   ctrl.selectedFunctions = ctrl.selectedFunctions.concat(func.FunctionId).concat(parents);
    // }

    if (!ctrl.isSelected(func)) {
      permissionsDataService.findChildFunctions(func).forEach(function(fn) {
        if (ctrl.isSelected(fn))
          ctrl.select(fn);
      });
    }
    if (ctrl.parent != null) {
      ctrl.parent(func);
    }

    ctrl.onClick(func);
  }

  function onSelect(func) {
    var parent = ctrl.functions.find(function (fn) {
      return fn.ChildFunctions != null && fn.ChildFunctions.indexOf(func) !== -1;
    });

    if (parent != null && !ctrl.isSelected(parent)) {
      ctrl.select(parent);
      ctrl.functions.filter(function (fn) { return fn !== parent });
      if (ctrl.parent != null) {
        ctrl.parent(parent);
      }
    }
  }

  function checkParent(func) {
    if(func.ChildFunctions.length > 0 && ctrl.isSelected(func)) {
        func.multiDeselectModal = true;
    } else{
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
    isSelected: '=',
    select: '=',
    parent: '=',
    onClick: '=',
    selectedRole: '='
  }
});
