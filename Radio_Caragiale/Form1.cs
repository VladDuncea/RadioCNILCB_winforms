using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Radio_Caragiale
{
    

    public partial class Form1 : Form
    {
        //MediaPlayer.MediaPlayer ThePlayer;
        WMPLib.IWMPPlaylist playlist;
        string sonerie;
        bool music_started = false;
        bool end_bell = false;
        bool halloween=false;

        public Form1()
        {
            InitializeComponent();
            playlist = axWindowsMediaPlayer1.playlistCollection.newPlaylist("Radio");
            axWindowsMediaPlayer1.settings.autoStart = false;
            axWindowsMediaPlayer1.settings.setMode("shuffle", true);
            axWindowsMediaPlayer1.settings.setMode("loop", true);
            axWindowsMediaPlayer1.settings.volume = 50;
            Ser_Load();
        }

        string[] files, paths;
        List<WMPLib.IWMPMedia> media = new List<WMPLib.IWMPMedia>();

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {   if (sender != null&& listBox1.SelectedIndex>=0)
            {
                Debug.WriteLine("Index:" + listBox1.SelectedIndex);
                axWindowsMediaPlayer1.Ctlcontrols.playItem(media[listBox1.SelectedIndex]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Index:");
            playlist.clear();
            listBox1.Items.Clear();
            listBox1.ClearSelected();
            media.Clear();
            button1_Click(null, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                sonerie = openFileDialog1.FileName;
                Ser_Save();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!File.Exists(System.IO.Directory.GetCurrentDirectory() + "/Serialization.duncea"))
                return;

            label2.Text = DateTime.Now.ToString("HH:mm:ss ");

            int current_min = int.Parse(DateTime.Now.ToString("mm"));
            int current_hh = int.Parse(DateTime.Now.ToString("HH"));
            int minutes = 50;
            if (halloween)
            {
                switch (current_hh)
                {
                    case 8:
                        minutes = 45;
                        break;
                    case 9:
                        minutes = 35;
                        break;
                    case 10:
                        minutes = 25;
                        break;
                    case 11:
                        minutes = 15;
                        break;
                    case 12:
                        if (current_min > 10)
                            minutes = 55;
                        else
                            minutes = 5;
                        break;
                    case 13:
                        minutes = 45;
                        break;
                    default:
                        minutes = 50;
                        break;
                }
                if (current_min >= minutes && current_min < minutes + 5 && !music_started && current_hh <= 13 && current_hh >= 7)
                {
                    //axWindowsMediaPlayer1.Ctlcontrols.playItem(sonerie);
                    //Process.Start(Directory.GetCurrentDirectory() + "\\Batch\\Start_wmp.bat");
                    axWindowsMediaPlayer1.openPlayer(sonerie);
                    System.Threading.Thread.Sleep(4000);
                    music_started = true;
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                    end_bell = false;
                }
                else if (int.Parse(DateTime.Now.ToString("mm")) >= minutes && current_hh == 14 && !end_bell)
                {
                    axWindowsMediaPlayer1.openPlayer(sonerie);
                    end_bell = true;
                }
                else if (music_started && (current_min < minutes || current_min >= minutes + 5))
                {
                    music_started = false;
                    axWindowsMediaPlayer1.Ctlcontrols.pause();
                    axWindowsMediaPlayer1.openPlayer(sonerie);
                }
            }
            else
            {
                if (int.Parse(DateTime.Now.ToString("mm")) >= minutes && !music_started && int.Parse(DateTime.Now.ToString("HH")) <= 13 && int.Parse(DateTime.Now.ToString("HH")) >= 7)
                {
                    //axWindowsMediaPlayer1.Ctlcontrols.playItem(sonerie);
                    //Process.Start(Directory.GetCurrentDirectory() + "\\Batch\\Start_wmp.bat");
                    axWindowsMediaPlayer1.openPlayer(sonerie);
                    System.Threading.Thread.Sleep(4000);
                    music_started = true;
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                    end_bell = false;
                }
                else if (int.Parse(DateTime.Now.ToString("mm")) >= minutes && int.Parse(DateTime.Now.ToString("HH")) == 14 && !end_bell)
                {
                    axWindowsMediaPlayer1.openPlayer(sonerie);
                    end_bell = true;
                }
                if (int.Parse(DateTime.Now.ToString("mm")) < minutes && music_started)
                {
                    music_started = false;
                    axWindowsMediaPlayer1.Ctlcontrols.pause();
                    axWindowsMediaPlayer1.openPlayer(sonerie);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                files = openFileDialog1.SafeFileNames;
                paths = openFileDialog1.FileNames;

                for (int i = 0; i < files.Length; i++)
                {
                    listBox1.Items.Add(files[i]);
                    WMPLib.IWMPMedia mediaItem = axWindowsMediaPlayer1.newMedia(paths[i]);
                    playlist.appendItem(mediaItem);
                    media.Add(mediaItem);
                }
                Ser_Save();
                axWindowsMediaPlayer1.currentPlaylist = playlist;
                //Ser_Load();
            }
        }

        public void Ser_Save()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(System.IO.Directory.GetCurrentDirectory() + "/Serialization.duncea", FileMode.Create, FileAccess.Write, FileShare.Write);
            UserData userdata = new UserData(paths,files,sonerie);
            bf.Serialize(fs,userdata);
            fs.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Made by Duncea Vlad in collaboration with 'ADI'", "RadioCaagiale", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (halloween)
            {
                halloween = false;
                MessageBox.Show("Mod 45 - 5 dezactivat", "RadioCaagiale", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                halloween = true;
                MessageBox.Show("Mod 45 - 5 activat", "RadioCaagiale", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

        }

        public void Ser_Load()
        {
            if (!File.Exists(System.IO.Directory.GetCurrentDirectory() + "/Serialization.duncea"))
                return;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(System.IO.Directory.GetCurrentDirectory() + "/Serialization.duncea", FileMode.Open, FileAccess.Read, FileShare.Read);
            UserData userdata =(UserData) bf.Deserialize(fs);
            paths = userdata.path_ser;
            files = userdata.filename_ser;

            for (int i = 0; i < paths.Length; i++)
            {
                listBox1.Items.Add(files[i]);
                WMPLib.IWMPMedia mediaItem = axWindowsMediaPlayer1.newMedia(paths[i]);
                playlist.appendItem(mediaItem);
                media.Add(mediaItem);
            }
            axWindowsMediaPlayer1.currentPlaylist = playlist;
            axWindowsMediaPlayer1.settings.volume = 100;
            sonerie = userdata.sonerie;
            fs.Close();

        }
    }

    [Serializable]
    public class UserData
    {
        public string[] path_ser;
        public string[] filename_ser;
        public string sonerie;

        public UserData(string[] path_ser,string[] filename_ser,string sonerie)
        {
            this.path_ser = path_ser;
            this.filename_ser = filename_ser;
            this.sonerie = sonerie;
        }
        
    }
}
