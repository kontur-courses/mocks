using System;

namespace Samples.MultiFileSender
{
    public interface IDateTimeService
    {
        DateTime Now { get; }
    }
}