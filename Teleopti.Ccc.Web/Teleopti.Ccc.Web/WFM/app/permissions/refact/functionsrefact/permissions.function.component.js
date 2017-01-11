function PermissionsTreeController(permissionsDataService, NoticeService, $translate) {
  var ctrl = this;

  ctrl.toggleFunction = toggleFunction;
  ctrl.onSelect = onSelect;
  ctrl.checkParent = checkParent;

  function toggleFunction(func) {
    if (angular.isUndefined(ctrl.selectedRole) || !ctrl.selectedRole.Id ){
      // NoticeService.warning($translate.instant(''), 5000, true);
      return;
    }

    if (ctrl.selectedRole != null && ctrl.selectedRole.BuiltIn) {
      NoticeService.warning($translate.instant('ChangesAreDisabled'), 5000, true);
      return;
    }

    if (ctrl.selectedRole != null && ctrl.selectedRole.IsMyRole) {
      NoticeService.warning($translate.instant('CanNotModifyMyRole'), 5000, true);
      return;
    }

    ctrl.select(func);

    if (!angular.isUndefined(ctrl.filterFunc) && ctrl.filterFunc.isSelected) {
      ctrl.filterFunc.selectedFunctionsFilter();
    }

    if (!angular.isUndefined(ctrl.filterFunc) && ctrl.filterFunc.isUnSelected) {
      ctrl.filterFunc.unSelectedFunctionsFilter();
    }

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
    selectedRole: '=',
    filterFunc: '='
  }
});
