using System.Collections.ObjectModel;
using EntityObserver.Tests.TestEntities;

namespace EntityObserver.Tests.TestObservers
{
    public class SynchronizingPersonObserver : Observer<Person>
    {
        public SynchronizingPersonObserver(Person entity) : base(entity)
        {
            InitializeObservers();

            SynchronizeCollections(Cars, entity.Cars);
        }
        
        
        public int Id
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        public string FirstName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string LastName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public int Age
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        public float Weight
        {
            get => GetValue<float>();
            set => SetValue(value);
        }

        public float Height
        {
            get => GetValue<float>();
            set => SetValue(value);
        }

        public bool IsEmployed
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
        
        public AddressObserver Address { get; set; }

        public ObservableCollection<string> Emails { get; set; }

        public ObserverCollection<CarObserver> Cars { get; set; }
    }
}