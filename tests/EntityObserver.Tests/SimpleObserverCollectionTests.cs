using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EntityObserver.Tests.TestEntities;
using EntityObserver.Tests.TestObservers;
using FluentAssertions;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class SimpleObserverCollectionTests
    {
        private Fixture _fixture;
        private IEnumerable<Address> _entities;
        private IEnumerable<AddressObserver> _observers;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _entities = _fixture.CreateMany<Address>();
            _observers = _entities.Select(e => new AddressObserver(e));
        }
        
        [Test]
        public void New_Default_ShouldNotBeNullAndEmpty()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var observer = new ObserverCollection<AddressObserver>();

            observer.Should().NotBeNull();
            observer.Should().BeEmpty();
        }

        [Test]
        public void New_ValidCollection_ShouldNotBeNullOrEmpty()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);

            observer.Should().NotBeNull();
            observer.Should().NotBeEmpty();
        }
        
        [Test]
        public void New_ValidCollection_AddedModifiedAndRemovedShouldBeEmpty()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);

            observer.AddedItems.Should().BeEmpty();
            observer.ModifiedItems.Should().BeEmpty();
            observer.RemovedItems.Should().BeEmpty();
        }
        
        [Test]
        public void New_ValidCollection_IsChangedShouldBeFalse()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);

            observer.IsChanged.Should().BeFalse();
        }
        
        [Test]
        public void New_ValidCollection_HasErrorsShouldBeFalse()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);

            observer.HasErrors.Should().BeFalse();
        }

        [Test]
        public void IsChanged_AddedItem_ShouldBeTure()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);
            
            observer.Add(new AddressObserver(_fixture.Create<Address>()));

            observer.IsChanged.Should().BeTrue();
        }
        
        
        [Test]
        public void IsChanged_RemoveItem_ShouldBeTure()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);
            
            observer.Remove(observer.First());

            observer.IsChanged.Should().BeTrue();
        }
        
        [Test]
        public void IsChanged_ModifiedItem_ShouldBeTure()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);

            observer.First().City = _fixture.Create<string>();

            observer.IsChanged.Should().BeTrue();
        }

        [Test]
        public void AcceptChanged_WhenCalled_ShouldClearCollections()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);
            
            observer.Add(new AddressObserver(_fixture.Create<Address>()));
            observer.Remove(observer.First());
            observer.First().City = _fixture.Create<string>();

            observer.AddedItems.Should().HaveCount(1);
            observer.RemovedItems.Should().HaveCount(1);
            observer.ModifiedItems.Should().HaveCount(1);
            
            observer.AcceptChanges();

            observer.AddedItems.Should().BeEmpty();
            observer.RemovedItems.Should().BeEmpty();
            observer.ModifiedItems.Should().BeEmpty();
        }
        
        [Test]
        public void AcceptChanged_WhenCalled_ShouldRetainUpdatedItems()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);
            var added = new AddressObserver(_fixture.Create<Address>());
            var removed = observer.First();
            var modified = observer.Last();
            var value = _fixture.Create<string>();
            
            
            observer.Add(added);
            observer.Remove(removed);
            modified.City = value;

            observer.AcceptChanges();

            observer.Should().Contain(added);
            observer.Should().NotContain(removed);
            observer.Single(a => a.Id == modified.Id).City.Should().Be(value);
        }
        
        [Test]
        public void RejectChanges_WhenCalled_ShouldClearCollections()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);
            
            observer.Add(new AddressObserver(_fixture.Create<Address>()));
            observer.Remove(observer.First());
            observer.First().City = _fixture.Create<string>();

            observer.AddedItems.Should().HaveCount(1);
            observer.RemovedItems.Should().HaveCount(1);
            observer.ModifiedItems.Should().HaveCount(1);
            
            observer.RejectChanges();

            observer.AddedItems.Should().BeEmpty();
            observer.RemovedItems.Should().BeEmpty();
            observer.ModifiedItems.Should().BeEmpty();
        }
        
        [Test]
        public void RejectChanges_WhenCalled_ShouldRevertUpdatedItems()
        {
            var observer = new ObserverCollection<AddressObserver>(_observers);
            var added = new AddressObserver(_fixture.Create<Address>());
            var removed = observer.First();
            var modified = observer.Last();
            var city = modified.City;
            
            
            observer.Add(added);
            observer.Remove(removed);
            modified.City = _fixture.Create<string>();

            observer.RejectChanges();

            observer.Should().NotContain(added);
            observer.Should().Contain(removed);
            observer.Single(a => a.Id == modified.Id).City.Should().Be(city);
        }
    }
}