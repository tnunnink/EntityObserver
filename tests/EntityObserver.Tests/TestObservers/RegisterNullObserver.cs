using EntityObserver.Tests.TestEntities;

namespace EntityObserver.Tests.TestObservers
{
    public class RegisterNullObserver : Observer<Person>
    {
        public RegisterNullObserver(Person entity) : base(entity)
        {
            RegisterObserver(null!);
        }
    }
}