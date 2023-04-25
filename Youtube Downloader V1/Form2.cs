using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExtractor;
using System.IO;
using MediaToolkit.Model;
using MediaToolkit;

namespace Youtube_Downloader_V1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox1.Text);
            textBox1.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox2.Text = fbd.SelectedPath;


                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
        }

        string url = "";
        private async void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                try
                {
                    listBox1.SelectedIndex = i;
                    url = listBox1.SelectedItem.ToString();
                    download();
                }
                catch (Exception)
                {
                }
                
            }
        }

        string dosyaadi;
        void download()
        {
            IEnumerable<VideoInfo> video = DownloadUrlResolver.GetDownloadUrls(url);
            VideoInfo vi = video.First(Info => Info.VideoType == VideoType.Mp4 && Info.Resolution == Convert.ToInt32(360));
            if (vi.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(vi);
                var videodownload = new VideoDownloader(vi, textBox2.Text + vi.Title + vi.VideoExtension);
                dosyaadi = vi.Title;
                videodownload.DownloadFinished += videodownload_DownloadFinished;
                videodownload.Execute();
            }
        }

        private async void videodownload_DownloadFinished(object sender, EventArgs e)
        {
            listBox2.Items.Add(dosyaadi + " İndirildi.");
            await donustur();
            
        }

        async Task donustur()
        {
            var inputfile = new MediaFile { Filename = textBox2.Text + "\\" + dosyaadi };
            var otputfile = new MediaFile { Filename = $"{textBox2.Text + "\\" + dosyaadi}." + comboBox1.Text + "" };
            if (true)
            {
                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputfile);
                    engine.Convert(inputfile, otputfile);
                    listBox2.Items.Add(dosyaadi + " " + comboBox1.Text + " formatına donüştürüldü.");
                }
            }
            else
            {
                listBox2.Items.Add(dosyaadi + " " + comboBox1.Text + " formatına donüştürülülemedi.");
            }
        }
    }
}
