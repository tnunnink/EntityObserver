using EntityObserver.Tests.TestEntities;

namespace EntityObserver.Tests.TestObservers
{
    public class RegisterMultipleSameObserver : Observer<Person>
    {
        public RegisterMultipleSameObserver(Person entity) : base(entity)
        {
            Address = new AddressObserver(entity.Address);
            
            RegisterObserver(Address);
            RegisterObserver(Address);
        }
        
        public AddressObserver Address { get; }
    }
}