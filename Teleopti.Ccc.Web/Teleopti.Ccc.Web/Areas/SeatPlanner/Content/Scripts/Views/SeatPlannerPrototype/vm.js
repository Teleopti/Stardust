define([
	'knockout',
	'navigation',
	'lazy',
	'resources',
	'moment',
	'momentTimezoneData',
	'shared/timezone-current',
	'locations',
	'Views/TreeList/vm',
	'Views/SeatPlannerPrototype/addSeatPlan'
], function (
	ko,
	navigation,
	lazy,
	resources,
	moment,
	momentTimezoneData,
	timezoneCurrent,
	locations,
	treeListVm,
	addSeatPlan
	) {

	return function () {

		var self = this;
		var businessUnitId;
		this.LocationTreeListViewModel = new treeListVm();
		this.TeamsTreeListViewModel = new treeListVm();
		this.Loading = ko.observable(false);
		this.Resources = resources;
		this.ShowMessage = ko.observable(true);
		this.Today = moment.tz(timezoneCurrent.IanaTimeZone()).startOf('day');
		this.StartDate = ko.observable(moment.tz(timezoneCurrent.IanaTimeZone()).startOf('day'));
		this.EndDate = ko.observable(moment.tz(timezoneCurrent.IanaTimeZone()).startOf('day').add(1, 'day'));

		//Temporary for initial prototype...load locations from json string.
		this.RootLocation = locations.call().getLocations()[0];
		this.TeamHierarchy = ko.observable();
		this.MapLocationsToTreeNodes = function () {
			return this.MapLocationToTreeNode(self.RootLocation);
		};

		this.MapLocationToTreeNode = function (location) {
			var childLocations = location.childLocations;
			var childNodes = [];
			if (childLocations !== undefined) {
				for (var i = 0, len = childLocations.length; i < len; i++) {
					childNodes.push(self.MapLocationToTreeNode(childLocations[i]));
				}
			}
			var locationName = location.name;
			if (location.seats !== undefined) {
				locationName += " ( Seats: " + location.seats.length + ")";
			}
			var treeNode = { id: location.id, name: locationName, children: childNodes, payload: location };
			return treeNode;
		};

		this.GetTreeNodesForLocations = this.LocationTreeListViewModel.createNodeFromJson(this.MapLocationsToTreeNodes());

		this.MapTeamsToTreeNode = function(site) {
			
			var teams = [];
			for (var i = 0, len = site.Teams.length; i < len; i++) {
				var team = site.Teams[i];
				var teamTreeNode = { id: team.Id, name: team.Name + " (Agents: "+team.NumberOfAgents+")", children: [], payload: team };
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

		this.MapTeamHierarchyToTreeNodes = function(teamHierarchy) {
			var businessUnit = teamHierarchy;
			var siteNodes = self.MapSitesToTreeNodes(businessUnit.Sites);
			var businessUnitTreeNode = { id: businessUnit.Id, name: businessUnit.Name, children: siteNodes, payload: null };
			return businessUnitTreeNode;
		}

		this.TeamHierarchyNodes = [];

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

		this.ErrorMessages = ko.computed(function () {
			var result = [];
			if (self.StartDateGreaterThanEndDate()) {
				result.push({ error: resources.StartDateMustBeSmallerThanEndDate });
			}
			if (self.StartDateLessThanToday()) {
				result.push({ error: resources.StartDateMustBeGreaterThanToday });
			}
			return result;
		});

		this.HasErrors = ko.computed(function () {
			return self.ErrorMessages().length > 0;
		});

		this.PerformSeatPlanning = function () {
			var selectedLocations = this.LocationTreeListViewModel.getSelectedLeafNodes();
			var selectedTeams = this.TeamsTreeListViewModel.getSelectedLeafNodes();

			//Robtodo: Validation routines will be called from here.

			var addSeatPlanMgr = new addSeatPlan();

			var data = {
				StartDate: self.StartDate,
				EndDate: self.EndDate,
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

		this.SetViewOptions = function (options) {

			businessUnitId = options.buid;

			//self.Date(function() {
			//	var date = options.date;++
			//	if (date == undefined) {
			//		return moment.tz(timezoneCurrent.IanaTimeZone()).startOf('day');
			//	} else {
			//		return moment.tz(moment(date, 'YYYYMMDD').format('YYYY-MM-DD'), timezoneCurrent.IanaTimeZone());
			//	}
			//}());

		};


	};
});
