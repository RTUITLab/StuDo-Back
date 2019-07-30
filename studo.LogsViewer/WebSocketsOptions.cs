using System;
using System.IO;

namespace studo.LogsViewer
{
    class WebSocketsOptions
    {
        public string SecretKey { get; }
        public string Url { get; }

        public WebSocketsOptions(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);

            SecretKey = lines[0];
            // if in product use product url
            if (lines[1] == "true")
                Url = lines[2];
            else if (lines[1] == "false")
                Url = lines[3];
            else
            {
                Url = null;
                throw new Exception($"Second line in '{fileName}' isn't set to \"true\" or \"false\"");
            }
        }
    }
}
