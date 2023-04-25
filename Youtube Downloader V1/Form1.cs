using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoLibrary;
using MediaToolkit;
using MediaToolkit.Model;
using System.IO;
//using AltoHttp;
using System.Net;
using System.Security.Cryptography;
using Youtube_Downloader_V1.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Media;
using System.Diagnostics;
using YoutubeExtractor;
using System.Threading;
using System.Reflection.Emit;

namespace Youtube_Downloader_V1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        //HttpDownloader httpDownloader;
        
        
        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                
                int sira = 1;
                try
                {
                    sira = listBox1.Items.Count + 1;
                }
                catch (Exception)
                {
                }
                
                string url = textBox1.Text;
                using (var cli = Client.For(YouTube.Default))
                {
                    var vid = await cli.GetVideoAsync(url);
                    listBox1.Items.Add(textBox1.Text);
                    lst_gosterim.Items.Add(sira.ToString() + " - " + vid.Title );
                    textBox1.Text = "";
                    listBox1.SelectedIndex = 0;
                    label3.Text = listBox1.SelectedIndex.ToString() + "/" + listBox1.Items.Count.ToString();
                }
                
                //Video vid = await youtube_.GetVideoAsync(url);

            }
            catch (Exception)
            {
                MessageBox.Show("URL Geçerli Değil !","Hata",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            textBox1.Select();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            lst_gosterim.Items.Clear();
            listBox2.Items.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.SelectedItem = lst_gosterim.SelectedItem;
            listBox1.Items.Remove(listBox1.SelectedItem);
            lst_gosterim.Items.Remove(lst_gosterim.SelectedItem);
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
       
       
        string kayitAdresi;
        int basarili=0;        
        bool isPause = false;
        private async void button3_Click(object sender, EventArgs e)
        {
            await Task.Run(() => calistir());
        }

        async Task calistir()
        {            
            try
            {
                button12.Enabled = true;
                
                pictureBox1.UseWaitCursor=true;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                comboBox1.Enabled = false;
                pictureBox1.Image = Resources.Radio_1s_200px;
                kayitAdresi = textBox2.Text;                
                progressBar2.Value = 0;
                
                basarili = 0;
                progressBar2.Maximum = listBox1.Items.Count;


                for (int i = 0; i < listBox1.Items.Count; i++)
                {

                    progressBar3.Value = 0;

                    while (isPause)
                    {
                        
                    }

                    try
                    {

                        listBox1.SelectedIndex = i;
                        lst_gosterim.SelectedIndex = i;
                        string url = listBox1.SelectedItem.ToString();
                        YouTube youtube_ = YouTube.Default;
                        Video vid = await youtube_.GetVideoAsync(url);

                        listBox2.Items.Add(vid.Title + " İndiriliyor... ");
                        if (await Task.Run(() => indir(vid)) == true)
                        {

                            if (tamamlandi)
                            {
                                tamamlandi = false;
                                var inputfile = new MediaFile { Filename = kayitAdresi + "\\" + vid.FullName };
                                var otputfile = new MediaFile { Filename = $"{kayitAdresi + "\\" + vid.FullName.Replace(".mp4", "")}." + comboBox1.Text + "" };
                                if (vid.FullName.Contains(comboBox1.Text) == false)
                                {
                                    if (await Task.Run(() => donusturme(inputfile, otputfile, vid)))
                                    {
                                        listBox2.Items.Add(vid.Title + "   " + comboBox1.Text + "'e Dönüştürüldü.");
                                        basarili++;
                                        label3.Text = basarili.ToString() + "/" + listBox1.Items.Count.ToString();
                                    }
                                    else
                                    {
                                        listBox2.Items.Add(vid.Title + " İndirildi ancak donüştürme işlemi başarısız oldu.");
                                    }
                                }
                                else
                                {
                                    listBox2.Items.Add(vid.Title + "   " + comboBox1.Text + "'e Dönüştürüldü.");
                                }
                            }

                        }
                        else
                        {
                            listBox2.Items.Add(vid.Title + " indirilemedi.");
                        }
                    }
                    catch (Exception)
                    {
                        listBox2.Items.Add("Bir Sorun Oluştu ! ");
                    }
                    if (progressBar2.Value < progressBar2.Maximum)
                    {
                        progressBar2.Value++;
                    }
                    
                }
                listBox2.Items.Add("Tüm İndirme İşlemleri Tamamlandı...");
                SystemSounds.Asterisk.Play();
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                comboBox1.Enabled = true;
                pictureBox1.Image = Resources.YouTube_social_white_squircle_svg;
                pictureBox1.UseWaitCursor = false;
                button12.Enabled = false;
            }
            catch (Exception)
            {
            }

        }

        string seciliindirme;      
        bool tamamlandi = false;
        async Task <bool> indir(Video vid)
        {
            try
            {

                
                seciliindirme = vid.Title;                

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += webClient_DownloadProgressChanged;
                    wc.DownloadFileCompleted += webClient_DownloadFileCompleted;
                    wc.DownloadFileAsync(new System.Uri(vid.Uri), textBox2.Text + "\\" + vid.FullName);
                }                                             

                await Task.Run(() => tamamlandimi_(vid));

                // eski hali
                //System.IO.File.WriteAllBytes(kayitAdresi + "\\" + vid.FullName, await vid.GetBytesAsync());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        async Task tamamlandimi_(Video vid)
        {
           
            while (tamamlandi == false)
            {
                while (isPause)
                {

                }
               // indirme tamamlanana kadar dnecek
            }            
            return;                       
        }

        async Task <bool> donusturme(MediaFile input, MediaFile output, Video vid) 
        {

            using (var engine = new Engine())
            {
                try
                {
                    label4.Text = seciliindirme + " Dönüştürülüyor... ";
                    //engine.ConvertProgressEvent += engine_ConvertProgressEvent;                    
                    //engine.ConversionCompleteEvent += engine_ConversionCompleteEvent;                    
                    engine.GetMetadata(input);
                    engine.Convert(input, output);
                    string klasorYolu = kayitAdresi; // bu klasörde dosya aranacak ve tüm alt klasörlerinde
                    string silinecekDosya = vid.FullName;   // silinecek dosyanın adı
                                                          //System.IO.SearchOption.AllDirectories ifadesi tüm alt klasörlerde de arama yapılmasını sağlıyor
                    string[] dosyaList = System.IO.Directory.GetFiles(klasorYolu, silinecekDosya, System.IO.SearchOption.AllDirectories);
                    foreach (string dosya in dosyaList)
                    {                      
                        System.IO.File.Delete(dosya);
                    }
                    //label4.Text = "%0";
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }                
            }
            
        }

        //private void engine_ConversionCompleteEvent(object sender, ConversionCompleteEventArgs e)
        //{
        //    tamamlandi = true;
        //}

        //private void engine_ConvertProgressEvent(object sender, ConvertProgressEventArgs e)
        //{
        //    while (isPause)
        //    {
        //        label4.Text = "Duraklatıldı " + " Kalan Süre " + e.ProcessedDuration.ToString("00,00") + "  /  " + e.TotalDuration.ToString("00,00") + seciliindirme ;
        //        Thread.Sleep(100);
        //    }

        //    label4.Text = " Kalan Süre " + e.ProcessedDuration.ToString("00,00") + "  /  " + e.TotalDuration.ToString("00,00") + seciliindirme + " Dönüştürülüyor... "  ;
            
        //}

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Yotube Downloader V1 İndirilenler"));
            }
            catch (Exception)
            {
            }
            textBox1.Select();
            textBox2.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Yotube Downloader V1 İndirilenler");

            comboBox1.SelectedIndex = 0;
                
           

        }

        async Task browsergit(string url)
        {
            chromiumWebBrowser1.Load(url);
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            await Task.Run(() => browsergit("https://www.youtube.com/"));
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            await Task.Run(() => browsergit("https://www.youtube.com/"));
        }

        private async void button9_Click(object sender, EventArgs e)
        {
            await Task.Run(() => browsergit(textBox3.Text));
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = textBox2.Text,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception)
            {
            }            
        }
        
        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            
            progressBar3.Value = 100;
            listBox2.Items.Add(seciliindirme + " İndirildi.");
            listBox2.SelectedIndex = listBox2.Items.Count-1;
            tamamlandi = true;
        }

        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {

            //while (isPause)
            //{

            //    //label4.Text = "Duraklatıldı " + " %" + e.ProgressPercentage.ToString() + "  /  " + seciliindirme + "Dosya Boyutu : " + ((double)(e.TotalBytesToReceive / 1024) / 1024).ToString("0.0") + " MB" + " İndirilen : " + ((double)(e.BytesReceived / 1024) / 1024).ToString("0.0") + " MB";                
            //    //Thread.Sleep(100);
            //}

            label4.Text = " %" + e.ProgressPercentage.ToString() + "  /  " + seciliindirme + "Dosya Boyutu : " + ((double)(e.TotalBytesToReceive / 1024)/ 1024).ToString("0.00") + " MB" + " İndirilen : " + ((double)(e.BytesReceived/1024)/ 1024).ToString("0.00") + " MB" ;
            progressBar3.Value = e.ProgressPercentage;



        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode==Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }

        private async void button12_Click(object sender, EventArgs e)
        {
            
            isPause = true;
            MessageBox.Show("Mevcut İndirme Bittikten Sonra Durdurulacaktır. Düzenleme Yapabilirsiniz.","Uyarı",MessageBoxButtons.OK,MessageBoxIcon.Information);
            button12.Enabled = false;
            button13.Enabled = true;
            button1.Enabled = true;            
            //button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            comboBox1.Enabled = true;
            pictureBox1.Image = Resources.Bean_Eater_1s_200px__2_;
            pictureBox1.UseWaitCursor = false;
        }

        

        private void button13_Click(object sender, EventArgs e)
        {           
            isPause = false;
            button12.Enabled = true;
            button13.Enabled = false;
            button1.Enabled = false;            
            //button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            comboBox1.Enabled = false;
            pictureBox1.Image = Resources.Radio_1s_200px;
            pictureBox1.UseWaitCursor = true;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
              
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }
    }
}
