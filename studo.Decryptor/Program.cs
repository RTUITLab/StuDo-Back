using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace studo.Decryptor
{
    class Program
    {
        private const string secretFilename = "Decryptor.Secret.txt";
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("You need to pass the path from where to take a file!");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File doesn't exist!");
                Console.WriteLine(args[0]);
                return;
            }

            var path = args[0];

            var name = Path.GetFileName(path);
            var name2 = name.Split('.');
            var newPath = path.Replace(name, name2[0] + " (1)." + name2[1]);

            using (var sourceStream = File.OpenRead(path))
            using (var destinationStream = File.Create(newPath))
            using (var provider = new AesCryptoServiceProvider())
            {
                var decryptorOptions = new DecryptorOptions(secretFilename);
                using (var cryptoTransform = provider.CreateDecryptor(decryptorOptions.Key, decryptorOptions.IV))
                using (var cryptoStream = new CryptoStream(sourceStream, cryptoTransform, CryptoStreamMode.Read))
                {
                    cryptoStream.CopyTo(destinationStream);
                }
            }

            Console.WriteLine($"Visit \"{newPath}\" to see result");
            Process.Start("notepad.exe", newPath);
        }
    }
}
