'use strict';
describe('RtaHistoricalController', function() {
    var $interval,
        $httpBackend,
        $state,
        $fakeBackend,
        $controllerBuilder,
        $translate,
        scope;

    var stateParams = {};

    beforeEach(module('wfm.rta'));
	beforeEach(module('wfm.rtaTestShared'));

    beforeEach(function() {
        module(function($provide) {
            $provide.factory('$stateParams', function() {
                stateParams = {};
                return stateParams;
            });
        });
    });

    beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _FakeRtaBackend_, _ControllerBuilder_, _$translate_) {
        $interval = _$interval_;
        $state = _$state_;
        $httpBackend = _$httpBackend_;
        $fakeBackend = _FakeRtaBackend_;
        $controllerBuilder = _ControllerBuilder_;
        $translate = _$translate_;

        scope = $controllerBuilder.setup('RtaHistoricalController');

        $fakeBackend.clear();
    }));

    it('should get agent', function() {
        var id = Math.random() * 1000 + 1
        stateParams.personId = id;
        $fakeBackend.withAgentState({
            PersonId: id,
            AgentName: 'Mikkey Dee',
            Schedules: [],
            OutOfAdherences: []
        });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.personId).toEqual(id);
        expect(vm.agentName).toEqual('Mikkey Dee');
    });

    it('should display schedule', function() {
        stateParams.personId = '1';
        $fakeBackend.withAgentState({
            PersonId: '1',
            AgentName: 'Mikkey Dee',
            Schedules: [{
                Color: 'lightgreen',
                StartTime: '2016-10-10T08:00:00',
                EndTime: '2016-10-10T17:00:00'
            }],
            OutOfAdherences: []
        });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.agentsFullSchedule[0].Color).toEqual('lightgreen');
        expect(vm.agentsFullSchedule[0].StartTime.format('HH:mm:ss')).toEqual('08:00:00');
        expect(vm.agentsFullSchedule[0].EndTime.format('HH:mm:ss')).toEqual('17:00:00');
        expect(vm.agentsFullSchedule[0].Width).toEqual((9 / 11 * 100) + '%');
        expect(vm.agentsFullSchedule[0].Offset).toEqual((1 / 11 * 100) + '%');
    });

    it('should display schedule', function() {
        stateParams.personId = '1';
        $fakeBackend.withAgentState({
            PersonId: '1',
            AgentName: 'Mikkey Dee',
            Schedules: [{
                Color: 'lightgreen',
                StartTime: '2016-10-10T08:00:00',
                EndTime: '2016-10-10T12:00:00'
            }, {
                Color: 'lightgreen',
                StartTime: '2016-10-10T12:00:00',
                EndTime: '2016-10-10T18:00:00'
            }],
            OutOfAdherences: []
        });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.agentsFullSchedule[0].Width).toEqual(4 / 12 * 100 + '%');
        expect(vm.agentsFullSchedule[0].Offset).toEqual(1 / 12 * 100 + '%');
        expect(vm.agentsFullSchedule[1].Width).toEqual(6 / 12 * 100 + '%');
        expect(vm.agentsFullSchedule[1].Offset).toEqual(5 / 12 * 100 + '%');
    });

    it('should display schedule', function() {
        stateParams.personId = '1';
        $fakeBackend.withAgentState({
            PersonId: '1',
            AgentName: 'Mikkey Dee',
            Schedules: [{
                Color: 'lightgreen',
                StartTime: '2016-10-10T08:00:00',
                EndTime: '2016-10-10T12:30:00'
            }, {
                Color: 'lightgreen',
                StartTime: '2016-10-10T12:30:00',
                EndTime: '2016-10-10T18:30:00'
            }],
            OutOfAdherences: []
        });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.agentsFullSchedule[0].Width).toEqual(4.5 / 13 * 100 + '%');
        expect(vm.agentsFullSchedule[0].Offset).toEqual(1 / 13 * 100 + '%');
        expect(vm.agentsFullSchedule[1].Width).toEqual(6 / 13 * 100 + '%');
        expect(vm.agentsFullSchedule[1].Offset).toEqual(5.5 / 13 * 100 + '%');
    });

    it('should display out of adherence', function() {
        stateParams.personId = '1';
        $fakeBackend.withAgentState({
            PersonId: '1',
            AgentName: 'Mikkey Dee',
            Schedules: [{
                StartTime: '2016-10-10T08:00:00',
                EndTime: '2016-10-10T18:00:00'
            }],
            OutOfAdherences: [{
                StartTime: '2016-10-10T08:00:00',
                EndTime: '2016-10-10T08:15:00'
            }]
        });

        var vm = $controllerBuilder.createController().vm;

		expect(vm.outOfAdherences[0].StartTime).toEqual('08:00:00');
		expect(vm.outOfAdherences[0].EndTime).toEqual('08:15:00');
        expect(vm.outOfAdherences[0].Width).toEqual((15 * 60) / (12 * 3600) * 100 + '%');
        expect(vm.outOfAdherences[0].Offset).toEqual(1 / 12 * 100 + '%');
    });

    it('should display out of adherence', function() {
        stateParams.personId = '1';
        $fakeBackend.withAgentState({
            PersonId: '1',
            AgentName: 'Mikkey Dee',
            Schedules: [{
                StartTime: '2016-10-10T08:00:00',
                EndTime: '2016-10-10T18:00:00'
            }],
            OutOfAdherences: [{
                StartTime: '2016-10-10T08:00:00',
                EndTime: '2016-10-10T08:15:00'
            }, {
                StartTime: '2016-10-10T09:15:00',
                EndTime: '2016-10-10T10:00:00'
            }]
        });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.outOfAdherences[0].Width).toEqual((15 * 60) / (12 * 3600) * 100 + '%');
        expect(vm.outOfAdherences[0].Offset).toEqual(1 / 12 * 100 + '%');
        expect(vm.outOfAdherences[1].Width).toEqual((45 * 60) / (12 * 3600) * 100 + '%');
        expect(vm.outOfAdherences[1].Offset).toEqual((15 * 60 + 7200) / (12 * 3600) * 100 + '%');
    });

	it('should display out of adherence start date when started a long time ago', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgentState({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T18:00:00'
			}],
			OutOfAdherences: [{
				StartTime: '2016-10-09T17:00:00'
			}]
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.outOfAdherences[0].StartTime).toEqual('2016-10-09 17:00:00');
	});

    it('should display full timeline', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withAgentState({
                PersonId: '1',
                AgentName: 'Mikkey Dee',
                Schedules: [{
                    StartTime: '2016-10-10T08:00:00',
                    EndTime: '2016-10-10T09:00:00'
                }, {
                    StartTime: '2016-10-10T15:00:00',
                    EndTime: '2016-10-10T17:00:00'
                }],
                OutOfAdherences: []
            })
            .withTimeline({
                StartTime: '2016-10-10T07:00:00',
                EndTime: '2016-10-10T18:00:00'
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.fullTimeline[0].Time.format('HH:mm')).toEqual('08:00');
        expect(vm.fullTimeline[vm.fullTimeline.length - 1].Time.format('HH:mm')).toEqual('17:00');
    });

    it('should handle out of adherence without end time', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                AgentName: 'Mikkey Dee',
                Schedules: [{
                    StartTime: '2016-10-10T08:00:00',
                    EndTime: '2016-10-10T09:00:00'
                }, {
                    StartTime: '2016-10-10T15:00:00',
                    EndTime: '2016-10-10T17:00:00'
                }],
                OutOfAdherences: [{
                    StartTime: '2016-10-10T07:00:00',
                    EndTime: null
                }]
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.outOfAdherences.length).toEqual(1);
        expect(vm.outOfAdherences[0].Offset).toEqual('0%');
        expect(vm.outOfAdherences[0].Width).toEqual(8 / 11 * 100 + '%');
    });

    it('should display current date', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [],
                OutOfAdherences: []
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.date.format('YYYY-MM-DD')).toBe('2016-10-10');
    });

    it('should display diamonds', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [],
                OutOfAdherences: [],
                Changes: [{
                    Time: '2016-10-10T08:00:00',
                    RuleColor: 'green'
                }]
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.diamonds[0].Color).toEqual('green');
    });

    it('should display diamonds at offset', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [],
                OutOfAdherences: [],
                Changes: [{
                    Time: '2016-10-10T08:00:00',
                    Activity: 'phone',
                    Rule: 'In Call',
                    State: 'Ready',
                    Adherence: 'In adherence'
                }]
            })
            .withTimeline({
                StartTime: '2016-10-10T08:00:00',
                EndTime: '2016-10-10T17:00:00'
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.diamonds[0].Offset).toEqual(1 / 11 * 100 + '%')
    });

    it('should display cards with changes', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [{
                    Name: 'phone',
                    Color: 'lightgreen',
                    StartTime: '2016-10-10T08:00:00',
                    EndTime: '2016-10-10T11:00:00'
                }, {
                    Name: 'break',
                    Color: 'red',
                    StartTime: '2016-10-10T11:00:00',
                    EndTime: '2016-10-10T11:10:00'
                }, {
                    Name: 'phone',
                    Color: 'lightgreen',
                    StartTime: '2016-10-10T11:10:00',
                    EndTime: '2016-10-10T14:00:00'
                }],
                Changes: [{
                    Time: '2016-10-10T08:00:00',
                    Activity: 'phone',
                    ActivityColor: 'lightgreen',
                    Rule: 'In Call',
                    RuleColor: 'darkgreen',
                    State: 'Ready',
                    Adherence: 'In adherence',
                    AdherenceColor: 'green'
                }, {
                    Time: '2016-10-10T08:15:00',
                    Activity: 'phone',
                    ActivityColor: 'lightgreen',
                    Rule: 'ACW',
                    RuleColor: 'darkgreen',
                    State: 'Ready',
                    Adherence: 'In adherence',
                    AdherenceColor: 'green'
                }, {
                    Time: '2016-10-10T11:02:00',
                    Activity: 'break',
                    ActivityColor: 'red',
                    Rule: 'Short Break',
                    RuleColor: 'darkred',
                    State: 'Logged off',
                    Adherence: 'In adherence',
                    AdherenceColor: 'green'
                }, {
                    Time: '2016-10-10T11:10:00',
                    Activity: 'phone',
                    ActivityColor: 'lightgreen',
                    Rule: 'In Call',
                    RuleColor: 'darkgreen',
                    State: 'Ready',
                    Adherence: 'In adherence',
                    AdherenceColor: 'green'
                }]
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.cards.length).toEqual(3);
        expect(vm.cards[0].Header).toEqual('phone 08:00 - 11:00');
        expect(vm.cards[0].Color).toEqual('lightgreen');
        expect(vm.cards[1].Header).toEqual('break 11:00 - 11:10');
        expect(vm.cards[1].Color).toEqual('red');
        expect(vm.cards[2].Header).toEqual('phone 11:10 - 14:00');
        expect(vm.cards[2].Color).toEqual('lightgreen');

        expect(vm.cards[0].Items.length).toEqual(2);
        expect(vm.cards[0].Items[0].Time).toEqual('2016-10-10T08:00:00');
        expect(vm.cards[0].Items[0].Activity).toEqual('phone');
        expect(vm.cards[0].Items[0].ActivityColor).toEqual('lightgreen');
        expect(vm.cards[0].Items[0].Rule).toEqual('In Call');
        expect(vm.cards[0].Items[0].RuleColor).toEqual('darkgreen');
        expect(vm.cards[0].Items[0].State).toEqual('Ready');
        expect(vm.cards[0].Items[0].Adherence).toEqual('In adherence');
        expect(vm.cards[0].Items[0].AdherenceColor).toEqual('green');
        expect(vm.cards[0].Items[1].Time).toEqual('2016-10-10T08:15:00');
        expect(vm.cards[0].Items[1].Activity).toEqual('phone');
        expect(vm.cards[0].Items[1].ActivityColor).toEqual('lightgreen');
        expect(vm.cards[0].Items[1].Rule).toEqual('ACW');
        expect(vm.cards[0].Items[1].RuleColor).toEqual('darkgreen');
        expect(vm.cards[0].Items[1].State).toEqual('Ready');
        expect(vm.cards[0].Items[1].Adherence).toEqual('In adherence');
        expect(vm.cards[0].Items[1].AdherenceColor).toEqual('green');

        expect(vm.cards[1].Items.length).toEqual(1);
        expect(vm.cards[1].Items[0].Time).toEqual('2016-10-10T11:02:00');
        expect(vm.cards[1].Items[0].Activity).toEqual('break');
        expect(vm.cards[1].Items[0].ActivityColor).toEqual('red');
        expect(vm.cards[1].Items[0].Rule).toEqual('Short Break');
        expect(vm.cards[1].Items[0].RuleColor).toEqual('darkred');
        expect(vm.cards[1].Items[0].State).toEqual('Logged off');
        expect(vm.cards[1].Items[0].Adherence).toEqual('In adherence');
        expect(vm.cards[1].Items[0].AdherenceColor).toEqual('green');

        expect(vm.cards[2].Items.length).toEqual(1);
        expect(vm.cards[2].Items[0].Time).toEqual('2016-10-10T11:10:00');
        expect(vm.cards[2].Items[0].Activity).toEqual('phone');
        expect(vm.cards[2].Items[0].ActivityColor).toEqual('lightgreen');
        expect(vm.cards[2].Items[0].Rule).toEqual('In Call');
        expect(vm.cards[2].Items[0].RuleColor).toEqual('darkgreen');
        expect(vm.cards[2].Items[0].State).toEqual('Ready');
        expect(vm.cards[2].Items[0].Adherence).toEqual('In adherence');
        expect(vm.cards[2].Items[0].AdherenceColor).toEqual('green');
    });

    it('should display changes before shift start in its own card', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [{
                    Name: 'phone',
                    Color: 'lightgreen',
                    StartTime: '2016-10-10T08:00:00',
                    EndTime: '2016-10-10T11:00:00'
                }],
                Changes: [{
                    Time: '2016-10-10T07:54:00',
                    Activity: null,
                    ActivityColor: null,
                    Rule: 'Positive',
                    RuleColor: 'orange',
                    State: 'Ready',
                    Adherence: 'Out adherence',
                    AdherenceColor: 'red'
                }]
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.cards[0].Header).toEqual('BeforeShiftStart');
        expect(vm.cards[0].Color).toEqual('black');
        expect(vm.cards[0].Items[0].Time).toEqual('2016-10-10T07:54:00');
        expect(vm.cards[0].Items[0].Activity).toEqual(null);
        expect(vm.cards[0].Items[0].ActivityColor).toEqual(null);
        expect(vm.cards[0].Items[0].Rule).toEqual('Positive');
        expect(vm.cards[0].Items[0].RuleColor).toEqual('orange');
        expect(vm.cards[0].Items[0].State).toEqual('Ready');
        expect(vm.cards[0].Items[0].Adherence).toEqual('Out adherence');
        expect(vm.cards[0].Items[0].AdherenceColor).toEqual('red');
    });

    it('should display changes after shift end in its own card', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [{
                    Name: 'phone',
                    Color: 'lightgreen',
                    StartTime: '2016-10-10T08:00:00',
                    EndTime: '2016-10-10T11:00:00'
                }],
                Changes: [{
                    Time: '2016-10-10T11:54:00',
                    Activity: null,
                    ActivityColor: null,
                    Rule: 'Positive',
                    RuleColor: 'orange',
                    State: 'Ready',
                    Adherence: 'Out adherence',
                    AdherenceColor: 'red'
                }]
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.cards[0].Header).toEqual('AfterShiftEnd');
        expect(vm.cards[0].Color).toEqual('black');
        expect(vm.cards[0].Items[0].Time).toEqual('2016-10-10T11:54:00');
        expect(vm.cards[0].Items[0].Activity).toEqual(null);
        expect(vm.cards[0].Items[0].ActivityColor).toEqual(null);
        expect(vm.cards[0].Items[0].Rule).toEqual('Positive');
        expect(vm.cards[0].Items[0].RuleColor).toEqual('orange');
        expect(vm.cards[0].Items[0].State).toEqual('Ready');
        expect(vm.cards[0].Items[0].Adherence).toEqual('Out adherence');
        expect(vm.cards[0].Items[0].AdherenceColor).toEqual('red');
    });

    it('should display closed cards by default', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [{
                    StartTime: '2016-10-10T08:00:00',
                    EndTime: '2016-10-10T11:00:00'
                }],
                Changes: [{
                    Time: '2016-10-10T08:00:00',
                }]
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.cards[0].isOpen).toEqual(false);
    });

    it('should highlight diamond and card item on click', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [{
                    StartTime: '2016-10-10T08:00:00',
                    EndTime: '2016-10-10T11:00:00'
                }],
                Changes: [{
                    Time: '2016-10-10T08:00:00'
                }]
            });

        var vm = $controllerBuilder.createController().vm;

        vm.diamonds[0].click();

        expect(vm.diamonds[0].highlight).toEqual(true);
        expect(vm.cards[0].isOpen).toEqual(true);
        expect(vm.cards[0].Items[0].highlight).toEqual(true);
    });

    it('should remove highlight from other diamond and card item on click', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [{
                        StartTime: '2016-10-10T08:00:00',
                        EndTime: '2016-10-10T09:00:00'
                    },
                    {
                        StartTime: '2016-10-10T09:00:00',
                        EndTime: '2016-10-10T10:00:00'
                    }
                ],
                Changes: [{
                        Time: '2016-10-10T08:00:00'
                    },
                    {
                        Time: '2016-10-10T09:00:00'
                    }
                ]
            });

        var vm = $controllerBuilder.createController().vm;

        vm.diamonds[0].click();
        vm.diamonds[1].click();

        expect(vm.diamonds[0].highlight).toEqual(false);
        expect(vm.cards[0].isOpen).toEqual(true);
        expect(vm.cards[0].Items[0].highlight).toEqual(false);
        expect(vm.diamonds[1].highlight).toEqual(true);
        expect(vm.cards[1].isOpen).toEqual(true);
        expect(vm.cards[1].Items[0].highlight).toEqual(true);
    });

    it('should highlight diamond and card item on click', function() {
        stateParams.personId = '1';
        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1',
                Schedules: [{
                    StartTime: '2016-10-10T08:00:00',
                    EndTime: '2016-10-10T11:00:00'
                }],
                Changes: [{
                    Time: '2016-10-10T08:00:00'
                }]
            });

        var vm = $controllerBuilder.createController().vm;

        vm.cards[0].Items[0].click();

        expect(vm.diamonds[0].highlight).toEqual(true);
        expect(vm.cards[0].Items[0].highlight).toEqual(true);
    });

    it('should generate timeline from server times', function() {
        stateParams.personId = '1';

        $fakeBackend
            .withTime('2016-10-10T15:00:00')
            .withAgentState({
                PersonId: '1'
            })
            .withTimeline({
                StartTime: '2016-10-10T00:00:00',
                EndTime: '2016-10-11T00:00:00'
            });

        var vm = $controllerBuilder.createController().vm;

        expect(vm.fullTimeline[0].Time.format('HH:mm')).toBe('01:00');
        expect(vm.fullTimeline[22].Time.format('HH:mm')).toBe('23:00');
    });
});