
define([
    'buster',
    'views/realtimeadherencesites/vm'
], function (
    buster,
    viewModel
    ) {
	return function () {

		buster.testCase("real time adherence view model", {
			"should have no sites if none filled": function() {
				var vm = viewModel();
				assert.equals(vm.sites(), []);
			},

			"should fill sites data": function() {
				var site1 = { Name: 'London', Id: 'guid1' };
				var site2 = { Name: 'Paris', Id: 'guid2' };
				var vm = viewModel();

				vm.fill([site1, site2]);

				assert.equals(vm.sites()[0].Id, "guid1");
				assert.equals(vm.sites()[0].Name, "London");
				assert.equals(vm.sites()[1].Id, "guid2");
				assert.equals(vm.sites()[1].Name, "Paris");
			},

			"should update number out of adherence": function() {
				var vm = viewModel();

				vm.fill([{ Name: 'London', Id: 'guid1' }]);
				vm.update({ Id: 'guid1', OutOfAdherence: 1 });

				assert.equals(vm.sites()[0].OutOfAdherence(), 1);
			},

			"should update number out of adherence on existing site": function() {
				var vm = viewModel();
				vm.fill([{ Name: 'London', Id: 'guid1' }]);
				vm.update({ Id: 'guid1', OutOfAdherence: 1 });

				assert.equals(vm.sites().length, 1);
				assert.equals(vm.sites()[0].OutOfAdherence(), 1);
			},

			"should not add when update if there is no site": function() {
				var vm = viewModel();
				vm.update({ Id: 'guid1', OutOfAdherence: 1 });

				assert.equals(vm.sites().length, 0);
			},

			"should set number of agents when fill": function() {
				var expected = 37;
				var vm = viewModel();
				var site = { NumberOfAgents: expected };

				vm.fill([site]);

				assert.equals(vm.sites()[0].NumberOfAgents, expected);
			},

			"should do update from notification": function() {
				var vm = viewModel();
				var notification = {
					DomainId: 'theguid',
					BinaryData: '{"OutOfAdherence":2}',
				};
				var site = { Id: "theguid" };
				vm.fill([site]);

				vm.updateFromNotification(notification);

				assert.equals(vm.sites()[0].OutOfAdherence(), 2);
			},

			"should go to realtime adherence teams view": function() {
				var vm = viewModel();
				var site = { Id: "aguid" };
				vm.fill([site]);
				var window = require('window');
				this.stub(window, "setLocationHash");

				vm.sites()[0].openSite();

				assert.calledWith(window.setLocationHash, "realtimeadherenceteams/aguid");
			},

			"should have resources available": function () {
				require('resources').ATextResources = "text";
				var vm = new viewModel();
				assert.defined(vm.resources.ATextResources);
			},


			"should indicate if site has been updated": function () {
				var vm = viewModel();

				vm.fill([{ Id: 'guid1' }]);
				vm.update({ Id: 'guid1', OutOfAdherence: 1 });

				assert.isTrue(vm.sites()[0].hasBeenUpdated());
			},

			"should indicate if site has not been updated": function () {
				var vm = viewModel();

				vm.fill([{ Id: 'guid1' }]);

				assert.isFalse(vm.sites()[0].hasBeenUpdated());
			},

			"should fill business units": function() {
				var vm = viewModel();
				vm.fillBusinessUnits([{ Id: 'guid1', Name: 'bu1' }]);

				assert.equals(vm.businessUnits().length, 1);
				assert.equals(vm.businessUnits()[0].Id, 'guid1');
				assert.equals(vm.businessUnits()[0].Name, 'bu1');
			}

		});

	};
});