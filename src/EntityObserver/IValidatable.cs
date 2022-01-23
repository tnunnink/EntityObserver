namespace EntityObserver
{
    /// <summary>
    /// 
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Gets a value indicating whether the object is valid.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Performs validation ...
        /// </summary>
        /// <param name="validationOption"></param>
        void Validate(ValidationOption validationOption);
    }
}