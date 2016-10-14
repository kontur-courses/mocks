using Samples.MultiFileSender.Dependencies;

namespace Samples.MultiFileSender
{
    public interface IDocumentChecker
    {
        bool CheckDocument(Document document);
    }
}