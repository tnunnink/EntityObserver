using System.Linq;
using AutoFixture;
using EntityObserver.Tests.TestEntities;
using EntityObserver.Tests.TestObservers;
using FluentAssertions;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class ComplexObserverCollectionTests
    {
        private Person _person;
        private Fixture _fixture;
        private PersonObserver _observer;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _person = _fixture.Create<Person>();
            _observer = new PersonObserver(_person);
        }
        
        [Test]
        public void Add_ValidItem_IsChangedShouldBeTrue()
        {
            var car = new CarObserver(_fixture.Create<Car>());
            
            _observer.Cars.Add(car);

            _observer.IsChanged.Should().BeTrue();
        }

        [Test]
        public void Add_ValidItem_ShouldRaisePropertyChangedForIsChanged()
        {
            var car = new CarObserver(_fixture.Create<Car>());
            var monitor = _observer.Monitor();
            
            _observer.Cars.Add(car);

            monitor.Should().RaisePropertyChangeFor(m => m.IsChanged);
        }
        
        [Test]
        public void Remove_ValidItem_IsChangedShouldBeTrue()
        {
            var first = _observer.Cars.First();
            
            _observer.Cars.Remove(first);

            _observer.IsChanged.Should().BeTrue();
        }

        [Test]
        public void Remove_ValidItem_ShouldRaisePropertyChangedForIsChanged()
        {
            var first = _observer.Cars.First();
            var monitor = _observer.Monitor();
            
            _observer.Cars.Remove(first);

            monitor.Should().RaisePropertyChangeFor(m => m.IsChanged);
        }
    }
}