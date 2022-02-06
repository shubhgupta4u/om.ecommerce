namespace om.shared.api.common.Interfaces
{
    public interface ICryptoService
    {
        string Encrypt(string plainText);
        string Decrypt(string plainText);
    }
}
