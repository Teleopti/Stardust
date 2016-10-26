function PermissionsListController() {
}   

angular.module('wfm.permissions').component('permissionsList', {
  templateUrl: 'app/permissions/refact/organizationrefact/permissions-organization-list.html',
  controller: PermissionsListController,
  bindings: {
    bu: '=',
  }
});