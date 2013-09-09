<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns="urn:schemas-microsoft-com:office:spreadsheet"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:msxsl="urn:schemas-microsoft-com:xslt"
 xmlns:user="urn:my-scripts"
 xmlns:o="urn:schemas-microsoft-com:office:office"
 xmlns:x="urn:schemas-microsoft-com:office:excel"
 xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet" >

	<xsl:template match="/">
		<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet"
		  xmlns:o="urn:schemas-microsoft-com:office:office"
		  xmlns:x="urn:schemas-microsoft-com:office:excel"
		  xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
		  xmlns:html="http://www.w3.org/TR/REC-html40">
			<xsl:apply-templates/>
		</Workbook>
	</xsl:template>

	<xsl:template match="/root">
		<Worksheet>
			<xsl:attribute name="ss:Name">
				<xsl:value-of select="local-name(/root/data)"/>
			</xsl:attribute>
			<Table x:FullColumns="1" x:FullRows="1">
				<xsl:apply-templates select="data"/>
			</Table>
		</Worksheet>
	</xsl:template>

	<xsl:template match="/root/data">
		<Row>
			<Cell><Data ss:Type="String"><xsl:value-of select="@name"/></Data></Cell>
			<xsl:apply-templates/>
		</Row>
	</xsl:template>

	<xsl:template match="/root/data/*">
		<Cell>
			<Data ss:Type="String">
				<xsl:value-of select="."/>
			</Data>
		</Cell>
	</xsl:template>
	
</xsl:stylesheet>