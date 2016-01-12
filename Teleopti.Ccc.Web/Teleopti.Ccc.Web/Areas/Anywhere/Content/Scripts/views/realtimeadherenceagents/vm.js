define([
		'knockout',
		'views/realtimeadherenceagents/agent',
		'views/realtimeadherenceagents/filter',
		'resources',
		'ajax',
		'helpers',
		'jquery',
		'lazy',
		'navigation',
		'views/realtimeadherenceagents/getadherence'
],
	function (ko,
		agent,
		filterService,
		resources,
		ajax,
		helpers,
		$,
		lazy,
		navigation,
		getadherence) {
		return function (historicalAdherenceServercall) {

			var that = {};
			that.Resources = resources;
			that.permissionAddActivity = ko.observable();
			that.permissionSendMessage = ko.observable(false);

			var fetchHistoricalAdherence = historicalAdherenceServercall || getadherence.ServerCall;

			that.agents = ko.observableArray();
			that.resources = resources;
			that.teamName = ko.observable();
			that.siteName = ko.observable();
			that.siteId = ko.observable();
			that.siteURI = ko.observable();
			that.filter = ko.observable();
			that.groupId = ko.observable();
			that.BusinessUnitId = "";
			that.rootURI = ko.observable();
			that.agentAdherenceEnabled = ko.observable(false);
			that.agentAdherenceDetailsEnabled = ko.observable(false);
			that.notifyViaSMSEnabled = ko.observable(false);
			that.AgentAdherence = ko.observable();

			that.SetViewOptions = function (options) {
				that.BusinessUnitId = options.buid;
				that.rootURI('#realtimeadherencesites/' + that.BusinessUnitId);
			};

			that.Agents = ko.computed(function () {
				var filter = that.filter();
				if (!filter) {
					return that.agents();
				} else {
					return ko.utils.arrayFilter(that.agents(), function (item) {
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

			that.AllChecked = ko.computed({
				read: function () {
					var firstUnchecked = ko.utils.arrayFirst(that.Agents(), function (item) {
						return item.SelectedToSendMessage() == false;
					});
					return firstUnchecked == null;
				},
				write: function (value) {
					ko.utils.arrayForEach(that.Agents(), function (item) {
						item.SelectedToSendMessage(value);
					});
				}
			});

			that.SendMessage = function () {
				var selectedAgentIds = lazy(that.Agents())
					.filter(function (x) {
						return x.SelectedToSendMessage();
					}).map(function (e) {
						return e.PersonId;
					}).join(",");
				window.open("Messages#" + selectedAgentIds, '_blank');
			};

			that.SendMessageEnabled = ko.computed(function() {
				return lazy(that.Agents())
					.some(function(x) {
						return x.SelectedToSendMessage();
					});
			});

			var fillSiteAndTeam = function(data) {
				var locationHash = window.location.hash;
				if (locationHash.match(/MultipleSites$/)) {
					that.teamName('');
					that.siteName('');
				} else if (locationHash.match(/MultipleTeams$/)) {
					that.teamName('');
					that.siteName(data.SiteName);
				} else {
					that.teamName(data.TeamName);
					that.siteName(data.SiteName);
				}

				that.siteId(data.SiteId);
				that.siteURI('#realtimeadherenceteams/' + that.BusinessUnitId + '/' + that.siteId());
			};

			var fillData = function (data, addIfNotExists) {
				var theAgentState = that.getAgentState(data.PersonId);

				if (!theAgentState && !addIfNotExists)
				    return;

				data.BusinessUnitId = that.BusinessUnitId;

				if (!theAgentState) {
				    theAgentState = agent();
				    theAgentState.fill(data);
				    that.agents.push(theAgentState);
				    return;
				}

				theAgentState.fill(data);
			};

			that.fillAgents = function (data) {
				if (!data || data.length == 0)
					return;
				fillSiteAndTeam(data[0]);
				for (var i = 0; i < data.length; i++) {
					fillData(data[i], true);
				}
				that.agents
                    .sort(function (left, right) { return left.Name == right.Name ? 0 : (left.Name < right.Name ? -1 : 1); });
			};

			that.fillAgentsStates = function (data) {
				for (var i = 0; i < data.length; i++) {
					fillData(data[i], false);
				}
			};

			that.getAgentState = function (id) {
				var agentState = that.agents().filter(function (item) {
					return item.PersonId === id;
				});
				if (agentState.length == 0)
					return null;
				return agentState[0];
			};

			that.updateFromNotification = function (notification) {
				var data = JSON.parse(notification.BinaryData);
				data.Id = notification.DomainId;
				that.fillAgentsStates(data.AgentStates);
			};

			that.SelectAgent = function (agentStateClicked) {
				if (typeof agentStateClicked === "string") {
					agentStateClicked = that.getAgentState(agentStateClicked);
				}

				deselectAllAgentsExcept(agentStateClicked);
				if (that.agentAdherenceEnabled()) {
					fetchHistoricalAdherence(function (data) {
						agentStateClicked.DisplayAdherencePercentage(data.AdherencePercent != undefined || data.LastTimestamp != undefined);
						agentStateClicked.HistoricalAdherence(data.AdherencePercent == undefined ? '-' : data.AdherencePercent + '%');
						agentStateClicked.LastAdherenceUpdate(data.LastTimestamp);
					}, agentStateClicked.PersonId);
				}
				agentStateClicked.Selected(!agentStateClicked.Selected());

			};

			var deselectAllAgentsExcept = function (agentState) {
				var selectedAgentStates = lazy(that.agents())
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