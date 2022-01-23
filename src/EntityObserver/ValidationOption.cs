namespace EntityObserver
{
    /// <summary>
    /// Represents a set of options that controls how validation is performed.
    /// </summary>
    public enum ValidationOption
    {
        /// <summary>
        /// Perform no validation.
        /// </summary>
        None,
        
        /// <summary>
        /// Performed validation on the entire object.
        /// </summary>
        Object,
        
        /// <summary>
        /// Perform validation of a single property
        /// </summary>
        Property,
        
        /// <summary>
        /// Perform validation of required fields only.
        /// </summary>
        Required
    }
}