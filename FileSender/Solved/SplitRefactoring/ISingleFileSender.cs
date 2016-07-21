using System.Security.Cryptography.X509Certificates;
using FileSender.Dependencies;

namespace FileSender.Solved.SplitRefactoring
{
    public interface ISingleFileSender
    {
        bool TrySendFile(File file, X509Certificate certificate);
    }
}