using System.ComponentModel;

namespace wpf.ModelWrapper
{
    public interface IValidatableChangeTracking : IChangeTracking
    {
        bool IsValid { get; }
    }
}