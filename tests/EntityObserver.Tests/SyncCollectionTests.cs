using System.Linq;
using AutoFixture;
using EntityObserver.Tests.TestEntities;
using EntityObserver.Tests.TestObservers;
using FluentAssertions;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class SyncCollectionTests
    {
        private Person _person;
        private Fixture _fixture;
        private SynchronizingPersonObserver _observer;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _person = _fixture.Create<Person>();
            _observer = new SynchronizingPersonObserver(_person);
        }

        [Test]
        public void Add_NewItem_ShouldUpdateUnderlyingCollection()
        {
            var item = new CarObserver(_fixture.Create<Car>());

            _observer.Cars.Add(item);

            _observer.Entity.Cars.Should().Contain(item.Entity);
        }
        
        [Test]
        public void Remove_NewItem_ShouldUpdateUnderlyingCollection()
        {
            var item = _observer.Cars.First();

            _observer.Cars.Remove(item);

            _observer.Entity.Cars.Should().NotContain(item.Entity);
        }

        [Test]
        public void Clear_WhenCalled_ShouldUpdateUnderlyingCollection()
        {
            _observer.Cars.Clear();

            _observer.Entity.Cars.Should().BeEmpty();
        }
    }
}