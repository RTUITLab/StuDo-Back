namespace studo.Decryptor
{
    class DecryptorOptions
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }

        public DecryptorOptions(string fileName)
        {
            string[] lines = System.IO.File.ReadAllLines(fileName);

            Key = System.Convert.FromBase64String(lines[0]);
            IV = System.Convert.FromBase64String(lines[1]);
        }
    }
}
