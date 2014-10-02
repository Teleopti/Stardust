define([
		'knockout',
		'views/realtimeadherenceagents/agentstate',
		'views/realtimeadherenceagents/agent',
		'views/realtimeadherenceagents/filter',
		'resources',
		'ajax',
		'helpers',
		'jquery',
		'lazy',
		'navigation'
],
	function (ko,
		agentstate,
		agent,
		filterService,
		resources,
		ajax,
		helpers,
		$,
		lazy,
		navigation) {
	return function () {

		var that = {};
		that.Resources = resources;
		that.permissionAddActivity = ko.observable();

		that.agents = []; 
		that.agentStates = ko.observableArray();
		that.resources = resources;
		that.teamName = ko.observable();
		that.siteName = ko.observable();
		that.siteId = ko.observable();
		that.siteURI = ko.observable();
		that.filter = ko.observable();
		that.groupId = ko.observable();
		that.changeScheduleAvailable = ko.observable(false);
		that.BusinessUnitId = ko.observable();
		that.rootURI = ko.observable();

		that.SetViewOptions = function (options) {
			that.BusinessUnitId(options.buid);
			that.rootURI('#realtimeadherencesites/' + that.BusinessUnitId());
		};

		that.filteredAgents = ko.computed(function() {
			var filter = that.filter();
			if (!filter) {
				return that.agentStates();
			} else {
				return ko.utils.arrayFilter(that.agentStates(), function (item) {
					return filterService().match(
					[
						item.Alarm(),
						item.Name,
						item.State(),
						item.Activity(),
						item.NextActivity(),
						item.NextActivityStartTime(),
						item.TeamName
					], filter);

				});
			}
		});
		
		that.fillAgents = function(data) {
			for (var i = 0; i < data.length; i++) {
				var a = agent();
				a.fill(data[i]);
				that.agents.push(a);
				var locationHash = window.location.hash;
				if (locationHash.match(/MultipleSites$/)) {
					that.teamName('');
					that.siteName('');
				} else if (locationHash.match(/MultipleTeams$/)) {
					that.teamName('');
					that.siteName(data[i].SiteName);
				} else {
					that.teamName(data[i].TeamName);
					that.siteName(data[i].SiteName);
				}

				that.siteId(data[i].SiteId);
			}
			that.siteURI('#realtimeadherenceteams/' + that.BusinessUnitId() +'/' + that.siteId());
		};

		that.fillAgentsStates = function(data) {
			var newagentState, existingState;

			if (!data) {
				that.agents.forEach(function(agent) {
					existingState = that.getExistingAgentState(agent.PersonId);
					if (existingState.length !== 0) {
						existingState[0].fill({ PersonId: agent.PersonId }, agent.Name, agent.TimeZoneOffset, agent.TeamName);			
					} else {
						newagentState = agentstate();
						newagentState.fill({ PersonId: agent.PersonId }, agent.Name, agent.TimeZoneOffset, agent.TeamName);
						that.agentStates.push(newagentState);
					}
				});
			} else {
				for (var i = 0; i < data.length; i++) {
					var a = that.getAgent(data[i].PersonId);
					if (!a) continue;
					existingState = that.getExistingAgentState(data[i].PersonId);
					if (existingState.length !== 0) {
						existingState[0].fill(data[i], a.Name, a.TimeZoneOffset, a.TeamName);
					} else {
						newagentState = agentstate();
						newagentState.fill(data[i], a.Name, a.TimeZoneOffset, a.TeamName);
						that.agentStates.push(newagentState);
					}
				}
			}
			that.agentStates.sort(function(left, right) { return left.Name == right.Name ? 0 : (left.Name < right.Name ? -1 : 1); });
		};
		that.getExistingAgentState = function(id) {
			var existingState = that.agentStates().filter(function(obj) {
						return obj.PersonId === id;
				});
			return existingState;
		};
		that.getAgent = function(id) {
			var agent = that.agents.filter(function(item) {
				return item.PersonId === id;
				});
			return agent[0];
		};
		that.getAgentState = function (id) {
			var agentState = that.agentStates().filter(function (item) {
				return item.PersonId === id;
				});
			return agentState[0];
		};
		that.refreshAlarmTime = function () {
			that.agentStates().forEach(function (item) {
				item.refreshAlarmTime();
			});
		};

		that.updateFromNotification = function (notification) {
			var data = JSON.parse(notification.BinaryData);
			data.Id = notification.DomainId;
			that.fillAgentsStates(data.AgentStates);
		};

		that.urlForChangingSchedule = function (data) {
			var a = that.getAgent(data.PersonId);
			return navigation.UrlForChangingSchedule(that.BusinessUnitId(), a.TeamId, a.PersonId, moment((new Date).getTime()));
		}

		that.getSelectedAgentState = function() {
			var selectedAgentState = that.agentStates().filter(function (obj) {
				return obj.Selected() === true;
			});
			return selectedAgentState;
		}

		that.SelectAgent = function (agentStateClicked) {
			deselectAllAgentsExcept(agentStateClicked);
			agentStateClicked.Selected(!agentStateClicked.Selected());
		};

		var deselectAllAgentsExcept = function (agentState) {
			var selectedAgentStates = lazy(that.agentStates())
				.filter(function (x) {
					if (agentState && x === agentState)
						return false;
					return x.Selected();
				});
			selectedAgentStates.each(function (x) {
				x.Selected(false);
			});
		};
		
		return that;
	};
}
);