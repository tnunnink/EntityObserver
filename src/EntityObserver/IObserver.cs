using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EntityObserver
{
    /// <summary>
    /// Defines a functionality for an observable model objet. This interface aggregates notification, change tracking,
    /// and validation into a single implementation.
    /// </summary>
    public interface IObserver : INotifyPropertyChanged, INotifyDataErrorInfo, IRevertibleChangeTracking,
        IValidatableObject
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public interface IObserver<out TModel> : IObserver where TModel : class
    {
        /// <summary>
        /// Gets the model object under observation.  
        /// </summary>
        TModel Entity { get; }
    }
}