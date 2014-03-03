
define(['buster'], function (buster) {

	buster.testCase("My thing", {
		"has the foo and bar": function() {
			assert.equals("foo", "foo");
		},

		"states the obvious": function() {
			assert(true);
		},

		"should create menu view model": function(done) {

			require(['menu'], function(menu) {
				var m = new menu();
				assert(m != null);
				done();
			});

		}
	});

});
