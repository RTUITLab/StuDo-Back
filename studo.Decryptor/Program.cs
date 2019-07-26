using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace studo.Decryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("You need to pass the path from where to take a file!");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File doesn't exist");
                Console.WriteLine(args[0]);
                return;
            }

            var path = args[0];
            byte[] key = null; // TODO: create secret file with content - key

            var newPath = path;
            var name = Path.GetFileName(newPath);
            var name2 = name.Split('.');
            newPath = newPath.Replace(name, name2[0] + " (1)." + name2[1]);

            using (var sourceStream = File.OpenRead(path))
            using (var destinationStream = File.Create(newPath))
            using (var provider = new AesCryptoServiceProvider())
            {
                var IV = new byte[provider.IV.Length];
                sourceStream.Read(IV, 0, IV.Length);
                using (var cryptoTransform = provider.CreateDecryptor(key, IV))
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
