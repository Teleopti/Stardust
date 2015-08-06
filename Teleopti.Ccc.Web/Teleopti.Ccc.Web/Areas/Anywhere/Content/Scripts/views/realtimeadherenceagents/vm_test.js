﻿define([
    'buster',
    'views/realtimeadherenceagents/vm',
    'window',
    'resources',
    'lazy'
], function(
    buster,
    viewModel,
    window,
    resources,
    lazy) {

    return function() {

        var agentById = function(vm, id) {
            return lazy(vm.filteredAgents())
                .find(function(a) { return a.PersonId === id; });
        }

        buster.testCase("=>real time adherence agents viewmodel", {
            "should have no agents if none filled": function() {
                var vm = viewModel();

                assert.equals(vm.filteredAgents(), []);
            },

            "should update root uri": function() {
                var vm = viewModel();

                vm.SetViewOptions({
                    buid: 'guid'
                });

                assert.equals(vm.rootURI(), '#realtimeadherencesites/guid');
            },

            "should fill agents info": function() {
                var vm = viewModel();

                vm.fillAgents([
                    {
                        PersonId: "guid",
                        Name: "Bill",
                        SiteId: "gui1",
                        SiteName: "site",
                        TeamId: "guid2",
                        TeamName: "team"
                    }
                ]);

                assert.equals(vm.filteredAgents()[0].PersonId, "guid");
                assert.equals(vm.filteredAgents()[0].Name, "Bill");
                assert.equals(vm.filteredAgents()[0].SiteId, "gui1");
                assert.equals(vm.filteredAgents()[0].SiteName, "site");
                assert.equals(vm.filteredAgents()[0].TeamId, "guid2");
                assert.equals(vm.filteredAgents()[0].TeamName, "team");
            },

            "should fill agent state data": function() {
                resources.TimeZoneOffsetMinutes = -600;
                var vm = viewModel();

                vm.fillAgents([
                    {
                        PersonId: "guid1",
                        Name: "Bill",
                        SiteId: "siteid",
                        SiteName: "site",
                        TeamId: "teamid",
                        TeamName: "team",
                    },
                    {
                        PersonId: "guid2",
                        Name: "John",
                        SiteId: "siteid2",
                        SiteName: "site2",
                        TeamId: "teamid2",
                        TeamName: "team2",
                    }
                ]);
                vm.fillAgentsStates([
                    {
                        PersonId: 'guid1',
                        State: 'Ready',
                        StateStart: moment('2014-01-21 12:00').format(),
                        Activity: 'Phone',
                        NextActivity: 'Lunch',
                        NextActivityStartTime: moment('2014-01-21 13:00').format(),
                        Alarm: 'Adhering',
                        AlarmColor: '#000001',
                        AlarmStart: moment('2014-01-21 12:15').format()
                    },
                    {
                        PersonId: 'guid2',
                        State: 'Pause',
                        StateStart: moment('2014-01-21 12:00').format(),
                        Activity: 'Lunch',
                        NextActivity: 'Phone',
                        NextActivityStartTime: moment('2014-01-21 13:00').format(),
                        Alarm: 'Not Adhering',
                        AlarmColor: '#ffffff',
                        AlarmStart: moment('2014-01-21 12:15').format()
                    }
                ]);

                var agent = agentById(vm, "guid1");
                assert.equals(agent.PersonId, "guid1");
                assert.equals(agent.State(), "Ready");
                assert.equals(agent.EnteredCurrentAlarm(), moment('2014-01-21 12:00').format());
                assert.equals(agent.Activity(), "Phone");
                assert.equals(agent.NextActivity(), "Lunch");
                assert.equals(agent.AlarmStart(), moment.utc(moment('2014-01-21 12:15').format()).add(-600, 'minutes').format());
                assert.equals(agent.Alarm(), "Adhering");
                assert.equals(agent.AlarmColor(), 'rgba(0,0,1, 0.6)');

                agent = agentById(vm, "guid2");
                assert.equals(agent.PersonId, "guid2");
                assert.equals(agent.State(), "Pause");
                assert.equals(agent.EnteredCurrentAlarm(), moment('2014-01-21 12:00').format());
                assert.equals(agent.Activity(), "Lunch");
                assert.equals(agent.NextActivity(), "Phone");
                assert.equals(agent.AlarmStart(), moment.utc(moment('2014-01-21 12:15').format()).add(-600, 'minutes').format());
                assert.equals(agent.Alarm(), "Not Adhering");
                assert.equals(agent.AlarmColor(), 'rgba(255,255,255, 0.6)');

            },

            "should not fill state for unknown agents": function() {
                var vm = viewModel();

                vm.fillAgentsStates([
                    {
                        PersonId: 'guid1'
                    }
                ]);
                
                assert.equals(vm.filteredAgents().length, 0);
            },

            "should fill next activity start for last activity": function() {
                resources.TimeZoneOffsetMinutes = -600;
                var vm = viewModel();

                vm.fillAgents([
                    {
                        PersonId: "guid1"
                    }
                ]);
                vm.fillAgentsStates([
                    {
                        PersonId: 'guid1',
                        NextActivityStartTime: moment('2014-01-21 13:00').format(),
                    }
                ]);

                var agent = agentById(vm, "guid1");
                var actual = moment.utc(moment('2014-01-21 13:00').format()).add(-600, 'minutes').format(resources.TimeFormatForMoment);
                assert.equals(agent.NextActivityStartTime(), actual);
            },

            "should update existing state data when filling state": function() {
                var vm = viewModel();

                vm.fillAgents([{ PersonId: "guid1" }]);
                vm.fillAgentsStates([{ PersonId: "guid1" }]);

                assert.equals(vm.filteredAgents().length, 1);
            },

            "should order by agent name": function() {
                var vm = viewModel();

                vm.fillAgents([
                    {
                        PersonId: "guid1",
                        Name: "John"
                    }, {
                        PersonId: "guid2",
                        Name: "Bill"
                    }
                ]);

                assert.equals(vm.filteredAgents()[0].Name, "Bill");
                assert.equals(vm.filteredAgents()[1].Name, "John");
            },

            "should display alarmtime based on when agent entered current alarm": function() {
                var vm = viewModel();

                vm.fillAgents([
                    {
                        PersonId: "guid1"
                    }
                ]);
                var now = moment();
                vm.fillAgentsStates([
                    {
                        PersonId: 'guid1',
                        StateStart: now.add(-10, 'seconds'),
                        AlarmStart: now
                    }
                ]);

                assert.equals(vm.filteredAgents()[0].AlarmTime(), "0:00:10");
            },

            "should not display alarm until alarm start": function() {
                resources.TimeZoneOffsetMinutes = 0;
                var vm = viewModel();

                vm.fillAgents([
                    {
                        PersonId: "guid1"
                    }
                ]);
                var now = moment();
                vm.fillAgentsStates([
                    {
                        PersonId: 'guid1',
                        Alarm: 'Adhering',
                        StateStart: now,
                        AlarmStart: now.add(10, 'seconds')
                    }
                ]);

                assert.equals(vm.filteredAgents()[0].Alarm(), undefined);
            },

            "should select an agent": function() {
                var vm = viewModel();

                vm.fillAgents([{ PersonId: "guid1" }]);

                var agent = agentById(vm, "guid1");
                vm.SelectAgent(agent);
                assert.equals(agent.Selected(), true);
            },

            "should generate change schedule url for an agent": function() {
                var vm = viewModel();

                vm.SetViewOptions({
                    buid: 'buId'
                });
                vm.fillAgents([
                    {
                        PersonId: "personId",
                        TeamId: "teamId"
                    }
                ]);

                var expectedUrl = "#teamschedule/buId/teamId/personId/" + moment().format("YYYYMMDD");
                assert.match(vm.filteredAgents()[0].ScheduleChangeUrl, expectedUrl);
            },


            "filtering:": {
                " should only display matching name": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }
                    ]);

                    vm.filter("Kurt");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt");
                },

                "should only display matching activity": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }
                    ]);
                    vm.fillAgentsStates([
                        {
                            PersonId: "guid1",
                            Activity: "Phone"
                        }, {
                            PersonId: "guid2",
                            Activity: "Lunch"
                        }
                    ]);

                    vm.filter("Phone");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt");
                },

                "should only display matching alarm": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }
                    ]);
                    vm.fillAgentsStates([
                        {
                            PersonId: "guid1",
                            Alarm: "Positive"
                        }, {
                            PersonId: "guid2",
                            Alarm: "Negative"
                        }
                    ]);

                    vm.filter("Positive");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt");
                },


                "should only display agents matching next activity start time": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }
                    ]);
                    vm.fillAgentsStates([
                        {
                            PersonId: "guid1",
                            NextActivityStartTime: moment.utc('2014-01-21 13:45').format()
                        }, {
                            PersonId: "guid2",
                            NextActivityStartTime: moment.utc('2014-01-21 13:00').format()
                        }
                    ]);

                    vm.filter('13:45');

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt");
                },

                "should only display agent matching multi filter criteria": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt",
                            TeamName: "ATeam"
                        }, {
                            PersonId: "guid2",
                            Name: "Kurt",
                            TeamName: "BTeam"
                        }
                    ]);

                    vm.filter("Kurt BTeam");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt");
                    assert.equals(vm.filteredAgents()[0].TeamName, "BTeam");
                },

                "should only display agent matching single quote filter criteria": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt Karlsson"
                        }, {
                            PersonId: "guid2",
                            Name: "Kurt A Karlsson"
                        }
                    ]);

                    vm.filter("'Kurt Karlsson'");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt Karlsson");
                },

                "should only display agent matching double quote filter criteria": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt Karlsson"
                        }, {
                            PersonId: "guid2",
                            Name: "Kurt A Karlsson"
                        }
                    ]);

                    vm.filter('"Kurt Karlsson"');

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt Karlsson");
                },

                "should only display agent exactly matching quote filter criteria": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt Karlsson"
                        }, {
                            PersonId: "guid2",
                            Name: "Karl Kurt Karlsson"
                        }
                    ]);

                    vm.filter('"Kurt Karlsson"');

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt Karlsson");
                },

                "should only display agent not matching when negating filter word with multiple words": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt Karlsson",
                            TeamName: "ATeam"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen",
                            TeamName: "ATeam"
                        }, {
                            PersonId: "guid3",
                            Name: "Kurt Olsson",
                            TeamName: "BTeam"
                        }
                    ]);

                    vm.filter("Kurt !BTeam");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt Karlsson");
                },

                "should only display agents thats not matching negating quoted searchwords": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt Karlsson"
                        }, {
                            PersonId: "guid2",
                            Name: "Kurt A Karlsson"
                        }
                    ]);

                    vm.filter("!'Kurt Karlsson'");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Kurt A Karlsson");
                },

                "should display all matching agents when using OR between searchwords": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }, {
                            PersonId: "guid3",
                            Name: "Arne"
                        }
                    ]);

                    vm.filter("Arne OR Glen");

                    assert.equals(vm.filteredAgents().length, 2);
                    assert.equals(vm.filteredAgents()[0].Name, "Arne");
                    assert.equals(vm.filteredAgents()[1].Name, "Glen");
                },

                "should display all matching agents when using OR and match other words normally": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt",
                            TeamName: "Team A"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen",
                            TeamName: "Team B"
                        }, {
                            PersonId: "guid2",
                            Name: "Arne",
                            TeamName: "Team B"
                        }
                    ]);

                    vm.filter("Kurt or Glen 'Team B'");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "Glen");
                },

                "should display all agents not matching negated OR": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }, {
                            PersonId: "guid3",
                            Name: "Arne"
                        }
                    ]);

                    vm.filter("Kurt OR !Arne");

                    assert.equals(vm.filteredAgents().length, 2);
                    assert.equals(vm.filteredAgents()[0].Name, "Glen");
                    assert.equals(vm.filteredAgents()[1].Name, "Kurt");
                },

                "should display all agents not matching negated with dash negation": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }, {
                            PersonId: "guid3",
                            Name: "Arne"
                        }
                    ]);

                    vm.filter("-Arne");

                    assert.equals(vm.filteredAgents().length, 2);
                    assert.equals(vm.filteredAgents()[0].Name, "Glen");
                    assert.equals(vm.filteredAgents()[1].Name, "Kurt");
                },

                "should display agents matching more than two OR searchwords": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }, {
                            PersonId: "guid3",
                            Name: "Arne"
                        }, {
                            PersonId: "guid4",
                            Name: "Kalle"
                        }
                    ]);

                    vm.filter("Kurt or Glen or Arne");

                    assert.equals(vm.filteredAgents().length, 3);
                    assert.equals(vm.filteredAgents()[0].Name, "Arne");
                    assert.equals(vm.filteredAgents()[1].Name, "Glen");
                    assert.equals(vm.filteredAgents()[2].Name, "Kurt");
                },

                "should display agents matching symbols": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "I>Like,Symbols$"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }
                    ]);

                    vm.filter(",");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].PersonId, "guid1");

                    vm.filter(">");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].PersonId, "guid1");

                    vm.filter("$");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].PersonId, "guid1");
                },

                "should display agents matching arabic name like 'اختبار' searchwords": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "اختبار"
                        }, {
                            PersonId: "guid2",
                            Name: "Glen"
                        }
                    ]);

                    vm.filter("ار");

                    assert.equals(vm.filteredAgents().length, 1);
                    assert.equals(vm.filteredAgents()[0].Name, "اختبار");
                },

                "should matching multiple search words, #30352": function() {
                    var vm = viewModel();
                    vm.fillAgents([
                        {
                            PersonId: "guid1",
                            Name: "Kurt Karlsson"
                        }, {
                            PersonId: "guid2",
                            Name: "Kurt Olsson"
                        }
                    ]);

                    vm.filter("Kurt Andersson");

                    assert.equals(vm.filteredAgents().length, 0);
                }
            },

            "historical adherence: ": {
                "should fetch the historical adherence for selected person": function() {
                    var vm = viewModel(function(callback, personId) {
                        callback({
                            AdherencePercent: personId === "guid1" ? 12 : 0
                        });
                    });
                    vm.agentAdherenceEnabled(true);

                    vm.fillAgents([{ PersonId: "guid1" }]);

                    vm.SelectAgent("guid1");

                    assert.equals(vm.filteredAgents()[0].HistoricalAdherence(), '12%');
                },

                "should only fetch historical data if agentAdherence is enabled": function() {
                    var hasFetchedHistoricalAdherence = false;
                    var vm = viewModel(function() {
                        hasFetchedHistoricalAdherence = true;
                    });
                    vm.agentAdherenceEnabled(false);
                    vm.fillAgents([{ PersonId: "guid1" }]);

                    vm.SelectAgent("guid1");

                    assert.isFalse(hasFetchedHistoricalAdherence);
                },

                "should update time since last update": function() {
                    var vm = viewModel(function(callback) {
                        callback({
                            LastTimestamp: "0:10:00"
                        });
                    });
                    vm.agentAdherenceEnabled(true);
                    vm.fillAgents([{ PersonId: "guid1" }]);

                    vm.SelectAgent("guid1");

                    assert.equals(vm.filteredAgents()[0].LastAdherenceUpdate(), "0:10:00");
                },

                "should hide adherence percentage if no data": function() {
                    var vm = viewModel(function(callback) {
                        callback({});
                    });
                    vm.agentAdherenceEnabled(true);
                    vm.fillAgents([{ PersonId: "guid1" }]);

                    vm.SelectAgent("guid1");

                    refute(vm.filteredAgents()[0].DisplayAdherencePercentage());
                },

                "should display adherence percentage if data": function() {
                    var vm = viewModel(function(callback) {
                        callback({
                            AdherencePercent: 12
                        });
                    });
                    vm.agentAdherenceEnabled(true);
                    vm.fillAgents([{ PersonId: "guid1" }]);

                    vm.SelectAgent("guid1");

                    assert(vm.filteredAgents()[0].DisplayAdherencePercentage());
                },

                "should keep the historical adherence when new state is pushed": function() {
                    var vm = viewModel(function(callback) {
                        callback({
                            AdherencePercent: 21
                        });
                    });
                    vm.agentAdherenceEnabled(true);

                    vm.fillAgents([{ PersonId: "guid1" }]);
                    vm.fillAgentsStates([{ PersonId: "guid1" }]);

                    vm.SelectAgent("guid1");
                    vm.fillAgentsStates([{ PersonId: "guid1" }]);

                    assert.equals(vm.filteredAgents()[0].HistoricalAdherence(), '21%');
                }

            }
        });
    };
});