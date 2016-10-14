using System.Security.Cryptography.X509Certificates;
using Samples.MultiFileSender.Dependencies;

namespace Samples.MultiFileSender
{
    public class SingleFileSender : ISingleFileSender
    {
        private readonly IDocumentChecker documentChecker;
        private readonly ICryptographer cryptographer;
        private readonly ISender sender;
        private readonly IRecognizer recognizer;

        public SingleFileSender(IDocumentChecker documentChecker,
            ICryptographer cryptographer,
            ISender sender,
            IRecognizer recognizer)
        {
            this.documentChecker = documentChecker;
            this.cryptographer = cryptographer;
            this.sender = sender;
            this.recognizer = recognizer;
        }

        public bool TrySendFile(File file, X509Certificate certificate)
        {
            Document document;
            if (!recognizer.TryRecognize(file, out document))
                return false;
            if (!documentChecker.CheckDocument(document))
                return false;
            var signedContent = cryptographer.Sign(document.Content, certificate);
            return sender.TrySend(signedContent);
        }
    }
}