///-----------------------------------------------------------------
///   File:     ExceptionExtension.cs
///   Author:   Andre Laskawy           
///   Date:	    21.01.2018 13:08:19
///   Revision History: 
///   Name:  Andre Laskawy         Date:  21.01.2018 13:08:19      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Common
{
    using System;

    /// <summary>
    /// Defines the <see cref="ExceptionExtension" />
    /// </summary>
    public static class ExceptionExtension
    {
        /// <summary>
        /// To text.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="code">The code.</param>
        /// <returns>
        /// The <see cref="string" /></returns>
        public static string ToText(this Exception ex, string code)
        {
            string message = code + ":" + ex.Message + Environment.NewLine;

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                message += "Inner exception: " + ex.Message + Environment.NewLine;
            }

            message += "Stacktrace: " + ex.StackTrace + Environment.NewLine;

            return message;
        }
    }
}
