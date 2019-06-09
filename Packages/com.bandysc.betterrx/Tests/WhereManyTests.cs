using System.Collections;
using System.Collections.Generic;
using BetterRx;
using NUnit.Framework;
using UniRx;
using UnityEngine;

public class WhereManyTests
{
    public class MyObject
    {
        public IReactiveProperty<bool> IsChoosen { get; }

        public int Id { get; }
        
        public MyObject(int id, bool selected)
        {
            IsChoosen = new ReactiveProperty<bool>(selected);
            Id = id;
        }
    }
    
    [Test]
    public void TestSimpleWhere()
    {
        var baseCollection = new ReactiveCollection<MyObject>();

        var o1 = new MyObject(1, true);
        var o2 = new MyObject(2, false);
        
        baseCollection.Add(o1);
        baseCollection.Add(o2);
        
        var selected = baseCollection.WhereMany(t => t.IsChoosen.Value, t => t.IsChoosen.AsUnitObservable());
        
        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual(1, selected[0].Id);


        o1.IsChoosen.Value = false;
        
        Assert.AreEqual(0, selected.Count);

        o2.IsChoosen.Value = true;
        
        Assert.AreEqual(1, selected.Count);

        o1.IsChoosen.Value = true;
        
        Assert.AreEqual(2, selected.Count);
    }
}
