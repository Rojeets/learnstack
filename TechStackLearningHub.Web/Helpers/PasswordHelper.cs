using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace TechStackLearningHub.Helpers
{
    /// <summary>
    /// Password derivation. The ONLY place that touches password cryptography.
    ///
    /// PBKDF2-HMAC-SHA256 with 100,000 iterations. The constants below are
    /// FROZEN and must stay identical to the values documented in
    /// App_Data/TechStackLearningHub.sql: the seeded admin/student accounts are
    /// stored as base64 hashes derived with exactly these parameters. Change
    /// any of them and every existing account stops validating.
    ///
    /// Deliberately NOT a plain SHA-256 of password+salt: a fast hash is
    /// cheap to brute-force, and a per-user salt is what stops one stolen
    /// Users table from being precomputed against every account at once.
    /// </summary>
    public static class PasswordHelper
    {
        public const int PBKDF2Iterations = 100000;
        public const int SaltSizeBytes = 16;
        public const int HashSizeBytes = 32;

        /// <summary>16 cryptographically random bytes from the OS RNG.</summary>
        public static byte[] GenerateSalt()
        {
            var salt = new byte[SaltSizeBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Derives the hash for a password under a known salt. Note this is
        /// deterministic: same (password, salt) always yields the same bytes,
        /// which is why the salt is stored alongside the hash.
        /// </summary>
        public static byte[] DeriveHash(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, PBKDF2Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSizeBytes);
            }
        }

        public static string GenerateSaltBase64()
        {
            return Convert.ToBase64String(GenerateSalt());
        }

        public static string DeriveHashBase64(string password, byte[] salt)
        {
            return Convert.ToBase64String(DeriveHash(password, salt));
        }

        /// <summary>
        /// Constant-time comparison. A plain `==` on the hash bytes short
        /// circuits on the first differing byte, and the time it takes leaks
        /// how many leading bytes were guessed correctly - enough to recover a
        /// hash byte by byte.
        ///
        /// This is the hand-rolled equivalent of CryptographicOperations
        /// .FixedTimeEquals, which does not exist in .NET Framework 4.8. The
        /// NoInlining/NoOptimization attributes stop the JIT from reintroducing
        /// an early exit or a bounds-check short circuit.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null)
                return false;
            if (left.Length != right.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] - right[i];
            }
            return diff == 0;
        }
    }
}
