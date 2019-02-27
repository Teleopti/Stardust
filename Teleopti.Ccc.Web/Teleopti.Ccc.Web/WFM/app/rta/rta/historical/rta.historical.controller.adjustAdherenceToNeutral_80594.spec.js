'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
    it('should not display adjusted to neutral adherences', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-25T07:00:00',
                EndTime: '2019-02-25T19:00:00'
            }
        });

        var vm = t.createController();

        expect(vm.showAdjustedToNeutralAdherences).toBe(false);
    });

    it('should display adjusted to neutral adherences', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-25T07:00:00',
                EndTime: '2019-02-25T19:00:00'
            },
            AdjustedToNeutralAdherences: [{
                StartTime: '2019-02-25T08:00:00',
                EndTime: '2019-02-25T09:00:00'
            }]
        });

        var vm = t.createController();

        expect(vm.showAdjustedToNeutralAdherences).toBe(true);
    });

    it('should have history with adjusted to neutral adherences', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-25T07:00:00',
                EndTime: '2019-02-25T19:00:00'
            },
            AdjustedToNeutralAdherences: [{
                StartTime: '2019-02-25T08:00:00',
                EndTime: '2019-02-25T09:00:00'
            }]
        });

        var vm = t.createController();

        expect(vm.adjustedToNeutralAdherences[0].StartTime).toEqual(moment('2019-02-25T08:00:00').format('LTS'));
        expect(vm.adjustedToNeutralAdherences[0].EndTime).toEqual(moment('2019-02-25T09:00:00').format('LTS'));
    });

    it('should have history with multiple adjusted to neutral adherences', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-25T07:00:00',
                EndTime: '2019-02-25T19:00:00'
            },
            AdjustedToNeutralAdherences: [{
                StartTime: '2019-02-25T09:00:00',
                EndTime: '2019-02-25T10:00:00'
            }]
        });

        var vm = t.createController();

        expect(vm.adjustedToNeutralAdherences[0].StartTime).toEqual(moment('2019-02-25T09:00:00').format('LTS'));
        expect(vm.adjustedToNeutralAdherences[0].EndTime).toEqual(moment('2019-02-25T10:00:00').format('LTS'));
    });

    it('should not have history with adjusted to neutral adherences', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-25T07:00:00',
                EndTime: '2019-02-25T19:00:00'
            }
        });

        var vm = t.createController();

        expect(vm.adjustedToNeutralAdherences.length).toEqual(0);
    });

    it('should display positioned', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-25T07:00:00',
                EndTime: '2019-02-25T17:00:00'
            },
            AdjustedToNeutralAdherences: [{
                StartTime: '2019-02-25T08:00:00',
                EndTime: '2019-02-25T09:00:00'
            }]
        });

        var vm = t.createController();

        expect(vm.adjustedToNeutralAdherences[0].Offset).toEqual("10%");
        expect(vm.adjustedToNeutralAdherences[0].Width).toEqual("10%");
    });

    it('should not highlight on click', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-25T07:00:00',
                EndTime: '2019-02-25T17:00:00'
            },
            AdjustedToNeutralAdherences: [{
                StartTime: '2019-02-25T08:00:00',
                EndTime: '2019-02-25T09:00:00'
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.adjustedToNeutralAdherences[0].click();
        });

        expect(vm.adjustedToNeutralAdherences[0].highlight).toBe(true);
    });

    it('should remove highlight from other adjusted to neutral adherence when clicked', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-25T07:00:00',
                EndTime: '2019-02-25T17:00:00'
            },
            AdjustedToNeutralAdherences: [
                {
                    StartTime: '2019-02-25T08:00:00',
                    EndTime: '2019-02-25T09:00:00'
                },
                {
                    StartTime: '2019-02-25T09:00:00',
                    EndTime: '2019-02-25T10:00:00'
                }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.adjustedToNeutralAdherences[0].click();
            vm.adjustedToNeutralAdherences[1].click();
        });

        expect(vm.adjustedToNeutralAdherences[0].highlight).toBe(false);
        expect(vm.adjustedToNeutralAdherences[1].highlight).toBe(true);
    });

    it('should open adjusted to neutral adherences card on click', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-25T07:00:00',
                EndTime: '2019-02-25T17:00:00'
            },
            AdjustedToNeutralAdherences: [{
                StartTime: '2019-02-25T08:00:00',
                EndTime: '2019-02-25T09:00:00'
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.adjustedToNeutralAdherences[0].click();
        });

        expect(vm.openAdjustedToNeutralAdherences).toBe(true);
    });

    it('should open approved periods card and form and set times', function (t) {
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-02-27T07:00:00',
                EndTime: '2019-02-27T17:00:00'
            },
            AdjustedToNeutralAdherences: [{
                StartTime: '2019-02-27T08:00:00',
                EndTime: '2019-02-27T09:00:00'
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.adjustedToNeutralAdherences[0].click();
        });

        expect(vm.openApprovedPeriods).toBe(true);
        expect(vm.openApproveForm).toBe(true);
        expect(vm.approveStartTime).toEqual(moment('2019-02-27T08:00:00').toDate());
        expect(vm.approveEndTime).toEqual(moment('2019-02-27T09:00:00').toDate());
    });
});