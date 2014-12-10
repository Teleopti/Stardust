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
		'navigation',
		'views/realtimeadherenceagents/getadherence'
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
		navigation,
		getadherence) {
		return function (historicalAdherenceServercall) {

			var that = {};
			that.Resources = resources;
			that.permissionAddActivity = ko.observable();

			var fetchHistoricalAdherence = historicalAdherenceServercall || getadherence.ServerCall;

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
			that.agentAdherenceEnabled = ko.observable(false);
			that.agentAdherenceDetailsEnabled = ko.observable(false);
			that.notifyViaSMSEnabled = ko.observable(false);
			that.AgentAdherence = ko.observable();

			that.SelectAgentsEnabled = ko.observable(false);

			that.SetViewOptions = function (options) {
				that.BusinessUnitId(options.buid);
				that.rootURI('#realtimeadherencesites/' + that.BusinessUnitId());
			};

			that.filteredAgents = ko.computed(function () {
				var filter = that.filter();
				if (!filter) {
					return that.agentStates();
				} else {
					return ko.utils.arrayFilter(that.agentStates(), function (item) {
						return filterService().match(
						[
							item.Alarm(),
							item.Name(),
							item.State(),
							item.Activity(),
							item.NextActivity(),
							item.NextActivityStartTime(),
							item.TeamName()
						], filter);

					});
				}
			});

			that.AllChecked = ko.computed({
				read: function () {
					var firstUnchecked = ko.utils.arrayFirst(that.filteredAgents(), function (item) {
						return item.SelectedToSendMessage() == false;
					});
					return firstUnchecked == null;
				},
				write: function (value) {
					ko.utils.arrayForEach(that.filteredAgents(), function (item) {
						item.SelectedToSendMessage(value);
					});
				}
			});

			that.SendMessage = function () {
				var selectedAgentIds = lazy(that.filteredAgents())
					.filter(function (x) {
						return x.SelectedToSendMessage();
					}).map(function (e) {
						return e.PersonId();
					}).join(",");
				window.location.href = "Messages?ids=" + selectedAgentIds;
			};

			that.SendMessageEnabled = ko.computed(function() {
				return lazy(that.filteredAgents())
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
				that.siteURI('#realtimeadherenceteams/' + that.BusinessUnitId() + '/' + that.siteId());
			};

			var fillData = function (data) {
				var theAgent = that.getAgent(data.PersonId);
				if (!theAgent) {
					theAgent = agent();
					theAgent.fill(data);
					that.agents.push(theAgent);
				}
				var theAgentState = that.getAgentState(data.PersonId);
				if (!theAgentState) {
					theAgentState = agentstate();
					that.agentStates.push(theAgentState);
				}
				data.Name = theAgent.Name;
				data.TimeZoneOffset = theAgent.TimeZoneOffset;
				data.TeamName = theAgent.TeamName;
				theAgentState.fill(data);
			}

			that.fillAgents = function (data) {
				if (!data || data.length == 0)
					return;
				fillSiteAndTeam(data[0]);
				for (var i = 0; i < data.length; i++) {
					fillData(data[i]);
				}
			};

			that.fillAgentsStates = function (data) {
				for (var i = 0; i < data.length; i++) {
					fillData(data[i]);
				}
				that.agentStates.sort(function (left, right) { return left.Name() == right.Name() ? 0 : (left.Name() < right.Name() ? -1 : 1); });
			};

			that.getAgent = function (id) {
				var agent = that.agents.filter(function (item) {
					return item.PersonId === id;
				});
				if (agent.length == 0)
					return null;
				return agent[0];
			};
			that.getAgentState = function (id) {
				var agentState = that.agentStates().filter(function (item) {
					return item.PersonId() === id;
				});
				if (agentState.length == 0)
					return null;
				return agentState[0];
			};
			that.getSelectedAgentState = function () {
				var selectedAgentState = that.agentStates().filter(function (obj) {
					return obj.Selected() === true;
				});
				return selectedAgentState;
			}

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
				var a = that.getAgent(data.PersonId());
				return navigation.UrlForChangingSchedule(that.BusinessUnitId(), a.TeamId, a.PersonId, moment((new Date).getTime()));
			}

			that.urlForAdherenceDetails = function (data) {
				var a = that.getAgent(data.PersonId());
				return navigation.UrlForAdherenceDetails(that.BusinessUnitId(), a.PersonId);
			}

			that.SelectAgent = function (agentStateClicked) {
				if (typeof agentStateClicked === "string") {
					agentStateClicked = that.getAgentState(agentStateClicked);
				}

				deselectAllAgentsExcept(agentStateClicked);
				if (that.agentAdherenceEnabled()) {
					fetchHistoricalAdherence(function (data) {
						agentStateClicked.DisplayAdherencePercentage(data.AdherencePercent != undefined);
						agentStateClicked.HistoricalAdherence(data.AdherencePercent);
						agentStateClicked.LastAdherenceUpdate(data.LastTimestamp);
					}, agentStateClicked.PersonId());
				}
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