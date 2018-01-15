'use strict';

rtaTester.describe('RtaAgentsController', function (it, fit, xit) {

	it('should notify invalid configuration', function (t) {
		t.backend.with.translation({
			Name: 'LoggedOutStateGroupMissingInConfiguration',
			Value: 'Logged out stuff',
		});
		t.backend.with.configurationValidation(
			{
				Resource: 'LoggedOutStateGroupMissingInConfiguration'
			}
		);
		t.createController();

		expect(t.lastNotice.Warning.includes('Logged out stuff')).toBeTruthy();
		expect(t.lastNotice.Lifetime).toEqual(10000);
		expect(t.lastNotice.DestroyOnStateGo).toEqual(true);
	});

	it('should notify invalid configuration', function (t) {
		t.backend.with.translation({
			Name: 'LoggedOutStateGroupMissingInRtaService',
			Value: 'Some other text',
		});
		t.backend.with.configurationValidation(
			{
				Resource: 'LoggedOutStateGroupMissingInRtaService'
			}
		);
		t.createController();

		expect(t.lastNotice.Warning.includes('Some other text')).toBeTruthy();
	});

	it('should notify with business unit name', function (t) {
		t.backend.with.translation({
			Name: 'LoggedOutStateGroupMissingInConfiguration',
			Value: 'String with {0}',
		});
		t.backend.with.configurationValidation(
			{
				Resource: 'LoggedOutStateGroupMissingInConfiguration',
				Data: ['Business unit']
			}
		);
		t.createController();

		expect(t.lastNotice.Warning.includes('String with Business unit')).toBeTruthy();
	});

	it('should notify with 2 data things', function (t) {
		t.backend.with.translation({
			Name: 'ResourceName',
			Value: 'String with {0} and {1}',
		});
		t.backend.with.configurationValidation(
			{
				Resource: 'ResourceName',
				Data: ['Data thing 1', 'Data thing 2']
			}
		);
		t.createController();

		expect(t.lastNotice.Warning.includes('String with Data thing 1 and Data thing 2')).toBeTruthy();
	});

	it('should notify with 2 configuration validation messages', function (t) {
		t.backend.with
			.configurationValidation({Resource: 'Message1'})
			.configurationValidation({Resource: 'Message2'})
		;
		t.createController();

		expect(t.lastNotice.Warning.includes('Message1')).toBeTruthy();
		expect(t.lastNotice.Warning.includes('Message2')).toBeTruthy();
	});

	it('should have "configuration issues found" message', function (t) {
		t.backend.with
			.translation({
				Name: 'RtaConfigurationIssuesFound',
				Value: 'Configuration issues found',
			})
			.configurationValidation({
				Resource: 'SomethingIsWrong'
			});
		t.createController();

		expect(t.lastNotice.Warning.includes('Configuration issues found')).toBeTruthy();
	});
	
	it('should not notify valid configuration', function (t) {
		t.createController();

		expect(t.lastNotice).toBeUndefined();
	})

});
