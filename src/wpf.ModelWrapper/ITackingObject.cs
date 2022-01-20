using System.ComponentModel;

namespace wpf.ModelWrapper
{
    public interface ITrackingObject : IRevertibleChangeTracking, IValidatableChangeTracking,
        IRequiredPropertyTracking, INotifyPropertyChanged
    {
    }
}