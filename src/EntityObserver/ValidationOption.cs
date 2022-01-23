namespace EntityObserver
{
    /// <summary>
    /// Represents an enumeration of validation options that allow the user to specify how the validation should be performed.
    /// </summary>
    public enum ValidationOption
    {
        /// <summary>
        /// Specifies that the validation should only validate no properties.
        /// </summary>
        None,
        
        /// <summary>
        /// Specifies that the validation should validate all properties.
        /// </summary>
        All,
        
        /// <summary>
        /// Specifies that the validation should only validate required properties.
        /// </summary>
        Required
    }
}