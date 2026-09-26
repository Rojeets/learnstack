using System.Data.SqlClient;

namespace TechStackLearningHub.Helpers
{
    /// <summary>
    /// Turns SQL Server constraint violations into a message that is safe to
    /// show a user.
    ///
    /// A raw SqlException reaching a page means the visitor sees a stack trace,
    /// a connection string fragment and the exact name of an internal index.
    /// The BLL catches SqlException, asks this class whether it recognises the
    /// error, and either rethrows it as a ValidationException with friendly
    /// text or logs it and shows a generic message.
    ///
    /// Returns a message string on a recognised constraint failure, or null
    /// when the error is NOT one we have wording for - null means "log this,
    /// do not show it".
    ///
    /// This lives in Helpers and returns a string rather than throwing a BLL
    /// type, so the dependency direction stays Helpers <- BLL. The BLL is the
    /// layer that decides to turn the string into a ValidationException.
    /// </summary>
    public static class SqlErrorHelper
    {
        public static string Translate(SqlException ex, string uniqueMessage, string foreignKeyMessage)
        {
            if (ex == null) return null;
            if (ex.Errors == null) return null;

            foreach (SqlError error in ex.Errors)
            {
                switch (error.Number)
                {
                    // UNIQUE constraint violation
                    case 2627:
                        return uniqueMessage;

                    // Cannot insert duplicate key row in unique index
                    case 2601:
                        return uniqueMessage;

                    // FOREIGN KEY constraint violation
                    case 547:
                        return foreignKeyMessage;
                }
            }

            return null;
        }
    }
}
