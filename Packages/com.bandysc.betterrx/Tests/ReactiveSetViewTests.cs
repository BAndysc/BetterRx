using NUnit.Framework;

namespace BetterRx.Collections.Tests
{
    public class ReactiveSetViewTests
    {
        [Test]
        public void TestAdd()
        {
            IReactiveSet<int> source = new ReactiveSet<int>();
            
            source.Add(3);
            
            ReactiveSetView<int> view = new ReactiveSetView<int>(source);

            Assert.AreEqual(1, view.Count);
            Assert.AreEqual(3, view[0]);
            
            source.Add(4);
            Assert.AreEqual(4, view[1]);
        }
    }
}