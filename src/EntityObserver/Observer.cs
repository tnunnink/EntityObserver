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
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class Observer<TEntity> : Notifiable, IObserver<TEntity> where TEntity : class
    {
        private readonly Dictionary<string, object> _originalValues = new();
        private readonly List<IObserver> _observers = new();

        /// <summary>
        /// Creates a new instance of the <see cref="Observer{TModel}"/> wrapper with the provided model class.
        /// </summary>
        /// <param name="model">A class that represents the model of the current observable wrapper.</param>
        /// <exception cref="ArgumentNullException">model is null.</exception>
        protected Observer(TEntity model)
        {
            Entity = model ?? throw new ArgumentNullException(nameof(model), "Model cannot be null");
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
/// <typeparam name="TValue">The type of the value to retrieve from the underlying model.</typeparam>peparam>
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
        /// <param name="validationOption"></param/// <typeparam name="TValue">The type of the value that is being set on the model object.</typeparam>typeparam>
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

            //var current = (TValue)propertyInfo.GetValue(Model)!;

            if (EqualityComparer<TValue>.Default.Equals(current, value)) return false;

            UpdateOriginalValue(current, value, propertyInfo.Name);

            setter ??= (m, v) => propertyInfo.SetValue(m, v);
            setter.Invoke(Entity, value);
            
            onChanged?.Invoke();
            
            ValidateProperty(propertyInfo.Name, value);

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

            
            //todo do we actually need to do this?
            ValidateObject();
            RaisePropertyChanged(string.Empty);
        }

        /// <summary>
        /// Gets the original value of the specified property determined using the provided member expression.
        /// </summary>
        /// <param name="propertySelector">A member expression that selects a property of the current model.</p/// <typeparam name="TValue">The type property for which to get the original value for.</typeparam>r.</typeparam>
        /// <returns>
        /// The value of the model properties original value before it was change.
        /// If it has not been changed, then the current value of the property.
        /// </returns>
        /// <exception cref="ArgumentException">propertySelect is not of type <see cref="MemberExpression"/>.</exception>
        public TValue GetOriginalValue<TValue>(Expression<Func<TEntity, TValue>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Property selector must be of type {typeof(MemberExpression)}.");

            var propertyName = memberExpression.Member.Name;

            return _originalValues.ContainsKey(propertyName)
                ? (TValue)_originalValues[propertyName]
                : GetValue<TValue>(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertySelector"><//// <typeparam name="TProperty"></typeparam>y"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool GetIsChanged<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Property selector must be of type {typeof(MemberExpression)}.");

            var propertyName = memberExpression.Member.Name;

            return _originalValues.ContainsKey(propertyName);
        }

        /// <inheritdoc />
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wrapperCollection"></param>
        /// <param name="modelCollection"></// <typeparam name="TObserver"></typeparam>er"></typ/// <typeparam name="TMember"></typeparam>er"></typeparam>
        protected void RegisterCollection<TObserver, TMember>(
            ObserverCollection<TObserver> wrapperCollection,
            ICollection<TMember> modelCollection)
            where TObserver : class, IObserver<TMember>
            where TMember : class
        {
            wrapperCollection.CollectionChanged += (_, e) =>
            {
                if (wrapperCollection.Count > 0)
                {
                    if (e.OldItems is not null)
                        foreach (var item in e.OldItems.Cast<TObserver>())
                            modelCollection.Remove(item.Entity);

                    if (e.NewItems is null) return;

                    foreach (var item in e.NewItems.Cast<TObserver>())
                        modelCollection.Add(item.Entity);
                }
                else
                {
                    modelCollection.Clear();
                }

                ValidateObject();
            };
        }

        /// <summary>
        /// Registers a collection changed event handler to the provided observable collection so the the user can
        /// perform custom synchronization logic when an <see cref="Observer{TModel}"/> member collection is updated. 
        /// </summary>
        /// <param name="collection">The wrapper collection to attach the collection change event handler to.</param>
        /// <param name="onCollectionChanged">
        /// An <see cref="NotifyCollectionChangedEventHandler"/> to execute when the collection items are added or removed.
        ////// <typeparam name="TObservable">The type of model that the </typeparam>t the </typeparam>
        protected void RegisterCollection<TObservable>(ObservableCollection<TObservable> collection,
            NotifyCollectionChangedEventHandler onCollectionChanged)
        {
            collection.CollectionChanged += onCollectionChanged;
            collection.CollectionChanged += (_, _) => ValidateObject();
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
            var model = GetModelProperty(propertyInfo.Name).GetValue(Entity);

            if (model is null) return null; 

            return Activator.CreateInstance(propertyInfo.PropertyType, model) as IObserver;
        }
        
        /// <summary>
        /// Creates a new <see cref="ObserverCollection{TObserver}"/> instance using the provided property type.
        /// </summary>
        /// <param name="propertyInfo">The type that represents the <see cref="IObserver"/> to instantiate.</param>
        /// <returns>A new instance of a <see cref="ObserverCollection{TObserver}"/> wrapping the underlying model collection.</returns>
        private IObserver? InstantiateObserverCollection(PropertyInfo propertyInfo)
        {
            var modelCollection = GetModelProperty(propertyInfo.Name).GetValue(Entity);

            if (modelCollection is not ICollection collection) return null;

            var observerType = propertyInfo.PropertyType.GetGenericArguments()[0];
            var observers = observerType.CreateList();
            
            foreach (var item in collection)
            {
                var instance = Activator.CreateInstance(observerType, item) as IObserver;
                observers.Add(instance);
            }

            return Activator.CreateInstance(propertyInfo.PropertyType, observers) as IObserver;
        }

        
        private void UpdateOriginalValue(object current, object value, string propertyName)
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
                throw new ArgumentException($"Could not retrieve property info for {propertyName}");

            return propertyInfo;
        }
    }
}