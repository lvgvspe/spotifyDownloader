using HtmlAgilityPack;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace SpotifyDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult folder = folderBrowserDialog1.ShowDialog();
            if (folder == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Downloader();
            async void Downloader()
            {
                string url = textBox1.Text;
                HtmlWeb web = new();
                var doc = web.Load(url);
                var music = doc.DocumentNode.SelectNodes("//a[contains(@class, 'EntityRowV2__Link-sc-ayafop-8')]");
                var span = doc.DocumentNode.SelectNodes("//span[contains(@class, 'Mesto-sc-1e7huob-0')]");
                List<HtmlAgilityPack.HtmlNode> artist = new();
                foreach (var s in span)
                {
                    artist.Add(s.FirstChild);
                }
                var list = music.Zip(artist, (m, a) => new { music = m, artist = a });
                var folder = doc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'Type__TypeElement-goli3j-0')]");
                if (!Directory.Exists($@"{textBox2.Text}\{folder.InnerHtml}"))
                {
                    Directory.CreateDirectory($@"{textBox2.Text}\{folder.InnerHtml}");
                }
                var youtube = new YoutubeClient();
                progressBar1.Maximum = list.Count() + 1;
                progressBar1.Value++;
                foreach (var l in list)
                {
                    var videos = youtube.Search.GetVideosAsync($"{l.music.InnerHtml} {l.artist.InnerHtml}");
                    List<string> id = new List<string>();
                    await foreach (var result in videos)
                    {
                        id.Add(result.Id);
                        break;
                    }
                    try
                    {
                        var video = await youtube.Videos.GetAsync($"{id[0]}");
                        string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                        Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                        string title = r.Replace(video.Title, "");
                        await youtube.Videos.DownloadAsync(id[0], $@"{textBox2.Text}\{folder.InnerHtml}\{title}.mp3");
                    }
                    catch { }
                    progressBar1.Value++;
                }
            }

        }
    }
}