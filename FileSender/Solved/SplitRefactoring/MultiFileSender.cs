using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FileSender.Dependencies;

namespace FileSender.Solved.SplitRefactoring
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
