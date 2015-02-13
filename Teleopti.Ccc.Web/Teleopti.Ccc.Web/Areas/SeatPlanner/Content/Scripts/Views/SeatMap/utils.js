define([
	'fabric'
], function(
	fabric
) {
	fabric.Object.prototype.transparentCorners = false;
	fabric.Object.prototype.lockScalingFlip = true;
	fabric.Object.prototype.hasBorders = false;

	return {
		ScaleImage: function(canvas, image) {

			var ratio = canvas.height / image.height;
			image.set({
				scaleY: ratio,
				scaleX: ratio
			});

		}
	};

});