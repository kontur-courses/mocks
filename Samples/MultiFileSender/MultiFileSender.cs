using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Samples.MultiFileSender.Dependencies;

namespace Samples.MultiFileSender
{
    public class MultiFileSender
    {
        private readonly ISingleFileSender singleFileSender;

        public MultiFileSender(ISingleFileSender singleFileSender)
        {
            this.singleFileSender = singleFileSender;
        }

        public Result SendFiles(File[] files, X509Certificate certificate)
        {
            return new Result
            {
                SkippedFiles = files
                    .Where(file => !singleFileSender.TrySendFile(file, certificate))
                    .ToArray()
            };
        }
        
        public class Result
        {
            public File[] SkippedFiles { get; set; }
        }
    }
}
