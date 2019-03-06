'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
    it('should display neutral adherences', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-26T07:00:00',
                EndTime: '2019-02-26T19:00:00'
            },
            NeutralAdherences: [{
                StartTime: '2019-02-26T08:00:00',
                EndTime: '2019-02-26T09:00:00'
            }]
        });

        var vm = t.createController();

        expect(vm.neutralAdherences[0].StartTime).toEqual(moment('2019-02-26T08:00:00').format('LTS'));
        expect(vm.neutralAdherences[0].EndTime).toEqual(moment('2019-02-26T09:00:00').format('LTS'));
    });

    it('should display recorded neutral adherences', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-26T07:00:00',
                EndTime: '2019-02-26T19:00:00'
            },
            RecordedNeutralAdherences: [{
                StartTime: '2019-02-26T08:00:00',
                EndTime: '2019-02-26T09:00:00'
            }]
        });

        var vm = t.createController();

        expect(vm.recordedNeutralAdherences[0].StartTime).toEqual(moment('2019-02-26T08:00:00').format('LTS'));
        expect(vm.recordedNeutralAdherences[0].EndTime).toEqual(moment('2019-02-26T09:00:00').format('LTS'));
    });

    it('should display recorded adherences', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-26T07:00:00',
                EndTime: '2019-02-26T19:00:00'
            },
            RecordedOutOfAdherences: [{
                StartTime: '2019-02-26T08:00:00',
                EndTime: '2019-02-26T09:00:00'
            }],
            RecordedNeutralAdherences: [{
                StartTime: '2019-02-26T10:00:00',
                EndTime: '2019-02-26T11:00:00'
            }]
        });

        var vm = t.createController();

        expect(vm.recordedAdherences[0].StartTime).toEqual(moment('2019-02-26T08:00:00').format('LTS'));
        expect(vm.recordedAdherences[0].EndTime).toEqual(moment('2019-02-26T09:00:00').format('LTS'));
        expect(vm.recordedAdherences[1].StartTime).toEqual(moment('2019-02-26T10:00:00').format('LTS'));
        expect(vm.recordedAdherences[1].EndTime).toEqual(moment('2019-02-26T11:00:00').format('LTS'));
    });


    it('should display recorded adherences ordered by start time', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-26T07:00:00',
                EndTime: '2019-02-26T19:00:00'
            },
            RecordedOutOfAdherences: [{
                StartTime: '2019-02-26T10:00:00',
                EndTime: '2019-02-26T11:00:00'
            }],
            RecordedNeutralAdherences: [{
                StartTime: '2019-02-26T08:00:00',
                EndTime: '2019-02-26T09:00:00'
            }]
        });

        var vm = t.createController();

        expect(vm.recordedAdherences[0].StartTime).toEqual(moment('2019-02-26T08:00:00').format('LTS'));
        expect(vm.recordedAdherences[1].StartTime).toEqual(moment('2019-02-26T10:00:00').format('LTS'));
    });
    
    it('should display recorded adherences with type', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-26T07:00:00',
                EndTime: '2019-02-26T19:00:00'
            },
            RecordedOutOfAdherences: [{
                StartTime: '2019-02-26T08:00:00',
                EndTime: '2019-02-26T09:00:00'
            }],
            RecordedNeutralAdherences: [{
                StartTime: '2019-02-26T10:00:00',
                EndTime: '2019-02-26T11:00:00'
            }]
        });

        var vm = t.createController();

        expect(vm.recordedAdherences[0].Type).toEqual("Out");
        expect(vm.recordedAdherences[1].Type).toEqual("Neutral");
    });

    it('should highlight recorded adherence on click', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-26T07:00:00',
                EndTime: '2019-02-26T19:00:00'
            },
            RecordedOutOfAdherences: [{
                StartTime: '2019-02-26T08:00:00',
                EndTime: '2019-02-26T09:00:00'
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.recordedAdherences[0].click();
        });

        expect(vm.recordedAdherences[0].highlight).toBe(true);
    });

    it('should highlight recorded out of adherence on clicking recorded adherence', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-26T07:00:00',
                EndTime: '2019-02-26T19:00:00'
            },
            RecordedOutOfAdherences: [{
                StartTime: '2019-02-26T08:00:00',
                EndTime: '2019-02-26T09:00:00'
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.recordedAdherences[0].click();
        });

        expect(vm.recordedOutOfAdherences[0].highlight).toBe(true);
    });

    it('should highlight recorded adherence on clicking recorded out of adherence', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-26T07:00:00',
                EndTime: '2019-02-26T19:00:00'
            },
            RecordedOutOfAdherences: [{
                StartTime: '2019-02-26T08:00:00',
                EndTime: '2019-02-26T09:00:00'
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.recordedOutOfAdherences[0].click();
        });

        expect(vm.recordedAdherences[0].highlight).toBe(true);
    });

    it('should highlight recorded neutral adherence on clicking recorded adherence', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-26T07:00:00',
                EndTime: '2019-02-26T19:00:00'
            },
            RecordedNeutralAdherences: [{
                StartTime: '2019-02-26T08:00:00',
                EndTime: '2019-02-26T09:00:00'
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.recordedAdherences[0].click();
        });

        expect(vm.recordedNeutralAdherences[0].highlight).toBe(true);
    });
});