(function() {
    'use strict';

    angular
        .module('wfm.permissions')
        .controller('PermissionsCtrlRefact', PermissionsCtrl);

    PermissionsCtrl.$inject = ['PermissionsServiceRefact'];

    function PermissionsCtrl(PermissionsServiceRefact) {
        var vm = this;

        vm.createRole = createRole;
        vm.editRole = editRole;
        vm.deleteRole = deleteRole;
       
        vm.roles = PermissionsServiceRefact.roles.query();

        function createRole(roleName) {
            var roleData = {Description: roleName};
            var newUser = PermissionsServiceRefact.roles.save(roleData);
            vm.roles.unshift(newUser);
        }

        function editRole(newRoleName, id) {
            PermissionsServiceRefact.manage.update({Id: id, newDescription: newRoleName});
        }

        function deleteRole(role) {
            PermissionsServiceRefact.manage.deleteRole({ Id: role.Id });   
            var index = vm.roles.indexOf(role);
            vm.roles.splice(index, 1);
        }
    }
})();

