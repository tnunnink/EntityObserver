using AutoFixture;
using FluentAssertions;
using EntityObserver.Tests.TestModels;
using EntityObserver.Tests.TestObservers;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class SimpleChangeTrackingTests
    {
        private Car _model;
        private Fixture _fixture;
        private CarObserver _observer;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _model = _fixture.Create<Car>();
            _observer = new CarObserver(_model);
        }

        [Test]
        public void IsChanged_ValueChanged_ShouldBeTrue()
        {
            var value = _fixture.Create<string>();

            _observer.Make = value;

            _observer.IsChanged.Should().BeTrue();
        }
        
        [Test]
        public void IsChanged_ValueReset_ShouldBeFalse()
        {
            var original = _model.Make;
            var value = _fixture.Create<string>();

            _observer.Make = value;
            _observer.Make = original;

            _observer.IsChanged.Should().BeFalse();
        }

        [Test]
        public void GetOriginalValue_NoChanges_ShouldRetainValue()
        {
            var original = _model.Make;

            _observer.GetOriginalValue(m => m.Make).Should().Be(original);
        }
        
        [Test]
        public void GetOriginalValue_SingleChanges_ShouldRetainValue()
        {
            var original = _model.Make;

            _observer.Make = _fixture.Create<string>();

            _observer.GetOriginalValue(m => m.Make).Should().Be(original);
        }

        [Test]
        public void GetOriginalValue_MultipleChanges_ShouldRetainValue()
        {
            var original = _model.Make;

            _observer.Make = _fixture.Create<string>();
            _observer.Make = _fixture.Create<string>();
            _observer.Make = _fixture.Create<string>();

            _observer.GetOriginalValue(m => m.Make).Should().Be(original);
        }

        [Test]
        public void AcceptChanged_ValueChange_IsChangedShouldBeFalse()
        {
            _observer.Make = _fixture.Create<string>();
            _observer.IsChanged.Should().BeTrue();
            
            _observer.AcceptChanges();

            _observer.IsChanged.Should().BeFalse();
        }
        
        [Test]
        public void AcceptChanged_ValueChange_OriginalShouldBeNewValue()
        {
            var newValue = _fixture.Create<string>();
            _observer.Make = newValue;

            _observer.AcceptChanges();

            _observer.GetOriginalValue(m => m.Make).Should().Be(newValue);
        }

        [Test]
        public void AcceptChanges_MultipleProperties_AllShouldRetainValues()
        {
            var model = _fixture.Create<Car>();

            _observer.Vin = model.Vin;
            _observer.Make = model.Make;
            _observer.Mileage = model.Mileage;
            _observer.Cost = model.Cost;
            
            _observer.AcceptChanges();

            _observer.Vin.Should().Be(model.Vin);
            _observer.Make.Should().Be(model.Make);
            _observer.Mileage.Should().Be(model.Mileage);
            _observer.Cost.Should().Be(model.Cost);
        }

        [Test]
        public void RejectChanges_ValueChange_IsChangedShouldBeFalse()
        {
            _observer.Make = _fixture.Create<string>();
            _observer.IsChanged.Should().BeTrue();
            
            _observer.RejectChanges();

            _observer.IsChanged.Should().BeFalse();
        }
        
        [Test]
        public void RejectChanges_ValueChange_ValueShouldBeOriginal()
        {
            var original = _model.Make;
            _observer.Make = _fixture.Create<string>();;

            _observer.RejectChanges();

            _observer.Make.Should().Be(original);
        }
        
        [Test]
        public void RejectChanges_ValueChange_GetOriginalShouldBeOriginal()
        {
            var original = _model.Make;
            _observer.Make = _fixture.Create<string>();;

            _observer.RejectChanges();

            _observer.GetOriginalValue(m => m.Make).Should().Be(original);
        }
        
        
        [Test]
        public void RejectChanges_MultipleProperties_AllShouldRevertValues()
        {
            var original = _model;

            _observer.Vin = _fixture.Create<string>();
            _observer.Make = _fixture.Create<string>();;
            _observer.Mileage = _fixture.Create<int>();;
            _observer.Cost = _fixture.Create<int>();;
            
            _observer.RejectChanges();

            _observer.Vin.Should().Be(original.Vin);
            _observer.Make.Should().Be(original.Make);
            _observer.Mileage.Should().Be(original.Mileage);
            _observer.Cost.Should().Be(original.Cost);
        }
    }
}