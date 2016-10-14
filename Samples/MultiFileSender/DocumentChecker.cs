using Samples.MultiFileSender.Dependencies;

namespace Samples.MultiFileSender
{
    public class DocumentChecker : IDocumentChecker
    {
        private readonly IDateTimeService dateTimeService;

        public DocumentChecker(IDateTimeService dateTimeService)
        {
            this.dateTimeService = dateTimeService;
        }

        public bool CheckDocument(Document document)
        {
            return CheckFormat(document) && CheckActual(document);
        }

        private bool CheckFormat(Document document)
        {
            return document.Format == "4.0" ||
                   document.Format == "3.1";
        }

        private bool CheckActual(Document document)
        {
            return document.Created.AddMonths(1) > dateTimeService.Now;
        }
    }
}