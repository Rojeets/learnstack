namespace TechStackLearningHub.Web.Helpers
{
    /// <summary>
    /// Delivers a password-reset link.
    ///
    /// This exists so AuthBLL can hand off "tell this person this link" without
    /// knowing or caring how, and so the transport can be swapped or faked in a
    /// test without touching the token logic that actually has to be correct.
    ///
    /// Contract for implementors: SendResetLink either delivers the message or
    /// throws. It must never swallow a failure, because a silently dropped reset
    /// leaves a user waiting for mail that will not arrive. It must also never
    /// log or echo the reset URL, which is a live credential for the duration of
    /// the token's life.
    /// </summary>
    public interface IResetNotifier
    {
        void SendResetLink(string toEmail, string displayName, string resetUrl);
    }
}
