using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EntityObserver
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TObserver"></typeparam>
    /// <remarks>
    /// This collection is a ...
    /// </remarks>
    public class ObserverCollection<TObserver> : ObservableCollection<TObserver>, IObserver 
        where TObserver : class, IObserver
    {
        private IList<TObserver> _originalCollection;

        private readonly ObservableCollection<TObserver> _addedItems;
        private readonly ObservableCollection<TObserver> _removedItems;
        private readonly ObservableCollection<TObserver> _modifiedItems;

        /// <summary>
        /// Creates a new empty instance of <see cref="ObserverCollection{TObserver}"/>.
        /// </summary>
        public ObserverCollection()
        {
            _originalCollection = this.ToList();

            AttachPropertyChangeHandler(_originalCollection);

            _addedItems = new ObservableCollection<TObserver>();
            _removedItems = new ObservableCollection<TObserver>();
            _modifiedItems = new ObservableCollection<TObserver>();

            AddedItems = new ReadOnlyObservableCollection<TObserver>(_addedItems);
            RemovedItems = new ReadOnlyObservableCollection<TObserver>(_removedItems);
            ModifiedItems = new ReadOnlyObservableCollection<TObserver>(_modifiedItems);
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="ObserverCollection{TObserver}"/> containing the collection
        /// of provided <see cref="IObserver"/> object. 
        /// </summary>
        /// <param name="items">The collection of <see cref="IObserver"/> objects to initialize the collection with.</param>
        public ObserverCollection(IEnumerable<TObserver> items) : base(items)
        {
            _originalCollection = this.ToList();

            AttachPropertyChangeHandler(_originalCollection);

            _addedItems = new ObservableCollection<TObserver>();
            _removedItems = new ObservableCollection<TObserver>();
            _modifiedItems = new ObservableCollection<TObserver>();

            AddedItems = new ReadOnlyObservableCollection<TObserver>(_addedItems);
            RemovedItems = new ReadOnlyObservableCollection<TObserver>(_removedItems);
            ModifiedItems = new ReadOnlyObservableCollection<TObserver>(_modifiedItems);
        }

        /// <summary>
        /// Gets a collection of all added items to the current <see cref="ObserverCollection{TObserver}"/>.
        /// </summary>
        public ReadOnlyObservableCollection<TObserver> AddedItems { get; }

        /// <summary>
        /// Gets a collection of all removed items to the current <see cref="ObserverCollection{TObserver}"/>.
        /// </summary>
        public ReadOnlyObservableCollection<TObserver> RemovedItems { get; }

        /// <summary>
        /// Gets a collection of all modified items to the current <see cref="ObserverCollection{TObserver}"/>.
        /// </summary>
        public ReadOnlyObservableCollection<TObserver> ModifiedItems { get; }

        
        /// <inheritdoc />
        public bool IsChanged => AddedItems.Any() || RemovedItems.Any() || ModifiedItems.Any();

        /// <inheritdoc />
        public bool HasErrors => this.Any(t => t.HasErrors);

        /// <inheritdoc />
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <inheritdoc />
        public void AcceptChanges()
        {
            _addedItems.Clear();
            _modifiedItems.Clear();
            _removedItems.Clear();

            foreach (var item in this)
                item.AcceptChanges();

            _originalCollection = this.ToList();
            
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
        }

        /// <inheritdoc />
        public void RejectChanges()
        {
            foreach (var addedItem in _addedItems.ToList())
                Remove(addedItem);

            foreach (var removedItem in _removedItems.ToList())
                Add(removedItem);

            foreach (var modifiedItem in _modifiedItems.ToList())
                modifiedItem.RejectChanges();
            
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
        }

        /// <inheritdoc />
        public IEnumerable GetErrors(string? propertyName) => this.Select(o => o.GetErrors(propertyName));

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }

        /// <inheritdoc />
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var added = this.Where(current => _originalCollection.All(orig => orig != current)).ToList();
            var removed = _originalCollection.Where(orig => this.All(current => current != orig)).ToList();

            var modified = this.Except(added).Except(removed).Where(item => item.IsChanged).ToList();

            AttachPropertyChangeHandler(added);
            DetachPropertyChangeHandler(removed);

            UpdateCollection(_addedItems, added);
            UpdateCollection(_removedItems, removed);
            UpdateCollection(_modifiedItems, modified);

            base.OnCollectionChanged(e);
            RaisePropertyChanged(nameof(IsChanged));
            RaisePropertyChanged(nameof(HasErrors));
        }
        
        private void AttachPropertyChangeHandler(IEnumerable<TObserver> collection)
        {
            foreach (var item in collection)
            {
                item.PropertyChanged -= ItemPropertyChanged;
                item.PropertyChanged += ItemPropertyChanged;
            }
        }

        private void DetachPropertyChangeHandler(IEnumerable<TObserver> collection)
        {
            foreach (var item in collection)
                item.PropertyChanged -= ItemPropertyChanged;
        }
        
        private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HasErrors))
            {
                RaisePropertyChanged(nameof(HasErrors));
                return;
            }

            if (sender is not TObserver observer)
                return;

            if (_addedItems.Contains(observer)) return;

            if (observer.IsChanged)
            {
                if (!_modifiedItems.Contains(observer))
                    _modifiedItems.Add(observer);
            }
            else
            {
                if (_modifiedItems.Contains(observer))
                    _modifiedItems.Remove(observer);
            }

            RaisePropertyChanged(nameof(IsChanged));
        }

        
        private static void UpdateCollection(ICollection<TObserver> collection, List<TObserver> items)
        {
            collection.Clear();
            
            foreach (var item in items)
                collection.Add(item);
        }
        
        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}