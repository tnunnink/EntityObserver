namespace EntityObserver
{
    /// <summary>
    /// 
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Gets a value indicating whether there are any validation errors.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Performs validation on the current object.
        /// </summary>
        /// <param name="requiredOnly">Flag indicating whether to validate all attributes or just required field attributes.
        /// By default this is set to false which will perform object validation.</param>
        void Validate(bool requiredOnly = false);

        /// <summary>
        /// Performs validation on the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <param name="value">The value of the property to validate against.</param>
        void Validate(string propertyName, object? value);
    }
}