using System.Data.SqlClient;
using TechStackLearningHub.Web.BLL;

namespace TechStackLearningHub.Web.Helpers
{
    /// <summary>
    /// Turns a raw SqlException into a friendly ValidationException, or returns
    /// null when the error isn't one of the recognised constraint violations -
    /// in which case the caller should log it and show a generic message.
    ///
    /// Usage in every BLL write method:
    /// <code>
    /// catch (SqlException sqlEx)
    /// {
    ///     var vex = SqlErrorHelper.Translate(sqlEx, "That email is already registered.");
    ///     if (vex != null) throw vex;
    ///     ErrorLogger.Log(sqlEx, "UserBLL.Register");
    ///     throw new ValidationException("A database error occurred. Please try again.");
    /// }
    /// </code>
    ///
    /// A raw SqlException reaching a page means the visitor sees a stack trace,
    /// a connection string fragment and the exact name of an internal index.
    /// Recognising the handful of constraint errors that are genuinely the
    /// USER's fault lets those be re-worded; everything else stays opaque.
    ///
    /// This returns a BLL type, so the dependency runs Helpers -> BLL. That is
    /// deliberate: the translation is part of the service contract pages
    /// consume, and the BLL remains the layer that decides a constraint
    /// violation is user-facing rather than a system fault.
    /// </summary>
    public static class SqlErrorHelper
    {
        private const int UniqueViolation1 = 2627;   // PRIMARY KEY / UNIQUE constraint
        private const int UniqueViolation2 = 2601;   // duplicate key on a unique index
        private const int ForeignKeyViolation = 547; // FK constraint (insert/update OR delete)

        public static ValidationException Translate(
            SqlException ex, string uniqueMessage = null, string fkMessage = null)
        {
            if (ex == null) return null;

            foreach (int number in Numbers(ex))
            {
                if (number == UniqueViolation1 || number == UniqueViolation2)
                    return new ValidationException(uniqueMessage ?? "That value is already in use.");

                if (number == ForeignKeyViolation)
                    return new ValidationException(fkMessage ??
                        "This action conflicts with related data and cannot be completed.");
            }

            // Not a recognised constraint violation - caller logs and shows a
            // generic message.
            return null;
        }

        /// <summary>
        /// Collects every error number on the exception. SqlException.Number
        /// only surfaces the first one, so a batch can report 2627 and 547
        /// together and checking .Number alone would miss the second.
        /// </summary>
        private static System.Collections.Generic.IEnumerable<int> Numbers(SqlException ex)
        {
            if (ex.Errors == null)
            {
                yield return ex.Number;
                yield break;
            }

            foreach (SqlError error in ex.Errors)
                yield return error.Number;
        }
    }
}
