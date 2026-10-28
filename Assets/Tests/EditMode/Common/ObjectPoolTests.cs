using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

public class ObjectPoolTests
{
    [Test]
    public void Should_Create_Free_Objects_With_Factory()
    {
        ObjectPool<int> pool = new(3, i => i * 10);

        Assert.AreEqual(3, pool.Size);
        Assert.AreEqual(3, pool.Free);
        Assert.AreEqual(0, pool.Reserved);
        CollectionAssert.AreEqual(new[] { 0, 10, 20 }, pool.GetAllValues());
    }

    [Test]
    public void Should_Not_Add_Duplicates()
    {
        ObjectPool<string> pool = new();

        bool first = pool.Add("arrow");
        bool second = pool.Add("arrow");

        Assert.IsTrue(first, "A new value should be added");
        Assert.IsFalse(second, "A value already in the pool shouldn't be added again");
        Assert.AreEqual(1, pool.Size);
    }

    [Test]
    public void Should_Reserve_Given_Objects()
    {
        ObjectPool<int> pool = new(2, i => i);

        int given = pool.Give();

        Assert.AreEqual(0, given);
        Assert.IsTrue(pool.CheckReserved(given));
        Assert.AreEqual(1, pool.Free);
        Assert.AreEqual(1, pool.Reserved);
    }

    [Test]
    public void Should_Give_Default_When_Exhausted()
    {
        ObjectPool<string> pool = new(1, () => "grenade");

        pool.Give();
        string given = pool.Give();

        Assert.IsNull(given);
    }

    [Test]
    public void Should_Give_Only_Objects_Matching_Search_Definition()
    {
        ObjectPool<int> pool = new(4, i => i);

        int given = pool.Give(value => value > 1);

        Assert.AreEqual(2, given);
        Assert.IsTrue(pool.CheckFree(0), "Objects skipped by the search should stay free");
    }

    [Test]
    public void Should_Free_Reserved_Objects_On_Put()
    {
        ObjectPool<int> pool = new(2, i => i);
        int given = pool.Give();

        bool put = pool.Put(given);

        Assert.IsTrue(put);
        Assert.IsTrue(pool.CheckFree(given));
        Assert.AreEqual(2, pool.Free);
    }

    [Test]
    public void Should_Reject_Put_Of_Free_Or_Unknown_Objects()
    {
        ObjectPool<int> pool = new(1, i => i);

        Assert.IsFalse(pool.Put(0), "An object that is already free can't be put back");
        Assert.IsFalse(pool.Put(42), "An object outside the pool can't be put back");
    }

    [Test]
    public void Should_Throw_When_Checking_Unknown_Object()
    {
        ObjectPool<int> pool = new(1, i => i);

        Assert.Throws<DataException>(() => pool.CheckFree(42));
        Assert.Throws<DataException>(() => { bool _ = pool[42]; });
    }

    [Test]
    public void Should_List_Free_And_Reserved_Objects()
    {
        ObjectPool<int> pool = new(3, i => i);

        pool.Give();

        CollectionAssert.AreEqual(new[] { 1, 2 }, pool.GetFree());
        CollectionAssert.AreEqual(new[] { 0 }, pool.GetReserved());
    }

    [Test]
    public void Should_Remove_Objects()
    {
        ObjectPool<int> pool = new(5, i => i);
        pool.Give();

        bool removed = pool.Remove(4);
        bool removedUnknown = pool.Remove(42);
        pool.Remove(value => value == 3);
        pool.Remove((value, free) => !free);

        Assert.IsTrue(removed);
        Assert.IsFalse(removedUnknown);
        CollectionAssert.AreEqual(new[] { 1, 2 }, pool.GetAllValues());
    }

    [Test]
    public void Should_Add_And_Remove_With_Operators()
    {
        ObjectPool<int> pool = new();

        pool += 1;
        pool += 2;
        pool -= 1;

        Assert.IsTrue(pool.Has(2));
        Assert.IsFalse(pool.Has(1));
        Assert.AreEqual(1, pool.Size);
    }

    [Test]
    public void Should_Give_Each_Object_Once_With_Multiple_Tasks()
    {
        int size = 100;
        ObjectPool<int> pool = new((uint)size, i => i + 1);

        int[] given = Task.WhenAll(Enumerable.Range(0, size * 2).Select(_ => Task.Run(() => pool.Give()))).Result;
        List<int> received = given.Where(value => value != default).ToList();

        Assert.AreEqual(size, received.Count, "Every object should be given exactly once");
        Assert.AreEqual(size, received.Distinct().Count(), "No object should be given twice");
        Assert.AreEqual(0, pool.Free);
    }
}
