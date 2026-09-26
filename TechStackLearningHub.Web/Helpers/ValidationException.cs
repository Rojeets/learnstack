using System;
using System.Runtime.Serialization;

namespace TechStackLearningHub.Helpers
{
    /// <summary>
    /// A failure the user is allowed to see.
    ///
    /// This type exists to make the boundary between "safe to show" and "must
    /// be logged" impossible to blur. Every message thrown as a
    /// ValidationException is rendered straight onto the page, so those strings
    /// are written for a student. Anything else - a dead connection, a missing
    /// configuration value, a constraint violation - must NOT travel this way:
    /// it gets logged by ErrorLogger and replaced with a generic message.
    ///
    /// In other words: throwing this type is a promise that the text is
    /// deliberate, authored, and free of connection strings, SQL, or stack
    /// detail.
    /// </summary>
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationException(string message)
            : base(message)
        {
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // Deserialization constructor. Required by the [Serializable] contract
        // so the exception survives binary remoting and app-domain
        // round-trips; without it the type is not actually serializable.
        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
