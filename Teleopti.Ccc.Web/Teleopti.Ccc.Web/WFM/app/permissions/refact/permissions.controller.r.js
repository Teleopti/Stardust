(function() {
	'use strict';

	angular
		.module('wfm.permissions')
		.controller('PermissionsRefactController', PermissionsCtrl);

	PermissionsCtrl.$inject = ['$filter', 'PermissionsServiceRefact', 'permissionsDataService'];

	function PermissionsCtrl($filter, PermissionsServiceRefact, permissionsDataService) {
		var vm = this;
		vm.showCreateModal;
		vm.roleName;

		fetchData();
		vm.createRole = createRole;
		vm.editRole = editRole;
		vm.deleteRole = deleteRole;
		vm.copyRole = copyRole;
		vm.selectRole = selectRole;
		vm.checkMyRole = checkMyRole;
		vm.prepareTree = prepareTree;
		vm.toggleAllFunction = toggleAllFunction;
		vm.unSelectedFunctionsFilter = unSelectedFunctionsFilter;
		vm.selectedFunctionsFilter = selectedFunctionsFilter;
		vm.allFunctionsFilter = allFunctionsFilter;
		vm.isFunctionSelected = isFunctionSelected;
		vm.selectFunction = selectFunction;
		vm.selectedOrNot = false;
		vm.selectedRole = {};
		vm.selectedFunctions = {};
		vm.componentFunctions = [];
		vm.selectedOrgData = {};
		vm.listHandler = listHandler;
		vm.selectDynamicOption = selectDynamicOption;
		vm.allDataFilter = allDataFilter;
		vm.selectedDataFilter = selectedDataFilter;
		vm.unselectedDataFilter = unselectedDataFilter;
		vm.filteredOrganizationSelection = {};
		vm.onFunctionClick = functionClick;
		vm.nodeClick = nodeClick;
		vm.functionsDescriptionFilter = functionsDescriptionFilter;
		vm.orgDataDescriptionFilter = orgDataDescriptionFilter;
		vm.isOrgDataSelected = isOrgDataSelected;
		vm.selectOrgData = selectOrgData;

		var functionsFilter = $filter('functionsFilter');
		var dataFilter = $filter('dataFilter');
		var descriptionFilter = $filter('newDescriptionFilter');

		function createRole(roleName) {
			if (angular.isUndefined(roleName)) {
				return;
			}

			var roleData = {
				Description: roleName
			};
			PermissionsServiceRefact.roles.save(roleData).$promise.then(function(data) {
				vm.roles.unshift(data);
				vm.selectedRole = data;
				refreshRoleSelection();
				markSelectedRole(vm.selectedRole);
			});
			vm.showCreateModal = false;
			vm.roleName = '';
			toggleSelection(vm.applicationFunctions, false);

			if (vm.organizationSelection !== null && vm.organizationSelection.BusinessUnit !== null) {
				toggleOrganizationSelecton(vm.organizationSelection.BusinessUnit, false);
			}
		}

		function editRole(newRoleName, role) {
			PermissionsServiceRefact.manage.update({
				Id: role.Id,
				newDescription: newRoleName
			}).$promise.then(function() {
				PermissionsServiceRefact.roles.query().$promise.then(function(data) {
					vm.roles = data;
					var selected = data.find(function(r) {
						return r.Id === role.Id;
					});
					markSelectedRole(selected);
				});
			});
		}

		function deleteRole(role) {
			if (role.BuiltIn) {
				return;
			}

			PermissionsServiceRefact.manage.deleteRole({
				Id: role.Id
			});
			var index = vm.roles.indexOf(role);
			vm.roles.splice(index, 1);

			if (role.Id === vm.selectedRole.Id) {
				toggleSelection(vm.applicationFunctions, false);
			}

			if (vm.organizationSelection !== null && vm.organizationSelection.BusinessUnit !== null) {
				toggleOrganizationSelecton(vm.organizationSelection.BusinessUnit, false);
			}
			vm.selectedRole = {};
			vm.selectedOrNot = false;
		}

		function copyRole(role) {
			PermissionsServiceRefact.copyRole.copy({
				Id: role.Id
			}).$promise.then(function(data) {
				vm.roles.unshift(data);
				vm.selectedRole = data;

				refreshRoleSelection();
				markSelectedRole(vm.selectedRole);
			});
		}

		function selectRole(role, keepSelectedRole) {
			if (!keepSelectedRole) {
				markSelectedRole(role);
			}

			PermissionsServiceRefact.manage.getRoleInformation({
				Id: role.Id
			}).$promise.then(function(data) {
				vm.selectedRole = data;
				vm.selectedRole.IsMyRole = role.IsMyRole;

				if (vm.selectedRole.AvailableTeams != null && vm.selectedRole.AvailableTeams.length > 0) {
					matchOrganizationData();
				}
				if (vm.selectedRole.AvailableTeams != null && vm.selectedRole.AvailableTeams.length < 1) {
					toggleOrganizationSelecton(vm.organizationSelection.BusinessUnit, false);
				}
				matchFunctionsForSelctedRole();

				if (vm.functionsFilterObj.isSelected) {
					selectedFunctionsFilter();
				} else if (vm.functionsFilterObj.isUnSelected) {
					unSelectedFunctionsFilter();
				} else if (vm.dataFilterObj.isSelected) {
					selectedDataFilter();
				} else if (vm.dataFilterObj.isUnSelected) {
					unselectedDataFilter();
				}

				if (vm.selectedRole.AvailableFunctions) {
					var hasAll = vm.selectedRole.AvailableFunctions.find(function(func) {
						return func.FunctionCode === 'All';
					});
					if (hasAll) {
						vm.selectedOrNot = true;
					} else {
						vm.selectedOrNot = false;
					}
				}
			});
		}

		function matchFunctionsForSelctedRole() {
			function loop(functions) {
				if (functions == null) {
					return;
				}
				functions.forEach(function(f) {
					vm.selectedFunctions[f.Id] = true;
					loop(f.ChildFunctions);
				});
			}
			vm.selectedFunctions = {};
			loop(vm.selectedRole.AvailableFunctions);
		}

		var previously = null;

		function markSelectedRole(role) {
			if (previously != null) {
				previously.IsSelected = false;
			}
			role.IsSelected = true;
			previously = role;
		}

		function checkMyRole(role) {
			if (role.IsMyRole || role.BuiltIn) {
				return false;
			} else {
				return true;
			}
		}

		function refreshRoleSelection() {
			for (var i = 0; i < vm.roles.length; i++) {
				vm.roles[i].IsSelected = false;
			}
			vm.roles[0].IsSelected = true;
		}

		//ORGANIZATION
		function nodeClick(node) {
			if (vm.dataFilterObj.isUnSelected) {
				unselectedDataFilter();
			} else if (vm.dataFilterObj.isSelected) {
				selectedDataFilter();
			}
			if (vm.selectedRole != null) {
				var selected = vm.isOrgDataSelected(node);
				permissionsDataService.selectOrganization(node, vm.selectedRole, selected);
			}
		}

		function toggleOrganizationSelecton(businessUnit, selectedOrNot) {
			vm.selectOrgData(businessUnit, selectedOrNot);
			if (businessUnit.ChildNodes != null) {
				for (var i = 0; i < businessUnit.ChildNodes.length; i++) {
					vm.selectOrgData(businessUnit.ChildNodes[i], selectedOrNot)
					if (businessUnit.ChildNodes[i].ChildNodes != null) {
						for (var j = 0; j < businessUnit.ChildNodes[i].ChildNodes.length; j++) {
							vm.selectOrgData(businessUnit.ChildNodes[i].ChildNodes[j], selectedOrNot)
						}
					}
				}
			}
		}

		function selectOrgData(org, selected) {
			var selectedOriginalNode = permissionsDataService.findOrgData(vm.organizationSelection.BusinessUnit, org.Id)

			if (selected != null) {
				if (selected) {
					vm.selectedOrgData[org.Id] = true;
				} else {
					delete vm.selectedOrgData[selectedOriginalNode.Id];
				}
			} else {
				if (vm.selectedOrgData[org.Id]) {
					delete vm.selectedOrgData[selectedOriginalNode.Id];
					toggleOrganizationSelecton(selectedOriginalNode, false);
				} else {
					vm.selectedOrgData[org.Id] = true;
				}
			}
			return selectedOriginalNode;
		}

		function isOrgDataSelected(org) {
			if (org) {
				return !!vm.selectedOrgData[org.Id];
			}
		}

		function matchOrganizationData() {
			var sites = vm.organizationSelection.BusinessUnit.ChildNodes;

			var selectedOrgData = vm.selectedRole.AvailableTeams.reduce(function(selection, team) {
				selection[team.Id] = true;
				return selection;
			}, {});
			selectedOrgData = sites.reduce(function(selection, site) {
				var teamIsSelected = site.ChildNodes.some(function(team) {
					return !!selection[team.Id];
				});
				if (teamIsSelected) {
					selection[site.Id] = true;
				}
				return selection;
			}, selectedOrgData);
			selectedOrgData[vm.organizationSelection.BusinessUnit.Id] = vm.selectedRole.AvailableTeams.length > 0;

			vm.selectedOrgData = selectedOrgData;
		}

		function selectDynamicOption(option) {
			if (vm.selectedRole.Id) {
				permissionsDataService.selectDynamicOption(option, vm.selectedRole);
			}
		}

		vm.dataFilterObj = {};

		function selectedDataFilter() {
			vm.dataFilterObj.isSelected = true;
			vm.dataFilterObj.isUnSelected = false;
			var data = dataFilter.selected(vm.organizationSelection, vm.selectedOrgData);
			orgDataHandler(data);
		}

		function unselectedDataFilter() {
			vm.dataFilterObj.isSelected = false;
			vm.dataFilterObj.isUnSelected = true;
			var data = dataFilter.unselected(vm.organizationSelection, vm.selectedOrgData);
			orgDataHandler(data);
		}

		function allDataFilter() {
			vm.dataFilterObj.isSelected = false;
			vm.dataFilterObj.isUnSelected = false;
			orgDataHandler(vm.organizationSelection);
		}

		function orgDataHandler(orgData) {
			vm.filteredOrganizationSelection = orgData;
		}

		function orgDataDescriptionFilter(searchString) {
			var data = descriptionFilter.filterOrgData(vm.organizationSelection, searchString);
			orgDataHandler(data);
		}

		//FUNCTIONS
		function isFunctionSelected(func) {
			return !!vm.selectedFunctions[func.FunctionId];
		}

		function selectFunction(func) {
			if (vm.selectedFunctions[func.FunctionId]) {
				var fn = permissionsDataService.findFunction(vm.applicationFunctions, func.FunctionId);
				delete vm.selectedFunctions[fn.FunctionId];
				toggleSelection([fn], false)
			} else {
				vm.selectedFunctions[func.FunctionId] = true;
			}
		}

		function createFlatFunctions(functions, flatFunctions) {
			for (var i = 0; i < functions.length; i++) {
				flatFunctions.push(functions[i]);
				if (functions[i].ChildFunctions != null && functions[i].ChildFunctions.length > 0) {
					createFlatFunctions(functions[i].ChildFunctions, flatFunctions);
				}
			}
		}

		function functionClick(fn) {
			var flatFunctions = [];

			if (vm.selectedRole === null) {
				return;
			}
			var parents = permissionsDataService.findParentFunctions(vm.applicationFunctions, fn)
				.map(function(f) {
					return f.FunctionId;
				});
			var functions = [];
			if (isFunctionSelected(fn)) {
				functions = parents.concat(fn.FunctionId);
			}
			createFlatFunctions(vm.applicationFunctions, flatFunctions);

			vm.selectedOrNot = flatFunctions.every(function(func) {
				return isFunctionSelected(func);
			});
			permissionsDataService.selectFunction(vm.selectedRole, functions, fn);
		}

		function toggleSelection(functions, selectedOrNot) {
			if (functions != null) {
				for (var i = 0; i < functions.length; i++) {
					var func = functions[i];
					if (selectedOrNot) {
						vm.selectedFunctions[func.FunctionId] = true;
					} else {
						delete vm.selectedFunctions[func.FunctionId];
					}
					toggleSelection(func.ChildFunctions, selectedOrNot);
				}
			}
		}

		function toggleAllFunction() {
			vm.selectedOrNot = !vm.selectedOrNot;
			toggleSelection(vm.applicationFunctions, vm.selectedOrNot);

			if (vm.selectedRole != null) {
				permissionsDataService.selectAllFunction(vm.selectedRole, vm.applicationFunctions, vm.selectedOrNot).then(function() {
					selectRole(vm.selectedRole, true);
				});
			}
		}

		function listHandler(arr) {
			vm.componentFunctions = arr;
		}

		function functionsDescriptionFilter(searchString) {
			var filteredArray = descriptionFilter.filterFunctions(vm.applicationFunctions, searchString);
			listHandler(filteredArray);
		}

		vm.functionsFilterObj = {};
		vm.functionsFilterObj.unSelectedFunctionsFilter = unSelectedFunctionsFilter;
		vm.functionsFilterObj.selectedFunctionsFilter = selectedFunctionsFilter;

		function unSelectedFunctionsFilter() {
			vm.functionsFilterObj.isUnSelected = true;
			vm.functionsFilterObj.isSelected = false;
			var filteredArray = functionsFilter.unselected(vm.applicationFunctions, vm.selectedFunctions);
			listHandler(filteredArray);
		}

		function selectedFunctionsFilter() {
			vm.functionsFilterObj.isSelected = true;
			vm.functionsFilterObj.isUnSelected = false;
			var filteredArray = functionsFilter.selected(vm.applicationFunctions, vm.selectedFunctions);
			listHandler(filteredArray);
		}

		function allFunctionsFilter() {
			vm.functionsFilterObj.isSelected = false;
			vm.functionsFilterObj.isUnSelected = false;
			listHandler(vm.applicationFunctions);
		}

		//GENERAL

		function prepareTree(appFunctions) {
			if (appFunctions != null && appFunctions.length > 1) {
				appFunctions[1].IsOpen = true;
				appFunctions[0].IsHidden = true;
			}
		}

		function fetchData() {
			PermissionsServiceRefact.roles.query().$promise.then(function(data) {
				vm.roles = data;
			});
			PermissionsServiceRefact.applicationFunctions.query().$promise.then(function(data) {
				vm.applicationFunctions = data;
				listHandler(vm.applicationFunctions);
				prepareTree(vm.applicationFunctions);
			});
			PermissionsServiceRefact.organizationSelection.get().$promise.then(function(data) {
				vm.organizationSelection = data;
				orgDataHandler(vm.organizationSelection);
			});
		}

	}
})();
