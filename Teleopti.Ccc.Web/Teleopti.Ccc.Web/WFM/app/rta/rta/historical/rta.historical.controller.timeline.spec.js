'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {

	it('should display timeline', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.with.historicalAdherence({
				PersonId: '1',
				AgentName: 'Mikkey Dee',
				Timeline: {
					StartTime: '2016-10-10T10:00:00',
					EndTime: '2016-10-10T20:00:00'
				}
			});

		var vm = t.createController();

		var first = 0;
		var last = vm.fullTimeline.length - 1;
		expect(vm.fullTimeline[first].Time.format('HH:mm')).toEqual('11:00');
		expect(vm.fullTimeline[last].Time.format('HH:mm')).toEqual('19:00');
		expect(vm.fullTimeline[first].Offset).toEqual('10%');
		expect(vm.fullTimeline[last].Offset).toEqual('90%');
	});

	it('should display timeline from... timeline', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.with.historicalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Timeline: {
					StartTime: '2016-10-10T00:00:00',
					EndTime: '2016-10-11T00:00:00'
				}
			});

		var vm = t.createController();

		var first = 0;
		var last = vm.fullTimeline.length - 1;
		var space = 100 / 24; // 4.166
		expect(vm.fullTimeline[first].Time.format('HH:mm')).toBe('01:00');
		expect(vm.fullTimeline[last].Time.format('HH:mm')).toBe('23:00');
		expect(vm.fullTimeline[first].Offset.substr(0, 5)).toEqual((100 / 24).toString().substr(0, 5));
		expect(vm.fullTimeline[last].Offset.substr(0, 5)).toEqual((100 / 24 * 23).toString().substr(0, 5));
	});

  fit('should display timeline labels for whole hours', function (t) {
    t.stateParams.personId = '1';
    t.backend
      .with.historicalAdherence({
      PersonId: '1',
      AgentName: 'Mikkey Dee',
      Timeline: {
        StartTime: '2016-10-10T10:30:00',
        EndTime: '2016-10-10T20:30:00'
      }
    });

    var vm = t.createController();

    var first = 0;
    var last = vm.fullTimeline.length - 1;
    expect(vm.fullTimeline[first].Time.format('HH:mm')).toEqual('11:00');
    expect(vm.fullTimeline[last].Time.format('HH:mm')).toEqual('20:00');
    expect(vm.fullTimeline[first].Offset).toEqual('5%');
    expect(vm.fullTimeline[last].Offset).toEqual('95%');
  });

});
