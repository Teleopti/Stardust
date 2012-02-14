namespace Teleopti.Ccc.Domain.Security
{
    public interface IOneWayEncryption
    {
        string EncryptString(string value);
    	string EncryptString(string value, string salt);
        string EncryptStringWithBase64(string value, string salt);
    }
}