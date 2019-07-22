using System.Collections.Generic;
using NUnit.Framework;
using UniRx;

namespace BetterRx.Collections.Tests
{
    public class FilterSetTests
    {
        [Test]
        public void TestFilterAdd()
        {
            IReactiveSet<int> source = new ReactiveSet<int>();
            
            FilterSet<int> filtered = new FilterSet<int>(source, x => x > 4);

            source.Add(3);
            source.Add(4);
            source.Add(5);
            source.Add(6);
            
            Assert.IsFalse(filtered.IsSubscribed);

            List<int> ints = new List<int>();
            
            System.IDisposable disp = filtered.ObserveAdd().Subscribe(@event => { ints.Add(@event.Value); });
            
            CollectionAssert.AreEquivalent(ints, new []{5, 6});
            
            Assert.IsTrue(filtered.IsSubscribed);

            source.Add(7);
            CollectionAssert.AreEquivalent(ints, new []{5, 6, 7});
            
            disp.Dispose();
            
            Assert.IsFalse(filtered.IsSubscribed);
        }

        [Test]
        public void TestChainDispose()
        {
            IReactiveSet<int> source = new ReactiveSet<int>();
            FilterSet<int> filtered = new FilterSet<int>(source, x => x >= 4);
            FilterSet<int> filtered2 = new FilterSet<int>(filtered, x => x <= 20);
            FilterSet<int> filtered3 = new FilterSet<int>(filtered2, x => x % 2 == 0);

            source.Add(3);
            source.Add(5);
            source.Add(10);
            source.Add(99);
            
            Assert.IsFalse(filtered.IsSubscribed);
            Assert.IsFalse(filtered2.IsSubscribed);
            Assert.IsFalse(filtered3.IsSubscribed);
            
            var view = new ReactiveSetView<int>(filtered3);
            
            Assert.IsTrue(filtered.IsSubscribed);
            Assert.IsTrue(filtered2.IsSubscribed);
            Assert.IsTrue(filtered3.IsSubscribed);
            
            Assert.AreEqual(1, view.Count);
            Assert.AreEqual(10, view[0]);

            source.Add(13);
            
            Assert.AreEqual(1, view.Count);
            Assert.AreEqual(10, view[0]);

            source.Add(16);
            
            Assert.AreEqual(2, view.Count);
            Assert.AreEqual(10, view[0]);
            Assert.AreEqual(16, view[1]);

            view.Dispose();
            
            Assert.IsFalse(filtered.IsSubscribed);
            Assert.IsFalse(filtered2.IsSubscribed);
            Assert.IsFalse(filtered3.IsSubscribed);
            
            view = new ReactiveSetView<int>(filtered2);
            var view2 = new ReactiveSetView<int>(filtered);
            
            Assert.IsTrue(filtered.IsSubscribed);
            Assert.IsTrue(filtered2.IsSubscribed);
            Assert.IsFalse(filtered3.IsSubscribed);
            
            CollectionAssert.AreEquivalent(view, new int[] {5, 10, 13, 16});
            CollectionAssert.AreEquivalent(view2, new int[] {5, 10, 13, 16, 99});
            view.Dispose();
            
            Assert.IsTrue(filtered.IsSubscribed);
            Assert.IsFalse(filtered2.IsSubscribed);
            Assert.IsFalse(filtered3.IsSubscribed);
            
            view2.Dispose();
            
            Assert.IsFalse(filtered.IsSubscribed);
            Assert.IsFalse(filtered2.IsSubscribed);
            Assert.IsFalse(filtered3.IsSubscribed);
        }
    }
}