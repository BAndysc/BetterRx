using NUnit.Framework;
using UniRx;

namespace BetterRx.Collections.Tests
{
    public class ReactiveSetTests
    {
        [Test]
        public void TestAdd()
        {
            IReactiveSet<int> set = new ReactiveSet<int>();
            
            Assert.IsTrue(set.Add(2));
            Assert.IsFalse(set.Add(2));
            Assert.IsTrue(set.Add(4));
            
            Assert.AreEqual(2, set.Count);
            
            CollectionAssert.AreEquivalent(set, new[]{2, 4});
        }
        
        [Test]
        public void TestRemove()
        {
            IReactiveSet<int> set = new ReactiveSet<int>();
            
            set.Add(2);
            set.Add(2);
            set.Add(4);
            
            Assert.IsFalse(set.Remove(5));
            Assert.IsTrue(set.Remove(2));
            
            
            Assert.AreEqual(1, set.Count);
            CollectionAssert.AreEquivalent(set, new[]{4});
        }
        
        [Test]
        public void TestClear()
        {
            IReactiveSet<int> set = new ReactiveSet<int>();
            
            set.Add(2);
            set.Add(2);
            set.Add(4);
            
            set.Clear();
            
            Assert.AreEqual(0, set.Count);
            CollectionAssert.AreEquivalent(set, new int[]{});
        }
        
        [Test]
        public void TestAddObservable()
        {
            IReactiveSet<int> set = new ReactiveSet<int>();

            int? lastAdded = null;

            set.ObserveAdd().Subscribe(@event => lastAdded = @event.Value);
            
            Assert.IsTrue(set.Add(2));
            
            Assert.AreEqual(2, lastAdded);

            lastAdded = null;
            
            Assert.IsFalse(set.Add(2));
            
            Assert.IsNull(lastAdded);
            
            Assert.IsTrue(set.Add(4));
            
            Assert.AreEqual(4, lastAdded);
        }
        
        [Test]
        public void TestRemoveObservable()
        {
            IReactiveSet<int> set = new ReactiveSet<int>();

            int? lastRemoved = null;

            set.ObserveRemove().Subscribe(@event => lastRemoved = @event.Value);
            
            set.Add(2);
            set.Add(3);
            set.Add(4);

            set.Remove(3);
            Assert.AreEqual(3, lastRemoved);

            lastRemoved = null;
            
            set.Remove(3);
            
            Assert.IsNull(lastRemoved);

            set.Remove(4);
            Assert.AreEqual(4, lastRemoved);
        }
    }
}