import { Injectable } from '@angular/core';

@Injectable()
export class HeatMapColorHelper {
	min: number;
	max: number;
	gradients: Array<HeatMapColorValue>;
	constructor() {
		this.gradients = [
			new HeatMapColorValue(255, 0, 0, -100),  //red
			new HeatMapColorValue(255, 255, 255, 0),        //white
			new HeatMapColorValue(0, 0, 255, 100),    //blue
		];

		this.min = this.gradients[0].value;
		this.max = this.gradients[this.gradients.length -1].value;
	}

	private padZero(str) {
		let len = 2;
		let zeros = new Array(len).join('0');
		return (zeros + str).slice(-len);
	}

	public invertColor(hex, bw) {
		if (hex.indexOf('#') === 0) {
			hex = hex.slice(1);
		}
		// convert 3-digit hex to 6-digits.
		if (hex.length === 3) {
			hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
		}
		if (hex.length !== 6) {
			throw new Error('Invalid HEX color.');
		}
		let r = parseInt(hex.slice(0, 2), 16),
			g = parseInt(hex.slice(2, 4), 16),
			b = parseInt(hex.slice(4, 6), 16);
		if (bw) {
			// http://stackoverflow.com/a/3943023/112731
			return (r * 0.299 + g * 0.587 + b * 0.114) > 186
				? '#000000'
				: '#FFFFFF';
		}
		// pad each with zeros and return
		return "#" + this.padZero((255 - r).toString(16)) + this.padZero((255 - g).toString(16)) + this.padZero((255 - b).toString(16));
	}

	public getColor(value: number) {
		if (value < this.min) value = this.min;
		if (value > this.max) value = this.max;

		for (let i = 0; i < this.gradients.length; i++) {
			const gradientColor = this.gradients[i];
			if (!(value < gradientColor.value)) continue;
			const previousGradientColor = this.gradients[Math.max(0, i - 1)];
			const diff = previousGradientColor.value - gradientColor.value;
			let fraction = 0;
			if (diff !== 0)
				fraction = (value - gradientColor.value) / diff;
			const red = Math.round((previousGradientColor.red - gradientColor.red) * fraction + gradientColor.red);
			const green = Math.round((previousGradientColor.green - gradientColor.green) * fraction + gradientColor.green);
			const blue = Math.round((previousGradientColor.blue - gradientColor.blue) * fraction + gradientColor.blue);

			return `#${this.convertRgbToHex(red, green, blue)}`;
		}

		const last = this.gradients[this.gradients.length - 1];
		return `#${this.convertRgbToHex(last.red, last.green, last.blue)}`;
	}

	private convertToHex(value: number) {
		let hex = Number(value).toString(16);
		if (hex.length < 2) {
			hex = `0${hex}`;
		}
		return hex;
	}

	private convertRgbToHex(r: number, g: number, b: number) {
		const red = this.convertToHex(r);
		const green = this.convertToHex(g);
		const blue = this.convertToHex(b);
		return red + green + blue;
	}
}


class HeatMapColorValue {
	red: number;
	green: number;
	blue: number;
	value: number;

	constructor(r: number, g: number, b: number, v : number) {
		this.red = r;
		this.green = g;
		this.blue = b;
		this.value = v;
	}
}