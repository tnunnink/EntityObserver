using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EntityObserver
{
    /// <summary>
    /// Defines a functionality for an observable entity wrapper.
    /// This interface aggregates notification, change tracking, and validation into a single implementation.
    /// </summary>
    public interface IObserver : INotifyPropertyChanged, INotifyDataErrorInfo, IRevertibleChangeTracking,
        IValidatableObject
    {
    }

    /// <summary>
    /// Provides support for change notification, change tracking, and validation for entity objects.
    /// </summary>
    /// <typeparam name="TEntity">The entity object type that the observer is wrapping.</typeparam>
    /// <remarks>
    /// This interface is the aggregate for basic UX related functionality such as change notification, change tracking,
    /// and validation of data  
    /// </remarks>
    public interface IObserver<out TEntity> : IObserver where TEntity : class
    {
        /// <summary>
        /// Gets the model object under observation.  
        /// </summary>
        TEntity Entity { get; }
    }
}