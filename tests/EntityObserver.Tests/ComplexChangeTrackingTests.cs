using AutoFixture;
using EntityObserver.Tests.TestEntities;
using EntityObserver.Tests.TestObservers;
using FluentAssertions;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class ComplexChangeTrackingTests
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
        public void AcceptChanges_WhenCalled_IsChangeShouldBeFalse()
        {
            _observer.Address.City = _fixture.Create<string>();
            _observer.IsChanged.Should().BeTrue();

            _observer.AcceptChanges();

            _observer.IsChanged.Should().BeFalse();
        }

        [Test]
        public void AcceptChanges_WhenCalled_ShouldNotBeOriginal()
        {
            var original = _person.Address.City;
            _observer.Address.City = _fixture.Create<string>();

            _observer.AcceptChanges();

            _observer.Address.City.Should().NotBe(original);
        }

        [Test]
        public void AcceptChanged_WhenCalled_GetOriginalShouldBeNewValue()
        {
            _observer.Address.City = _fixture.Create<string>();

            _observer.AcceptChanges();

            _observer.Address.GetOriginalValue(m => m.City).Should().Be(_observer.Address.City);
        }

        [Test]
        public void AcceptChanges_MultipleProperties_AllShouldRetainValues()
        {
            var model = _fixture.Create<Address>();

            _observer.Address.Street = model.Street;
            _observer.Address.City = model.City;
            _observer.Address.State = model.State;
            _observer.Address.Zip = model.Zip;

            _observer.AcceptChanges();

            _observer.Address.Street.Should().Be(model.Street);
            _observer.Address.City.Should().Be(model.City);
            _observer.Address.State.Should().Be(model.State);
            _observer.Address.Zip.Should().Be(model.Zip);
        }

        [Test]
        public void RejectChanges_WhenCalled_IsChangedShouldBeFalse()
        {
            _observer.Address.State = _fixture.Create<string>();

            _observer.RejectChanges();

            _observer.IsChanged.Should().BeFalse();
        }

        [Test]
        public void RejectChanges_WhenCalled_ShouldBeOriginal()
        {
            var original = _person.Address.City;
            _observer.Address.City = _fixture.Create<string>();

            _observer.RejectChanges();

            _observer.Address.City.Should().Be(original);
        }

        [Test]
        public void RejectChanges_MultipleValues_ShouldAllBeReverted()
        {
            var street = _observer.Address.Street;
            var city = _observer.Address.City;
            var state = _observer.Address.State;
            var zip = _observer.Address.Zip;
            
            _observer.Address.Street = _fixture.Create<string>();
            _observer.Address.City = _fixture.Create<string>();
            _observer.Address.State = _fixture.Create<string>();
            _observer.Address.Zip = _fixture.Create<int>();
            
            _observer.Address.Street.Should().NotBe(street);
            _observer.Address.City.Should().NotBe(city);
            _observer.Address.State.Should().NotBe(state);
            _observer.Address.Zip.Should().NotBe(zip);
            
            _observer.RejectChanges();
            
            _observer.Address.Street.Should().Be(street);
            _observer.Address.City.Should().Be(city);
            _observer.Address.State.Should().Be(state);
            _observer.Address.Zip.Should().Be(zip);
        }
    }
}