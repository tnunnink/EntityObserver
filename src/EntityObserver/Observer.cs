using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EntityObserver
{
    /// <summary>
    /// An entity wrapper that adds change notification, change tracking, and validation for use in Windows applications. 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity or model that the observer is wrapping.</typeparam>
    public abstract class Observer<TEntity> : ValidationNotifier, IObserver<TEntity> where TEntity : class
    {
        private readonly Dictionary<string, object?> _originalValues = new();
        private readonly List<IObserver> _observers = new();

        /// <summary>
        /// Creates a new instance of the <see cref="Observer{TModel}"/> wrapper with the provided entity class.
        /// </summary>
        /// <param name="entity">A class that represents the model of the current observable wrapper.</param>
        /// <exception cref="ArgumentNullException">model is null.</exception>
        protected Observer(TEntity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity), "Model cannot be null");
        }

        /// <inheritdoc />
        public TEntity Entity { get; }

        /// <inheritdoc />
        public bool IsChanged => _originalValues.Count > 0 || _observers.Any(o => o.IsChanged);

        /// <inheritdoc />
        public override bool HasErrors => base.HasErrors || _observers.Any(o => o.HasErrors);

        /// <summary>
        /// Gets the value of the specified property from the underlying model property.
        /// </summary>
        /// <param name="propertyName">The name of the property to get the value for.</param>
        /// <param name="getter">An optional getting <see cref="Func{TResult}"/> delegate that specifies how to get
        /// the value of the underlying model property. If not provided, the method will use reflection.</param>
        /// <typeparam name="TValue">The type of the value to retrieve from the underlying model.</typeparam>
        /// <returns>The value of the specified model property.</returns>
        /// <exception cref="ArgumentException">
        /// propertyName is null or empty -or- the specified name does not exist on the underlying model object.
        /// </exception>
        /// <remarks>
        /// This method uses reflection to get the value of the underlying model property. This can be bypassed using
        /// the getter delegate. Use this method to retrieve the value of the underlying model properties.
        /// </remarks>
        protected TValue GetValue<TValue>([CallerMemberName] string? propertyName = null,
            Func<TEntity, TValue>? getter = null)
        {
            getter ??= m => (TValue)GetModelProperty(propertyName).GetValue(m)!;

            return getter.Invoke(Entity);
        }

        /// <summary>
        /// Sets the value of the specified property on the underlying model object with the provided value. 
        /// </summary>
        /// <param name="value">The new value to set the property with.</param>
        /// <param name="propertyName">The name of the property to set the value for.
        /// This parameter is optional and if not provided will get the name from the <see cref="CallerMemberNameAttribute"/>.
        /// </param>
        /// <param name="getter">
        /// </param>
        /// <param name="setter">
        /// An optional setter delegate that can be provided to bypass using reflection to set the underlying model property.
        /// </param>
        /// <param name="onChanged">
        /// An optional action delegate that represents an action to be performed immediately after the value has been changed.
        /// </param>
        /// <param name="validationOption">.</param>
        /// <typeparam name="TValue">The type of the value that is being set on the model object.</typeparam>
        /// <returns>true if the provided value was set on the model object; otherwise, false.</returns>
        protected bool SetValue<TValue>(TValue value,
            [CallerMemberName] string? propertyName = null,
            Func<TEntity, TValue>? getter = null,
            Action<TEntity, TValue>? setter = null,
            Action? onChanged = null,
            ValidationOption? validationOption = null)
        {
            var propertyInfo = GetModelProperty(propertyName);

            getter ??= m => (TValue)GetModelProperty(propertyName).GetValue(m)!;
            var current = getter.Invoke(Entity);

            if (EqualityComparer<TValue>.Default.Equals(current, value)) return false;

            UpdateOriginalValue(current, value, propertyInfo.Name);

            setter ??= (m, v) => propertyInfo.SetValue(m, v);
            setter.Invoke(Entity, value);

            onChanged?.Invoke();

            if (validationOption is null)
                Validate(propertyInfo.Name, value);
            else
                Validate(validationOption);

            RaisePropertyChanged(propertyName);
            RaisePropertyChanged(nameof(IsChanged));
            return true;
        }

        /// <inheritdoc />
        public void AcceptChanges()
        {
            _originalValues.Clear();

            foreach (var observer in _observers)
                observer.AcceptChanges();

            RaisePropertyChanged(string.Empty);
        }

        /// <inheritdoc />
        public void RejectChanges()
        {
            foreach (var (key, value) in _originalValues)
                SetValue(value, key);

            _originalValues.Clear();

            foreach (var observer in _observers)
                observer.RejectChanges();

            Validate();
            RaisePropertyChanged(string.Empty);
        }

        /// <summary>
        /// Gets the original value of the specified property.
        /// </summary>
        /// <param name="propertySelector">
        /// An expression that selects a specific property of the <see cref="Observer{TEntity}"/>.
        /// This expression must be of type <see cref="MemberExpression"/>.
        /// </param>
        /// <typeparam name="TProperty">The type of property to select.</typeparam>
        /// <returns>The original value of the selected property if it has pending changes; otherwise, the current value.</returns>
        /// <exception cref="ArgumentException">propertySelector is not a <see cref="MemberExpression"/>.</exception>
        public TProperty GetOriginalValue<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Property selector must be of type {typeof(MemberExpression)}.");

            var propertyName = memberExpression.Member.Name;

            return _originalValues.ContainsKey(propertyName)
                ? (TProperty)_originalValues[propertyName]!
                : GetValue<TProperty>(propertyName);
        }

        /// <summary>
        /// Gets a value indicating whether the specified property has pending changes.
        /// </summary>
        /// <param name="propertySelector">
        /// An expression that selects a specific property of the <see cref="Observer{TEntity}"/>.
        /// This expression must be of type <see cref="MemberExpression"/>.
        /// </param>
        /// <typeparam name="TProperty">The type of property to select.</typeparam>
        /// <returns>ture if the specified property has pending changes; otherwise, false..</returns>
        /// <exception cref="ArgumentException">propertySelector is not a <see cref="MemberExpression"/>.</exception>
        public bool GetIsChanged<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Property selector must be of type {typeof(MemberExpression)}.");

            var propertyName = memberExpression.Member.Name;

            return _originalValues.ContainsKey(propertyName);
        }

        /// <summary>
        /// Gets a collection of errors for the specified property using the provided property selector expression
        /// </summary>
        /// <param name="propertySelector">
        /// An expression that selects a specific property of the <see cref="Observer{TEntity}"/>.
        /// This expression must be of type <see cref="MemberExpression"/>.
        /// </param>
        /// <typeparam name="TProperty">The type of property to select.</typeparam>
        /// <returns>A collection of errors for the specified property if any exist; otherwise, and empty collection.</returns>
        /// <exception cref="ArgumentException">propertySelector is not a <see cref="MemberExpression"/>.</exception>
        public IEnumerable<string> GetErrors<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Property selector must be of type {typeof(MemberExpression)}.");

            var propertyName = memberExpression.Member.Name;

            return GetErrors(propertyName) as IEnumerable<string> ?? Array.Empty<string>();
        }

        /// <summary>
        /// Performs validation on the specified property of the current <see cref="Observer{TEntity}"/>.
        /// </summary>
        /// <param name="propertySelector">
        /// An expression that selects a specific property of the <see cref="Observer{TEntity}"/>.
        /// This expression must be of type <see cref="MemberExpression"/>.
        /// </param>
        /// <typeparam name="TProperty">The type of property to select.</typeparam>
        /// <exception cref="ArgumentException">propertySelector is not a <see cref="MemberExpression"/>.</exception>
        public void Validate<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Property selector must be of type {typeof(MemberExpression)}.");

            var propertyName = memberExpression.Member.Name;
            var value = GetValue<TProperty>(propertyName);

            Validate(propertyName, value);
        }

        /// <inheritdoc />
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }

        /// <summary>
        /// Provides the ability for an observable collection to update its underlying entity collection when items
        /// in the collection change. This prevents the user from having to manually keep collections in sync. 
        /// </summary>
        /// <param name="observableCollection">An observable collection to monitor for changes.</param>
        /// <param name="entityCollection">The underlying entity collection that should be kept in sync.</param>
        /// <typeparam name="TObserver">The type of </typeparam>
        /// <typeparam name="TMember"></typeparam>
        /// <remarks>
        /// This is the standard implementation which assumes that the underlying entity collection is a simple mutable
        /// collection where items can be added or removed without any additional considerations. For entities or domain
        /// classes with more complex logic for adding and removing items, the user can specify a custom synchronization
        /// event handler using the alternate overload. Otherwise, the user can handle synchronization logix manually.
        /// </remarks>
        /// <seealso cref="SynchronizeCollections{TObserver}"/>
        protected void SynchronizeCollections<TObserver, TMember>(
            ObservableCollection<TObserver> observableCollection,
            ICollection<TMember> entityCollection)
            where TObserver : class, IObserver<TMember>
            where TMember : class
        {
            observableCollection.CollectionChanged += (_, e) =>
            {
                if (observableCollection.Count > 0)
                {
                    if (e.OldItems is not null)
                        foreach (var item in e.OldItems.Cast<TObserver>())
                            entityCollection.Remove(item.Entity);

                    if (e.NewItems is null) return;

                    foreach (var item in e.NewItems.Cast<TObserver>())
                        entityCollection.Add(item.Entity);
                }
                else
                {
                    entityCollection.Clear();
                }
                
                Validate();
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="onCollectionChanged"></param>
        /// <typeparam name="TObserver"></typeparam>
        protected void SynchronizeCollections<TObserver>(ObservableCollection<TObserver> collection,
            NotifyCollectionChangedEventHandler onCollectionChanged)
            where TObserver : class, IObserver
        {
            collection.CollectionChanged += onCollectionChanged;
            collection.CollectionChanged += (_, _) => Validate();
        }

        /// <summary>
        /// Registers the provided <see cref="IObserver"/> to the current <see cref="Observer{TModel}"/> object.
        /// </summary>
        /// <param name="observer">The <see cref="IObserver"/> instance to register.</param>
        /// <exception cref="ArgumentNullException">observer is null.</exception>
        /// <remarks>
        /// Registering an <see cref="IObserver"/> instance will hook up the property change event handler so that the
        /// current <see cref="Observer{TModel}"/> object will get notifications when child complex or collection
        /// properties are changed or have validation errors.
        /// </remarks>
        /// <seealso cref="InitializeObservers"/>
        protected void RegisterObserver(IObserver observer)
        {
            if (observer is null)
                throw new ArgumentNullException(nameof(observer));

            if (_observers.Contains(observer)) return;

            _observers.Add(observer);

            observer.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(IsChanged) or nameof(HasErrors))
                    RaisePropertyChanged(e.PropertyName);
            };
        }

        /// <summary>
        /// Instantiates and registers all observable properties of the current <see cref="Observer{TModel}"/> object. 
        /// </summary>
        /// <remarks>
        /// This method uses reflection to find <see cref="IObservable{T}"/> member properties of the current
        /// <see cref="Observer{TModel}"/> and attempts to instantiate and register them for tracking. This allows the
        /// user to simply call a single initialize method instead of performing initialization and registration manually
        /// in the constructor of the <see cref="Observer{TModel}"/>. 
        /// </remarks>
        protected void InitializeObservers()
        {
            var properties = GetType().GetProperties();

            foreach (var property in properties.Where(p => p.PropertyType.IsObserver()))
            {
                var observer = property.PropertyType.IsObserverCollection()
                    ? InstantiateObserverCollection(property)
                    : InstantiateObserver(property);

                property.SetValue(this, observer);

                if (observer is not null)
                    RegisterObserver(observer);
            }
        }

        /// <summary>
        /// Creates a new <see cref="IObserver"/> instance using the provided property type.
        /// </summary>
        /// <param name="propertyInfo">The type that represents the <see cref="IObserver"/> to instantiate.</param>
        /// <returns>A new instance of a <see cref="IObserver"/> wrapping the underlying model property.</returns>
        private IObserver? InstantiateObserver(PropertyInfo propertyInfo)
        {
            var property = GetModelProperty(propertyInfo.Name).GetValue(Entity);

            if (property is null) return null;

            return Activator.CreateInstance(propertyInfo.PropertyType, property) as IObserver;
        }

        /// <summary>
        /// Creates a new <see cref="ObserverCollection{TObserver}"/> instance using the provided property type.
        /// </summary>
        /// <param name="propertyInfo">The type that represents the <see cref="IObserver"/> to instantiate.</param>
        /// <returns>A new instance of a <see cref="ObserverCollection{TObserver}"/> wrapping the underlying model collection.</returns>
        private IObserver? InstantiateObserverCollection(PropertyInfo propertyInfo)
        {
            var property = GetModelProperty(propertyInfo.Name).GetValue(Entity);

            if (property is not IEnumerable enumerable) return null;

            var observerType = propertyInfo.PropertyType.GetGenericArguments()[0];
            var observers = observerType.CreateList();

            foreach (var item in enumerable)
            {
                var instance = Activator.CreateInstance(observerType, item) as IObserver;
                observers.Add(instance);
            }

            return Activator.CreateInstance(propertyInfo.PropertyType, observers) as IObserver;
        }


        private void UpdateOriginalValue(object? current, object? value, string propertyName)
        {
            if (!_originalValues.ContainsKey(propertyName))
            {
                _originalValues.Add(propertyName, current);
                return;
            }

            if (!Equals(_originalValues[propertyName], value)) return;

            _originalValues.Remove(propertyName);
        }

        private PropertyInfo GetModelProperty(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Property name can not be null or empty");

            var propertyInfo = Entity.GetType().GetProperty(propertyName);

            if (propertyInfo is null)
                throw new ArgumentException($"Could not find property info for {propertyName}");

            return propertyInfo;
        }
    }
}