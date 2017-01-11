function PermissionsListController(permissionsDataService, NoticeService, $translate) {
	var ctrl = this;

	ctrl.toggleNode = toggleNode;

	function toggleNode(node) {

		if (angular.isUndefined(ctrl.selectedRole) || !ctrl.selectedRole.Id ){
      NoticeService.warning($translate.instant('NeedToSelectARoleFirst'), 5000, true);
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

		toggleSelection(node);

		// if (!angular.isUndefined(ctrl.datafilter) && ctrl.datafilter.isUnSelected) {
		// 	ctrl.datafilter.unselectedDataFilter();
		// }

		ctrl.onClick(node);
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
			if (!ctrl.org.BusinessUnit.IsSelected) {
				ctrl.org.BusinessUnit.IsSelected = true;
			}

			for (var i = 0; i < node.ChildNodes.length; i++) {
				node.ChildNodes[i].IsSelected = selectedOrNot;
			}
			var noSiteSelected = ctrl.org.BusinessUnit.ChildNodes.every(function(site) {
				return !site.IsSelected;
			});
			if (noSiteSelected) {
				ctrl.org.BusinessUnit.IsSelected = false;
			}
		}
		if (node.Type === 'Team') {
			if (!ctrl.org.BusinessUnit.IsSelected) {
				ctrl.org.BusinessUnit.IsSelected = true;
			}
			traverseNodes(node);
		}
	}

	function traverseNodes(node) {
		var sites = ctrl.org.BusinessUnit.ChildNodes;


		for (var i = 0; i < sites.length; i++) {
			for (var j = 0; j < sites[i].ChildNodes.length; j++) {
				if (sites[i].ChildNodes[j].Id === node.Id) {
					var notAllTeamsInSiteSelected = sites[i].ChildNodes.some(function(team) {
						return team.IsSelected;
					});
					if (notAllTeamsInSiteSelected) {
						sites[i].IsSelected = true;
					} else {
						sites[i].IsSelected = false;
						var notAllSitesInBuSelected = sites.some(function(site) {
							return site.IsSelected;
						});
						if (!notAllSitesInBuSelected) {
							ctrl.org.BusinessUnit.IsSelected = false;
						}
					}

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
		selectedRole: '=',
		onClick: '=',
		datafilter: '='
	}
});
