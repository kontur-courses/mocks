using System.Security.Cryptography.X509Certificates;
using Samples.MultiFileSender.Dependencies;

namespace Samples.MultiFileSender
{
    public interface ISingleFileSender
    {
        bool TrySendFile(File file, X509Certificate certificate);
    }
}