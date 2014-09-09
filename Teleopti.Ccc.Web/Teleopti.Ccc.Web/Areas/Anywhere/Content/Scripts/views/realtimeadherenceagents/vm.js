define([
		'knockout',
		'views/realtimeadherenceagents/agentstate',
		'views/realtimeadherenceagents/agent',
		'views/realtimeadherenceagents/filter',
		'subscriptions.personschedule',
		'helpers',
		'shared/timeline',
		'resources',
		'lazy',
		'views/teamschedule/person',
		'resizeevent'
],
	function (ko,
		agentstate,
		agent,
		filterService,
		personsubscriptions,
		helpers,
		timeLineViewModel,
		resources,
		lazy,
		personViewModel,
		resize) {
	return function () {

		var that = {};
		that.Resources = resources;
		that.permissionAddFullDayAbsence = ko.observable();
		that.permissionAddIntradayAbsence = ko.observable();
		that.permissionRemoveAbsence = ko.observable();
		that.permissionAddActivity = ko.observable();
		that.permissionMoveActivity = ko.observable();

		that.agents = []; 
		that.agentStates = ko.observableArray();
		that.resources = resources;
		that.teamName = ko.observable();
		that.siteName = ko.observable();
		that.siteId = ko.observable();
		that.siteURI = ko.observable();
		that.filter = ko.observable();
		
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
			that.siteURI('#realtimeadherenceteams/' + that.siteId());
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

		that.SelectAgent = function (agentStateClicked) {
			if (agentStateClicked.Selected())
				that.deselectAll();
			else
				agentStateClicked.Selected(true);
			that.unsubscribePersonSchedule();
			if (agentStateClicked.Selected())
				that.subscribePersonSchedule(agentStateClicked.PersonId);
		}

		that.getSelectedAgentState = function() {
			var selectedAgentState = that.agentStates().filter(function (obj) {
				return obj.Selected() === true;
			});
			return selectedAgentState;
		}

		that.selectedPersonId = function() {
			var selectedAgentState = that.agentStates().filter(function (obj) {
				return obj.Selected() === true;
			});
			if (selectedAgentState)
				return selectedAgentState[0].PersonId;
			return undefined;
		}

		that.deselectAll = function() {
			that.agentStates().forEach(function (agentState) {
				agentState.Selected(false);
			});
		}

		that.changeSchedule = ko.observable(false);
		that.Loading = ko.observable(true);

		that.Persons = ko.observableArray();
		that.SortedPersons = ko.computed(function () {
			return that.Persons().sort(function (first, second) {
				first = first.OrderBy();
				second = second.OrderBy();
				return first == second ? 0 : (first < second ? -1 : 1);
			});
		});

		var layers = function () {
			return lazy(that.Persons())
				.map(function (x) { return x.Shifts(); })
				.flatten()
				.map(function (x) { return x.Layers(); })
				.flatten();
		};

		that.TimeLine = new timeLineViewModel(ko.computed(function () { return layers().toArray(); }));
		
		var today = moment();
		resize.onresize(function () {
			that.TimeLine.WidthPixels($('.time-line-for').width());
		});

		that.subscribePersonSchedule = function (personId) {
			personsubscriptions.subscribePersonSchedule(
				personId,
				helpers.Date.ToServer(today),
				function (data) {
					updateSchedule(data);
					that.Loading(false);
					resize.notify();
				}
			);
		}

		that.unsubscribePersonSchedule = function () {
			personsubscriptions.unsubscribePersonSchedule();
		}

		var personForId = function (id, personArray) {
			if (!id)
				return undefined;
			var personvm = lazy(personArray)
				.filter(function (x) { return x.Id == id; })
				.first();
			if (!personvm) {
				personvm = new personViewModel({ Id: id });
				personArray.push(personvm);
			}
			return personvm;
		};

		var updateSchedule = function(data) {
			that.Persons([]);
			var personArray = [];

			var schedule = data;
			schedule.Offset = today;
			schedule.Date = moment(schedule.Date, resources.FixedDateFormatForMoment);
			var personvm = personForId(that.selectedPersonId(), personArray);
			personvm.AddData(schedule, that.TimeLine);

			that.Persons.push.apply(that.Persons, personArray);
		};

		that.SelectPerson = function (person) {
			deselectAllPersonsExcept(person);
			deselectAllLayersExcept();

			person.Selected(!person.Selected());
		};

		that.SelectLayer = function (layer) {
			deselectAllPersonsExcept();
			deselectAllLayersExcept(layer);

			layer.Selected(!layer.Selected());
		};

		var deselectAllPersonsExcept = function (person) {
			var selectedPersons = lazy(that.Persons())
				.filter(function (x) {
					if (person && x === person)
						return false;
					return x.Selected();
				});
			selectedPersons.each(function (x) {
				x.Selected(false);
			});
		};

		var deselectAllLayersExcept = function (layer) {
			var selectedLayers = layers()
				   .filter(function (x) {
				   	if (layer && x === layer) {
				   		return false;
				   	}
				   	return x.Selected();
				   });
			selectedLayers.each(function (x) {
				x.Selected(false);
			});
		};

		return that;
	};
}
);