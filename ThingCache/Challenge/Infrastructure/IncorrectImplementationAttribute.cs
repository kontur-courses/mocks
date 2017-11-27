using System;

namespace Challenge.Infrastructure
{
    public class IncorrectImplementationAttribute : Attribute
    {
        public IncorrectImplementationAttribute(string description)
        {
        }
    }
}