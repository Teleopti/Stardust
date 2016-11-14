function PermissionsListController() {
  var ctrl = this;

  ctrl.toggleNode = toggleNode;

  // function toggleAllFunction () {
  //   vm.applicationFunctions[0].IsSelected = !vm.applicationFunctions[0].IsSelected;
  //
  //   var selectedOrNot = vm.applicationFunctions[0].IsSelected ? true : false;
  //   vm.selectedOrNot = selectedOrNot;
  //   toggleSelection(vm.applicationFunctions, selectedOrNot);
  // }
  //
  // function toggleSelection(functions, selectedOrNot){
  //   for (var i = 0; i < functions.length; i++) {
  //     functions[i].IsSelected = selectedOrNot;
  //     if(functions[i].ChildFunctions != null && functions[i].ChildFunctions.length > 0) {
  //       toggleSelection(functions[i].ChildFunctions, selectedOrNot);
  //     }
  //   }
  // }

  function toggleNode(node) {
    node.IsSelected = !node.IsSelected;
    var selectedOrNot = node.IsSelected;

    if (node.Type === "Site") {
      ctrl.org.BusinessUnit.IsSelected = true;

      for (var i = 0; i < node.ChildNodes.length; i++) {
        node.ChildNodes[i].IsSelected = selectedOrNot;
      }
    }
    if (node.Type === "Team") {
      ctrl.org.BusinessUnit.IsSelected = true;
      traverseNodes(node);
    }
  }

  function traverseNodes(node){
    var sites = ctrl.org.BusinessUnit.ChildNodes;

    for (var i = 0; i < sites.length; i++) {
      for (var j = 0; j < sites[i].ChildNodes.length; j++) {
         if (sites[i].ChildNodes[j].Id === node.Id) {
          sites[i].IsSelected = true;
          var isAllSelected = sites[i].ChildNodes.every(function(team){
            return !team.IsSelected;
          });
          if (isAllSelected) {
            sites[i].IsSelected = false;
          }
         }
      }
    }



  }

  // function traverseNodes (node){
  //   var sites = ctrl.org.BusinessUnit.ChildNodes;
  //
  //   for (var i = 0; i < sites.length; i++) {
  //     for (var j = 0; j < sites[i].ChildNodes.length; j++) {
  //       if (sites[i].ChildNodes[j].Id === node.Id) {
  //         sites[i].IsSelected = true;
  //         if (!sites[i].ChildNodes[j].IsSelected){
  //           sites[i].IsSelected = false;
  //         }
  //       }
  //     }
  //   }
  // }
}



angular.module('wfm.permissions').component('permissionsList', {
  templateUrl: 'app/permissions/refact/organizationrefact/permissions-organization-list.html',
  controller: PermissionsListController,
  bindings: {
    org: '=',
  }
});
