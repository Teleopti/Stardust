define([
	'knockout',
	'navigation',
	'lazy',
	'resources',
	'moment',
	'momentTimezoneData',
	'Views/TreeList/vm',
	'Views/SeatPlannerPrototype/addSeatPlan'
], function (
	ko,
	navigation,
	lazy,
	resources,
	moment,
	momentTimezoneData,
	treeListVm,
	addSeatPlan
	) {

	return function () {
		var self = this;
		var businessUnitId;
		this.Resources = resources;
		this.LocationTreeListViewModel = new treeListVm(false);
		this.TeamsTreeListViewModel = new treeListVm(true);
		this.Loading = ko.observable(false);
		this.Today = moment().startOf('day');
		this.StartDate = ko.observable(moment().startOf('day'));
		this.EndDate = ko.observable(moment().startOf('day'));
		this.ApplyingSeatPlanning = ko.observable(false);
		this.TeamHierarchy = ko.observable();
		this.LocationHierarchy = ko.observable();
		this.TeamHierarchyNodes = [];
		this.LocationHierarchyNodes = [];

		this.MapLocationToTreeNode = function (location) {

			if (!location) return null;

			var childLocations = location.Children;
			var childNodes = [];
			if (childLocations !== null) {
				for (var i = 0, len = childLocations.length; i < len; i++) {
					childNodes.push(self.MapLocationToTreeNode(childLocations[i]));
				}
			}
			var locationName = location.Name;
			if (location.Seats !== null) {
				locationName += " ( Seats: " + location.Seats.length + ")";
			}
			var treeNode = { id: location.Id, name: locationName, children: childNodes, payload: location };
			return treeNode;
		};

		this.GetTreeNodesForLocations = ko.computed({
			read: function () {
				var locationHierarchy = self.LocationHierarchy();
				if (locationHierarchy !== undefined) {
					//RobTodo: review: fix this better so binding doesnt reload this list.
					if (self.LocationHierarchyNodes.length == 0) {
						self.LocationHierarchyNodes = self.LocationTreeListViewModel.createNodeFromJson(self.MapLocationToTreeNode(locationHierarchy));
					}
					return self.LocationHierarchyNodes;
				} else {
					return null;
				}
			},
			deferEvaluation: true
		});

		this.MapTeamsToTreeNode = function (site) {

			var teams = [];
			for (var i = 0, len = site.Teams.length; i < len; i++) {
				var team = site.Teams[i];
				var teamTreeNode = { id: team.Id, name: team.Name + " (Agents: " + team.NumberOfAgents + ")", children: [], payload: team };
				teams.push(teamTreeNode);
			}
			return teams;
		}

		this.MapSitesToTreeNodes = function (sites) {
			var siteNodes = [];
			for (var i = 0, len = sites.length; i < len; i++) {
				var site = sites[i];
				var teams = self.MapTeamsToTreeNode(site);
				var siteNode = { id: site.Id, name: site.Name, children: teams, payload: site };
				siteNodes.push(siteNode);
			}
			return siteNodes;
		};

		this.MapTeamHierarchyToTreeNodes = function (teamHierarchy) {
			var businessUnit = teamHierarchy;
			var siteNodes = self.MapSitesToTreeNodes(businessUnit.Sites);
			var businessUnitTreeNode = { id: businessUnit.Id, name: businessUnit.Name, children: siteNodes, payload: null };
			return businessUnitTreeNode;
		}

		this.GetTreeNodesForBusinessHierarchy = ko.computed({
			read: function () {
				var teamHierarchy = self.TeamHierarchy();
				if (teamHierarchy !== undefined) {
					//RobTodo: review: fix this better so binding doesnt reload this list.
					if (self.TeamHierarchyNodes.length == 0) {
						self.TeamHierarchyNodes = self.TeamsTreeListViewModel.createNodeFromJson(self.MapTeamHierarchyToTreeNodes(teamHierarchy));

					}
					return self.TeamHierarchyNodes;
				} else {
					return null;
				}
			},
			deferEvaluation: true
		});

		this.StartDateFormatted = ko.computed(function () {
			return self.StartDate().format(resources.DateFormatForMoment);
		});

		this.EndDateFormatted = ko.computed(function () {
			return self.EndDate().format(resources.DateFormatForMoment);
		});

		this.StartDateGreaterThanEndDate = ko.computed(function () {
			return self.StartDate() > self.EndDate();
		});

		this.StartDateLessThanToday = ko.computed(function () {
			return self.StartDate() < self.Today;
		});

		this.NoLocationSelected = function () {
			return (self.LocationTreeListViewModel.getSelectedLeafNodes() == 0);
		};

		this.NoTeamSelected = function () {
			return (self.TeamsTreeListViewModel.getSelectedLeafNodes() == 0);
		};

		this.ErrorMessages = ko.computed(function () {
			var result = [];
			if (self.StartDateGreaterThanEndDate()) {
				result.push({ error: resources.StartDateMustBeEndDateOrSmaller });
			}
			if (self.StartDateLessThanToday()) {
				result.push({ error: resources.StartDateMustBeTodayOrGreater });
			}

			if (self.ApplyingSeatPlanning()) {
				if (self.NoLocationSelected()) {
					result.push({ error: resources.AnyLocationShouldBeSelected });
				}
				if (self.NoTeamSelected()) {
					result.push({ error: resources.AnyTeamShouldBeSelected });
				}
			}
			return result;
		});

		this.HasErrors = ko.computed(function () {
			return self.ErrorMessages().length > 0;
		});

		this.PerformSeatPlanning = function () {
			var selectedLocations = this.LocationTreeListViewModel.getSelectedLeafNodes();
			var selectedTeams = this.TeamsTreeListViewModel.getSelectedLeafNodes();

			self.ApplyingSeatPlanning(true);

			if (selectedLocations.length == 0 || selectedTeams.length == 0) {
				return;
			}

			var addSeatPlanMgr = new addSeatPlan();

			var data = {
				StartDate: self.StartDate().format('YYYY-MM-DD HH:mm'),
				EndDate: self.EndDate().format('YYYY-MM-DD HH:mm'),
				BusinessUnitId: businessUnitId,
				Teams: selectedTeams,
				Locations: selectedLocations
			}

			addSeatPlanMgr.SetData(data);
			addSeatPlanMgr.Apply();

		};

		this.LoadTeams = function (data) {
			this.TeamHierarchy(data);
		};

		this.LoadLocations = function (data) {
			
			self.LocationHierarchy(data);
		};

		this.SetViewOptions = function (options) {
			businessUnitId = options.buid;
		};


	};
});
