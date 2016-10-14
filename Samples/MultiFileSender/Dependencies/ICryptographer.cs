using System.Security.Cryptography.X509Certificates;

namespace Samples.MultiFileSender.Dependencies
{
    public interface ICryptographer
    {
        byte[] Sign(byte[] content, X509Certificate certificate);
    }
}