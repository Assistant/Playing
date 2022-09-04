using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace MusicBeePlugin
{
    class HttpServer
    {
        public static HttpListener listener;
        public static int port;
        public static string host = "http://localhost";
        public static string artist = string.Empty;
        public static string title = string.Empty;
        public static bool runServer = true;
        public static bool runPending = false;
        public static bool isRunning = false;
        public static int requestCount = 0;
        public static string apiData = @"{{""title"":""{0}"",""artist"":""{1}""}}";
        public static string pageData = @"
<!DOCTYPE html>
<html>
  <head>
    <title>MusicBee song info</title>
    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
    <link href=""https://fonts.googleapis.com/css2?family=Indie+Flower&display=swap"" rel=""stylesheet""> 
    <style>
      p {{
        font-family: 'Indie Flower', 'Segoe UI Symbol';
        font-size: 5.22rem;
        margin: 0;
        color: #00de00;
        -webkit-text-stroke: 0.25rem black;
        paint-order: stroke fill;
      }}
      .note::before {{ content: '\266A'; }}
      body {{
        background-color: rgba(0, 0, 0, 0);
        margin: 0px auto;
        overflow: hidden;
      }}
    </style>
  </head>
  <body>
    {0}
    <script>
      const song = document.getElementById('song')
      const title = document.getElementById('title')
      const separator = document.getElementById('separator')
      const artist = document.getElementById('artist')
      const api = `${{window.location.href}}api`
      const set = (target, value) => {{ if (target.innerHTML !== value) target.innerHTML = value }}      
      const clear = () => {{
        song.classList.remove('note')
        set(title, '')
        set(separator, '')
        set(artist, '')
      }}
      const apply = (data) => {{
        if (data.title == null && data.artist == null) return
        if (data.title === '' && data.artist === '') {{
          clear()
          return
        }}
        song.classList.add('note')
        if (data.artist === '') {{
          set(title, data.title)
          set(separator, '')
          return
        }}
        if (data.title === '') {{
          set(title, 'Unknown')
        }} else {{
          set(title, data.title)
        }}
        set(separator, '-')
        set(artist, data.artist)
      }}
      const getSong = () => {{
        fetch(api)
          .then(response => response.json())
          .then(data => {{
            apply(data)
            window.setTimeout(getSong, 100)
          }})
          .catch(_ => {{
            clear()
            window.setTimeout(getSong, 100)
          }})
      }}
      window.setTimeout(getSong, 100)
    </script>
  </body>
</html>".TrimStart();

        public static async Task HandleIncomingConnections()
        {
            while (runServer)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                resp.ContentEncoding = Encoding.UTF8;
                resp.AppendHeader("Access-Control-Allow-Origin", "*");
                if (req.Url.AbsolutePath == "/api")
                {
                    byte[] data = Encoding.UTF8.GetBytes(string.Format(apiData, title, artist));
                    resp.ContentType = "text/json";
                    resp.ContentLength64 = data.LongLength;
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                }
                else
                {
                    byte[] data = Encoding.UTF8.GetBytes(string.Format(pageData, FormatSong()));
                    resp.ContentType = "text/html";
                    resp.ContentLength64 = data.LongLength;
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                }
                resp.Close();
            }
        }

        static string FormatSong()
        {
            if (title == string.Empty && artist == string.Empty)
            {
                return @"<p id=""song""><span id=""title""></span> <span id=""separator""></span> <span id=""artist""></span></p>";
            }
            if (artist == string.Empty)
            {
                return string.Format(@"<p class=""note"" id=""song""><span id=""title"">{0}</span> <span id=""separator""></span> <span id=""artist""></span></p>", title);
            }
            if (title == string.Empty)
            {
                return string.Format(@"<p class=""note"" id=""song""><span id=""title"">Unknown</span> <span id=""separator"">-</span> <span id=""artist"">{0}</span></p>", artist);
            }
            return string.Format(@"<p class=""note"" id=""song""><span id=""title"">{0}</span> <span id=""separator"">-</span> <span id=""artist"">{1}</span></p>", title, artist);
        }
        public static async Task Run(int _port)
        {
            await Task.Run(() =>
            {
                port = _port;
                isRunning = true;
                listener = new HttpListener();
                listener.Prefixes.Add($"{host}:{port}/");
                listener.Start();
                Task listenTask = HandleIncomingConnections();
                listenTask.GetAwaiter().GetResult();
                listener.Close();
                isRunning = false;
            });
        }
        
        public static void Start()
        {
            if (!isRunning)
            {
                runServer = true;
                Task.Run(() => Run(port));
            }
            else
            {
                if (!runPending)
                {
                    runPending = true;
                    Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        if (runServer)
                        {
                            runPending = false;
                            Start();
                        }
                    });
                }
            }
        }

        public static void Stop()
        {
            runServer = false;
        }
    }
}