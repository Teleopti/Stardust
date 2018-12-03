describe('colorUtils', function() {
	var colorUtils;
	beforeEach(module('wfm.utilities'));
	beforeEach(inject(function($injector) {
		colorUtils = $injector.get('colorUtils');
	}));

	it('should get black text color for a hex light background', function() {
		var textColor = colorUtils.getTextColorBasedOnBackgroundColor('#dfdfdf');
		expect(textColor).toBe('black');
	});

	it('should get white text color for a hex dark background', function() {
		var textColor = colorUtils.getTextColorBasedOnBackgroundColor('#333');
		expect(textColor).toBe('white');
	});

	it('should get black text color for a RGB light background', function() {
		var textColor = colorUtils.getTextColorBasedOnBackgroundColor('rgb(200,200,200)');
		expect(textColor).toBe('black');
	});

	it('should get white text color for a RGB dark background', function() {
		var textColor = colorUtils.getTextColorBasedOnBackgroundColor('rgb(20,20,20,1)');
		expect(textColor).toBe('white');
	});

	it('should be able to convert a HEX color #333 to RGB', function() {
		var hexColor = colorUtils.hexToRGB('#333');
		expect(hexColor).toBe('rgb(51,51,51)');
	});

	it('should be able to convert a HEX color #c00000 to RGB', function() {
		var hexColor = colorUtils.hexToRGB('#c00000');
		expect(hexColor).toBe('rgb(192,0,0)');
	});

	it('should be able to convert color to RGB', function() {
		var rgbColor = colorUtils.colorToRGB('20,20,20');
		expect(rgbColor).toBe('rgb(20,20,20)');
	});
});
