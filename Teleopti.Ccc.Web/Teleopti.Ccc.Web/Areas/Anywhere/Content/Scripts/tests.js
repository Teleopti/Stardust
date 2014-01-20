buster.testCase("My thing", {
	"has the foo and bar": function () {
		assert.equals("foo", "bar");
	},

	"states the obvious": function () {
		assert(true);
	}
})
