using System;
using System.IO;
using System.Text;
using BartN.Domain;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace BartN
{
    public sealed class AesOfbCryptoContext : CryptoContext
    {
        private AesOfbCryptoContext(string password, byte[] salt)
            : base(password, salt)
        { }

        public static CryptoContext ForPassword(string password, Settings settings)
        {
            byte[] salt = settings.Salt.ToByteArray();

            return new AesOfbCryptoContext(password, salt);
        }

        public override Stream DecryptStream(Stream sourceStream)
        {
            Span<byte> iv = stackalloc byte[16];
            if (iv.Length != sourceStream.Read(iv))
            {
                throw new InvalidDataException();
            }

            CipherKeyGenerator aes = new CipherKeyGenerator();

            aes.Init(new KeyGenerationParameters(new SecureRandom(), 256));
            KeyParameter keyParam = ParameterUtilities.CreateKeyParameter("AES", Key);
            ParametersWithIV ivKeyParam = new ParametersWithIV(keyParam, iv.ToArray());
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/OFB/NoPadding");
            cipher.Init(false, ivKeyParam);

            return new CipherStream(sourceStream, cipher, null);
        }

        protected override void DeriveKey()
        {
            Key = Org.BouncyCastle.Crypto.Generators.SCrypt.Generate(
                Encoding.UTF8.GetBytes(Password), Salt, 1 << 18, 8, 1, 32);
        }
    }
}
