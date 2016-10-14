namespace Samples.MultiFileSender.Dependencies
{
    public interface ISender
    {
        bool TrySend(byte[] content);
    }
}