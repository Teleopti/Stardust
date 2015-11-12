'use strict';
describe('RtaGridService', function() {
	var target, $state;

	beforeEach(module('wfm.rta'));
	beforeEach(inject(function(_$state_, RtaGridService) {
		target = RtaGridService;
		$state = _$state_;
	}));

	it('should select an agent', function() {
		var personId = '11610fe4-0130-4568-97de-9b5e015b2564';

  target.selectAgent(personId);

		expect(target.isSelected(personId)).toEqual(true);
	});

	it('should unselect an agent', function() {
		var personId = '11610fe4-0130-4568-97de-9b5e015b2564';

  target.selectAgent(personId);
  target.selectAgent(personId);

		expect(target.isSelected(personId)).toEqual(false);
	});
});
