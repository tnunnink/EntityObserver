using System.Collections.ObjectModel;
using System.Collections.Specialized;
using EntityObserver.Tests.TestEntities;

namespace EntityObserver.Tests.TestObservers
{
    /// <summary>
    /// An example of an observer that overrides getters, setters, collection syncs in order to test an observer that
    /// allows for customization
    /// </summary>
    public class CustomPersonObserver : Observer<Person>
    {
        public CustomPersonObserver(Person entity) : base(entity)
        {
            InitializeObservers();
            
            //SynchronizeCollections(Emails, OnCarsChanged);
        }

        private void OnCarsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
                
        }

        public int Id
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        public string FirstName
        {
            get => GetValue<string>();
            set => SetValue(value, getter: m => m.FirstName, setter: (p, s) => p.Rename(s, LastName));
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
        
        public AddressObserver Address { get; }

        public ObservableCollection<string> Emails { get; }

        public ObserverCollection<CarObserver> Cars { get; }
    }
}