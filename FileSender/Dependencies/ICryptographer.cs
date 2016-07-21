using System.Security.Cryptography.X509Certificates;

namespace FileSender.Dependencies
{
    public interface ICryptographer
    {
        byte[] Sign(byte[] content, X509Certificate certificate);
    }
}