function PermissionsListController(permissionsDataService) {
  var ctrl = this;

  ctrl.toggleNode = toggleNode;

  function toggleNode(node) {
    toggleSelection(node);
    permissionsDataService.selectOrganization(node);
  }

  function toggleSelection(node) {
    node.IsSelected = !node.IsSelected;
    var selectedOrNot = node.IsSelected;
    
    if (node.Type === 'BusinessUnit') {
      for (var i = 0; i < node.ChildNodes.length; i++) {
        node.ChildNodes[i].IsSelected = selectedOrNot;
        for (var j = 0; j < node.ChildNodes[i].ChildNodes.length; j++) {
          node.ChildNodes[i].ChildNodes[j].IsSelected = selectedOrNot;
        }
      }
    }

    if (node.Type === 'Site') {
      ctrl.org.BusinessUnit.IsSelected = selectedOrNot;

      for (var i = 0; i < node.ChildNodes.length; i++) {
        node.ChildNodes[i].IsSelected = selectedOrNot;
      }
    }
    if (node.Type === 'Team') {
      ctrl.org.BusinessUnit.IsSelected = selectedOrNot;
      traverseNodes(node);
    }
  }

  function traverseNodes(node){
    var sites = ctrl.org.BusinessUnit.ChildNodes;

    for (var i = 0; i < sites.length; i++) {
      for (var j = 0; j < sites[i].ChildNodes.length; j++) {
        if (sites[i].ChildNodes[j].Id === node.Id) {
          var isAllSelected = sites[i].ChildNodes.every(function(team){
            return team.IsSelected;
          });
          sites[i].IsSelected = isAllSelected;
        }
      }
    }
  }
}

angular.module('wfm.permissions').component('permissionsList', {
  templateUrl: 'app/permissions/refact/organizationrefact/permissions-organization-list.html',
  controller: PermissionsListController,
  bindings: {
    org: '=',
  }
});
