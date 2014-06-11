
define(['buster'], function (buster) {
	return function () {

		buster.testCase("My thing", {
			"has the foo and bar": function () {
				assert.equals("foo", "foo");
			},

			"states the obvious": function () {
				assert(true);
			}
		});

	};
});
