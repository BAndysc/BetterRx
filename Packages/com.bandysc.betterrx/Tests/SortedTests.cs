
using System.Collections.Generic;
using BetterRx;
using NUnit.Framework;
using UniRx;

public class SortedTests
{
    [Test]
    public void SortTest()
    {
        var collection = new ReactiveCollection<int>();

        collection.Add(1);
        collection.Add(5);
        collection.Add(4);
        
        var sorted = collection.Sort((a, b) => a.CompareTo(b), null);
        
        Assert.AreEqual(3, sorted.Count);
        Assert.AreEqual(true, IsSorted(sorted));
        
        
        collection.Add(0);
        collection.Add(8);
        collection.Add(2);
        
        Assert.AreEqual(6, sorted.Count);
        Assert.AreEqual(true, IsSorted(sorted));

        collection.Remove(1);
        collection.Remove(4);
        collection.Remove(5);
        collection.Add(1);
        
        Assert.AreEqual(4, sorted.Count);
        Assert.AreEqual(true, IsSorted(sorted));
    }

    private bool IsSorted(IEnumerable<int> ints)
    {
        int? prev = null;

        foreach (var @int in ints)
        {
            if (prev.HasValue)
            {
                if (prev.Value > @int)
                    return false;
            }

            prev = @int;
        }

        return true;
    }
}