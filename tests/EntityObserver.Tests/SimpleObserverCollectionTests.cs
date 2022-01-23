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
    }
}