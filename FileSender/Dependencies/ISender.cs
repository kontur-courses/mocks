namespace FileSender.Dependencies
{
    public interface ISender
    {
        bool TrySend(byte[] content);
    }
}