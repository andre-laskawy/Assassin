///-----------------------------------------------------------------
///   File:     IContext.cs
///   Author:   Andre Laskawy           
///   Date:	    30.03.2018 16:47:31
///   Revision History: 
///   Name:  Andre Laskawy         Date:  30.03.2018 16:47:31      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Data.Base
{
    /// <summary>
    /// Defines the <see cref="IContext" />
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Ensures the context
        /// </summary>
        void Ensure();
    }
}
