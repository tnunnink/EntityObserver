using EntityObserver.Tests.TestEntities;

namespace EntityObserver.Tests.TestObservers
{
    public class InvalidMemberNameObserver : Observer<Person>
    {
        public InvalidMemberNameObserver(Person entity) : base(entity)
        {
        }

        public string First
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Last
        {
            get => GetValue<string>(string.Empty);
            set => SetValue(value);
        }
    }
}