function PermissionsListController(permissionsDataService, NoticeService, localeLanguageSortingService, $translate) {
	var ctrl = this;
	ctrl.toggleNode = toggleNode;
	ctrl.sortByLocaleLanguage = localeLanguageSortingService.sort;

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

		var orgNode = toggleSelection(node);

		ctrl.onClick(orgNode);
	}

	function toggleSelection(node) {
		var originalNode = ctrl.select(node);
		var selectedOrNot = ctrl.isSelected(node);

		if (node.Type === 'BusinessUnit') {
			for (var i = 0; i < node.ChildNodes.length; i++) {
				ctrl.select(node.ChildNodes[i], selectedOrNot);
				for (var j = 0; j < node.ChildNodes[i].ChildNodes.length; j++) {
					ctrl.select(node.ChildNodes[i].ChildNodes[j], selectedOrNot);
				}
			}
		}

		if (node.Type === 'Site') {
			if (!ctrl.isSelected(ctrl.org.BusinessUnit)) {
				ctrl.select(ctrl.org.BusinessUnit);
			}

			for (var i = 0; i < node.ChildNodes.length; i++) {
				ctrl.select(node.ChildNodes[i], selectedOrNot);
			}
			
			var noSiteSelected = ctrl.originalOrg.BusinessUnit.ChildNodes.every(function(site) {
				return !ctrl.isSelected(site);
			});
			if (noSiteSelected) {
				ctrl.select(ctrl.org.BusinessUnit, false);
			}
		}
		if (node.Type === 'Team') {
			if (!ctrl.isSelected(ctrl.org.BusinessUnit)) {
				ctrl.select(ctrl.org.BusinessUnit, true);
			}
			traverseNodes(node);
		}
		return originalNode;
		
	}

	function traverseNodes(node) {
		var sites = ctrl.org.BusinessUnit.ChildNodes;

		for (var i = 0; i < sites.length; i++) {
			var site = sites[i];
			for (var j = 0; j < site.ChildNodes.length; j++) {
				var team = site.ChildNodes[j];
				if (team.Id === node.Id) {
					var anyTeamSelected = site.ChildNodes.some(function(team) {
						return ctrl.isSelected(team);
					});
					if (anyTeamSelected) {
						ctrl.select(site, true);
					} else {
						ctrl.select(site, false);
						var notAllSitesInBuSelected = sites.some(function(s) {
							return ctrl.isSelected(s);
						});
						if (!notAllSitesInBuSelected) {
							ctrl.select(ctrl.org.BusinessUnit, false);
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
		originalOrg: '=',
		org: '=',
		selectedRole: '=',
		onClick: '=',
		datafilter: '=',
		isSelected: '=',
		select: '='
	}
});
