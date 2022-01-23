using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EntityObserver
{
    /// <summary>
    /// A base implementation for the <see cref="INotifyPropertyChanged"/> and <see cref="INotifyDataErrorInfo"/>.
    /// </summary>
    public abstract class Notifiable : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors;

        /// <summary>
        /// Creates a new instance of a <see cref="Notifiable"/> base class.
        /// </summary>
        protected Notifiable()
        {
            _errors = new Dictionary<string, List<string>>();
        }

        /// <inheritdoc />
        public virtual bool HasErrors => _errors.Any();

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc />
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <inheritdoc />
        public virtual IEnumerable GetErrors(string? propertyName)
        {
            return propertyName is not null && _errors.ContainsKey(propertyName)
                ? _errors[propertyName]
                : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the name of the provided property.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property to raise the value change notification for.
        /// This value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.
        /// </param>
        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Performs validation on the current <see cref="Notifiable"/> object using the specified validation option.
        /// </summary>
        /// <param name="option">The <see cref="ValidationOption"/> that specifies which validation to perform.</param>
        /// <param name="propertyName">The optional property name to validate. Must be provided to perform property validation.</param>
        /// <param name="value">The optional value used for property validation. Must be provided to perform property validation.</param>
        protected void Validate(ValidationOption? option, string? propertyName = null, object? value = null)
        {
            switch (option)
            {
                case ValidationOption.None:
                    return;
                case ValidationOption.Object:
                    ValidateObject();
                    break;
                case ValidationOption.Property:
                    ValidateProperty(propertyName!, value!);
                    break;
                case ValidationOption.Required:
                    ValidateRequired();
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Performs validation on the current object using the <see cref="Validator"/> class.
        /// </summary>
        protected void ValidateObject()
        {
            if (HasErrors)
            {
                ClearErrors();
                RaisePropertyChanged(nameof(HasErrors));
            }

            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            Validator.TryValidateObject(this, context, results, true);

            UpdateErrors(results);
        }

        /// <summary>
        /// Performs validation on the current object using the <see cref="Validator"/> class.
        /// </summary>
        private void ValidateRequired()
        {
            //todo how would we clear required errors first?

            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            Validator.TryValidateObject(this, context, results, false);

            UpdateErrors(results);
        }

        /// <summary>
        /// Performs validation on the specified property with the provided new property value.
        /// </summary>
        protected void ValidateProperty(string propertyName, object value)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Property name can not be null or empty for property validation");

            if (value is null)
                throw new ArgumentNullException(nameof(value));
            
            if (_errors.ContainsKey(propertyName))
            {
                ClearError(propertyName);
                RaisePropertyChanged(nameof(HasErrors));
            }

            var results = new List<ValidationResult>();
            var context = new ValidationContext(this) { MemberName = propertyName };
            Validator.TryValidateProperty(value, context, results);

            UpdateErrors(results);
        }

        /// <summary>
        /// Raises the <see cref="ErrorsChanged"/> event for the name of the provided property.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property to raise the error change notification for.
        /// This value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.
        /// </param>
        private void RaiseErrorsChanged([CallerMemberName] string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Updates the errors collection with the validation results.
        /// </summary>
        /// <param name="results">The collection of validation results to process.</param>
        private void UpdateErrors(IReadOnlyCollection<ValidationResult> results)
        {
            if (!results.Any()) return;

            var propertyNames = results.SelectMany(r => r.MemberNames).Distinct().ToList();

            foreach (var propertyName in propertyNames)
            {
                var errors = results.Where(r => r.MemberNames.Contains(propertyName))
                    .Select(r => r.ErrorMessage).Distinct().ToList();
                
                _errors[propertyName] = errors;

                RaiseErrorsChanged(propertyName);
            }

            RaisePropertyChanged(nameof(HasErrors));
        }

        /// <summary>
        /// Clears all validation errors from the errors collection of the current object.
        /// </summary>
        private void ClearErrors()
        {
            foreach (var propertyName in _errors.Keys.ToList())
            {
                _errors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }
        
        /// <summary>
        /// Clears validation errors for the specified property from the errors collection of the current object.
        /// </summary>
        private void ClearError(string propertyName)
        {
            _errors.Remove(propertyName);
            RaiseErrorsChanged(propertyName);
        }
    }
}