export class DocumentMock implements Partial<Document> {
	location = { href: '', hash: '', reload() {} } as Location;
}
