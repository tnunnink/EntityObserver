using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace wpf.ModelWrapper
{
    public abstract class ObservableErrorInfo : Observable, INotifyDataErrorInfo
    {
        protected ObservableErrorInfo()
        {
            Errors = new Dictionary<string, List<string>>();
        }

        protected readonly Dictionary<string, List<string>> Errors;

        /// <summary>
        /// Gets the value indicating whether the current model wrapper has errors.
        /// </summary>
        public bool HasErrors => Errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return propertyName != null && Errors.ContainsKey(propertyName)
                ? Errors[propertyName]
                : Enumerable.Empty<string>();
        }
        
        public IEnumerable<string> GetAllErrors()
        {
            return Errors.Any()
                ? Errors.SelectMany(x => x.Value)
                : Enumerable.Empty<string>();
        }

        protected void ClearErrors()
        {
            foreach (var propertyName in Errors.Keys.ToList())
            {
                Errors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }

        protected void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}