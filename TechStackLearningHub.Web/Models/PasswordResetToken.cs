using System;

namespace TechStackLearningHub.Web.Models
{
    /// <summary>
    /// A single outstanding password-reset request.
    ///
    /// TokenHash holds SHA-256 of the token that was emailed, never the token
    /// itself. The token is 32 bytes of CSPRNG output, so it cannot be guessed,
    /// but a stolen copy of this table would otherwise hand an attacker every
    /// live reset link in the system. Hashing on the way in means a database
    /// leak yields nothing usable, the same reasoning as the per-user password
    /// salt in Helpers/PasswordHelper.
    ///
    /// Rows are not deleted on use: UsedAt records that the token was spent and
    /// ExpiresAt records when it would have died anyway, so a replay is refused
    /// for a reason that can be audited rather than one that looks like a typo.
    /// </summary>
    public class PasswordResetToken
    {
        public int TokenID { get; set; }
        public int UserID { get; set; }

        /// <summary>Base64 SHA-256 of the emailed token. Never the token.</summary>
        public string TokenHash { get; set; }

        public DateTime ExpiresAt { get; set; }

        /// <summary>Null while the token is still redeemable.</summary>
        public DateTime? UsedAt { get; set; }

        public DateTime CreatedDate { get; set; }

        /// <summary>True only while the token can still be redeemed.</summary>
        public bool IsRedeemable
        {
            get { return !UsedAt.HasValue && ExpiresAt > DateTime.Now; }
        }
    }
}
