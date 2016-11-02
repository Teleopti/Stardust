function PermissionsListController() {
  var ctrl = this;

  ctrl.toggleNode = toggleNode;

  function toggleNode(node) {
    node.IsSelected = !node.IsSelected;
  }
}

angular.module('wfm.permissions').component('permissionsList', {
  templateUrl: 'app/permissions/refact/organizationrefact/permissions-organization-list.html',
  controller: PermissionsListController,
  bindings: {
    bu: '=',
  }
});

