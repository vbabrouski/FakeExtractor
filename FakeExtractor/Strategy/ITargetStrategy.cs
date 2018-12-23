using System;
using System.Collections.Generic;

namespace FakeExtractor.Strategy
{
    public interface ITargetStrategy : IStrategy
    {
        Dictionary<FakesType, HashSet<Type>> DetectedTypes { get; }
        HashSet<string> FoundFakesConfiguration { get; }
    }
}