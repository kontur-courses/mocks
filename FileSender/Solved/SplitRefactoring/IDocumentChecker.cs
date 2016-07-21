using FileSender.Dependencies;

namespace FileSender.Solved.SplitRefactoring
{
    public interface IDocumentChecker
    {
        bool CheckDocument(Document document);
    }
}