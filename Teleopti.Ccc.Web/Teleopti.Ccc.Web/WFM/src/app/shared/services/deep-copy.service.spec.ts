import { TestBed, inject } from '@angular/core/testing';
import { DeepCopyService } from './deep-copy.service';

describe('DeepCopyService', () => {
	beforeEach(() => {
		TestBed.configureTestingModule({
			providers: [DeepCopyService]
		});
	});

	it('should be created', inject([DeepCopyService], (service: DeepCopyService) => {
		expect(service).toBeTruthy();
	}));

	it('should be able to copy an object', inject([DeepCopyService], (service: DeepCopyService) => {
		const originalObj: any = {
			a: {
				b: 'c'
			}
		};
		const copiedObj = service.copy(originalObj);

		expect(Object.keys(copiedObj).length).toBe(1);

		originalObj['a']['b'] = 1;

		expect(copiedObj['a']['b']).toBe('c');
	}));

	it('should be able to copy an array', inject([DeepCopyService], (service: DeepCopyService) => {
		const originalArr = [{ a: { b: 'c' } }];
		const copiedArr = service.copy(originalArr);

		expect(Object.keys(copiedArr).length).toBe(1);

		originalArr[0]['a']['b'] = 'd';

		expect(Object.prototype.toString.call(copiedArr)).toBe('[object Array]');
		expect(copiedArr[0]['a']['b']).toBe('c');
	}));

	it('should be able to copy an string', inject([DeepCopyService], (service: DeepCopyService) => {
		let originalObj = 'I am a string';
		const copiedObj = service.copy(originalObj);

		expect(originalObj).toBe('I am a string');

		originalObj = 'I am a new string';

		expect(copiedObj).toBe('I am a string');
	}));

	it('should be able to copy an number', inject([DeepCopyService], (service: DeepCopyService) => {
		let orginalNum = 123456;
		const copiedNum = service.copy(orginalNum);

		expect(orginalNum).toBe(123456);

		orginalNum = 1;

		expect(copiedNum).toBe(123456);
	}));
});
