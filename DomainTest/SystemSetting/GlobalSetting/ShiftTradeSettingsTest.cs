using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.DomainTest.SystemSetting.GlobalSetting
{
	[TestFixture]
	public class ShiftTradeBusinessRuleConfigTest
	{
		[Test]
		public void ShouldBeDeserializedCorrectly()
		{
			var originalSetting = new ShiftTradeSettings
			{
				MaxSeatsValidationEnabled = true,
				MaxSeatsValidationSegmentLength = 23,
				BusinessRuleConfigs = new[]
				{
					new ShiftTradeBusinessRuleConfig
					{
						FriendlyName = "Data Part Of Agent Day Rule",
						BusinessRuleType = typeof(DataPartOfAgentDay).FullName,
						Enabled = true,
						HandleTypeOnBroken = RequestHandleType.AutoDeny
					},
					new ShiftTradeBusinessRuleConfig
					{
						FriendlyName = "Min Weekly Rest Rule",
						BusinessRuleType = typeof(MinWeeklyRestRule).FullName,
						Enabled = true,
						HandleTypeOnBroken = RequestHandleType.SetToPending
					},
					new ShiftTradeBusinessRuleConfig
					{
						FriendlyName = "Min Week Work Time Rule",
						BusinessRuleType = typeof(MinWeekWorkTimeRule).FullName,
						Enabled = false,
						HandleTypeOnBroken = RequestHandleType.SetToPending
					}
				}
			};

			var xmlSerializer = new XmlSerializer(typeof(ShiftTradeSettings));

			string serializedValue;
			using (var stringWriter = new StringWriter())
			{
				using (var xmlWriter = new XmlTextWriter(stringWriter))
				{
					xmlSerializer.Serialize(xmlWriter, originalSetting);
					serializedValue = stringWriter.ToString();
				}
			}

			ShiftTradeSettings deserializedSetting;
			using (TextReader reader = new StringReader(serializedValue))
			{
				deserializedSetting = (ShiftTradeSettings)xmlSerializer.Deserialize(reader);
			}

			Assert.AreEqual(originalSetting.MaxSeatsValidationEnabled, deserializedSetting.MaxSeatsValidationEnabled);
			Assert.AreEqual(originalSetting.MaxSeatsValidationSegmentLength, deserializedSetting.MaxSeatsValidationSegmentLength);

			Assert.AreEqual(originalSetting.BusinessRuleConfigs.Length, deserializedSetting.BusinessRuleConfigs.Length);
			for (var i = 0; i < originalSetting.BusinessRuleConfigs.Length; i++)
			{
				var originalRuleConfig = originalSetting.BusinessRuleConfigs[i];
				var deserializedRuleConfig = deserializedSetting.BusinessRuleConfigs[i];
				Assert.AreEqual(originalRuleConfig.FriendlyName, deserializedRuleConfig.FriendlyName);
				Assert.AreEqual(originalRuleConfig.BusinessRuleType, deserializedRuleConfig.BusinessRuleType);
				Assert.AreEqual(originalRuleConfig.Enabled, deserializedRuleConfig.Enabled);
				Assert.AreEqual(originalRuleConfig.HandleTypeOnBroken, deserializedRuleConfig.HandleTypeOnBroken);
			}
		}
	}
}