﻿/* ====================================

   Thanks to MarcusD for his Parser & other
   additions.

   Credits:

   @Dr.Hacknik -- Project Leader, Coding, and Designer
   @MarcusD -- Project Partner, Minor Coding, Bug Fixer, and Tips
   @IgnaceMaes -- Material Skin Lib for C#
   @GitHub -- For hosting
   @FileTrip -- For Downloads (In the past)
   @WiiBrew -- For hosting Updates (In the past)
   @OneDrive -- For Backups
   @GBATemp & Users -- For Your support
   @YouTube & DreamInCode for a little help

   License:

   3DNUS is licensed under the Open-GPL v3 License.
   All code made in this Project is Open-Source, nothing
   is closed source.
   For the full license, open the LICENSE.txt file for the GPL License
   v2/v3.
*/

//=====================================
using _3DNUS;

using MarcusD.at;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3DNUS_Material_Edition
{
    public partial class FormMain : MaterialForm
    {
        public static readonly String server = "http://nus.cdn.c.shop.nintendowifi.net/ccs/download/";
        private bool is64 = System.Environment.Is64BitOperatingSystem;
        public static readonly String ninupdate = "http://yls8.mtheall.com/ninupdates/titlelist.php?csv=1&sys=";

        public static readonly String[] sysarr = new String[]
        {
            "ctr",  "ktr"
        };

        public static int selsys = 0;

        private YLS yls;
        private Operation op;
        private SyncDown sd;
        private upd upd;

        private volatile Boolean working = false;
        private volatile Boolean cancel = false;

        private readonly WebClient wc = new WebClient();

        public readonly String cd = Path.GetDirectoryName(Application.ExecutablePath);

        public FormMain()
        {
            InitializeComponent();
            MaterialSkinManager.Instance.AddFormToManage(this);

            op = new Operation()
            {
                progbar = prog_main,
                subbar = prog_sub,
                lblprog = prog_main,
                lblsub = prog_sub
            };

            sd = new SyncDown(op);

            if (!File.Exists("make_cdn_cia.exe")) c_cia.Checked = false;
            c_cia.CheckedChanged += (_dmy_sndr, _dmy_evt) =>
            {
                if (c_cia.Checked && !File.Exists("make_cdn_cia.exe"))
                {
                    MessageBox.Show("Error: make_cdn_cia.exe is not in the working directory!\r\n" +
                        "This option is unavailable while make_cdn_cia.exe is not found", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    c_cia.Checked = false;
                }
            };
            t_titleid.KeyUp += (_dmy_sndr__, _keyevt) =>
            {
                if (_keyevt.KeyCode == Keys.Enter || _keyevt.KeyCode == Keys.Return)
                {
                    t_titleid.Text = t_titleid.Text.Trim();

                    if (b_download.Enabled) b_download.PerformClick();
                }
            };
            b_download.SizeChanged += (__dmy_sndr_, __wat) =>
            {
                b_download.Left = (this.Width / 2) - (b_download.Width / 2);
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbl_ver.Text = Application.ProductVersion;
            if (Properties.Settings.Default.dev_def_titlelist == "old")
            {
                old_3ds.Checked = true;
            }
            if (Properties.Settings.Default.dev_def_titlelist == "new")
            {
                new_3ds.Checked = true;
            }
            //try
            //{
            //    Process.Start(cd + "\\3DNUS Upd - Lite.exe");
            t_log.Text += " " + DateTime.Now;

            DialogResult dialogResult = MessageBox.Show("All components that are used in 3DNUS are either created by Me, or other users; these components are Open-Source, and can not be distributed for any cost. \r\nThis also includes 3DNUS, and/or it's Components. \r\n \r\nIF you paid for 3DNUS or any other of its components, please demand your Money back Immediately! \r\nAlso, report where you Purchased 3DNUS or as a Bundle. \r\nWe are NOT affiliated with Nintendo, or any other Company. \r\n \r\nThis project is Non-Profit, meaning it will always be Free, and is maintained by Volunteers. \r\nBy using this Program, you agree to these Terms. \r\n \r\nDo you agree to these Terms? \r\n \r\nIn order to access other menu's, right-click the main Window. \r\n \r\nCaution: Some parts of 3DNUS may have some Loud Bck Music!", "Legal Terms:", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (dialogResult == DialogResult.Yes)
            {
            }
            else if (dialogResult == DialogResult.No)
            {
                MessageBox.Show("You have chosen to NOT agree to the Terms, therefor the Program will now Close.", "Legal Terms:", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            //}
            //catch
            //{
            //    MessageBox.Show("Unable to Start the updater; \r\nMake sure it is in the Directory.", "Updater Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            if (Properties.Settings.Default.dev_music == "1")
            {
                dev_bck_music.URL = cd + "\\snd\\dev_bck_music_main.mp3";
            }
            else
            {
            }
        }

        private void firmwdownload(YLS_Sysver sys, char region)
        {
            if (!yls.regions.ContainsKey(region))
            {
                StringBuilder sb = new StringBuilder();
                foreach (char chr in yls.regions.Keys)
                {
                    sb.Append(chr);
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 3, 2);

                MessageBox.Show("Invalid region! Valid regions are:\r\n" + sb.ToString(), "Invalid region", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Directory.CreateDirectory(sys.label + region);
            sd.op.setProgValue(1);
            sd.op.setProgMax(yls.regions[region].Count);

            Boolean showerror = true;
            int errorn = 0;

            foreach (YLS_Title t in yls.regions[region])
            {
                sd.op.setProgValue(sd.op.getProgValue() + 1);

                YLS_Titlever optimal = null;

                foreach (YLS_Titlever tv in t.ver)
                {
                    if (tv.sysver == sys) { optimal = tv; break; }
                    if (tv.sysver < sys && (optimal == null || tv.sysver > optimal.sysver)) optimal = tv;
                }

                if (optimal == null) continue;

                if (!singledownload(t.id, optimal.version) && !cancel)
                {
                    errorn++;
                    if (!showerror || check_noerr.Checked) continue;

                    if (sd.ex != null) MessageBox.Show(printstack(sd.ex), "Exception dump", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult dr = MessageBox.Show("Do you want to dismiss further errors?", "Error", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (dr)
                    {
                        case System.Windows.Forms.DialogResult.Yes:
                            showerror = false;
                            break;

                        case System.Windows.Forms.DialogResult.No:
                            continue;
                        default: return;
                    }
                }
                if (cancel) return;
            }

            log("Firmware downloading finished with " + (errorn == 0 ? "no" : errorn.ToString()) + " error" + (errorn == 1 ? "" : "s"));

            if (!check_noerr.Checked) MessageBox.Show("Firmware downloading finished with " + errorn + " error" + (errorn == 1 ? "" : "s") +
                 (errorn == 0 ? ". Yay! ^^" : " :("), "Download completed", MessageBoxButtons.OK,
                 (errorn == 0) ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            sd.op.setProgValue(0);
            sd.op.setSubValue(0);
            sd.op.setProgMax(1);
            sd.op.setSubMax(1);
        }

        private void log(string msg)
        {
            t_log.AppendText("\r\n" + msg);
        }

        private void extensionManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Application.StartupPath + "\\Extension Manager.exe");
            }
            catch
            {
                MessageBox.Show("We're unable to locate the Executable, please make sure it's there!", "3DNUS: Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void materialRadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            selsys = 0;
            yls = null;
        }

        private void materialRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            selsys = 1;
            yls = null;
        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            b_download.Enabled = false;
            cancel = false;

            if (working)
            {
                cancel = true;
                sd.stahp();
                b_download.Text = "BEGIN DOWNLOAD";
                b_download.Enabled = true;
                return;
            }

            t_titleid.Text = t_titleid.Text.Trim();

            Match match = Regex.Match(t_titleid.Text, @"(\d+)\.(\d+)(\.(\d+))?(-(\d+))?([a-zA-Z])+");

            if (match.Success)
            {
                if (yls == null)
                {
                    log("Checking title database, please wait...");
                    String csvname = sysarr[selsys] + ".csv";
                    if (sd.syncDown(ninupdate + sysarr[selsys], csvname)) yls = YLS.Import(csvname);
                    else
                    {
                        if (File.Exists(csvname))
                        {
                            MessageBox.Show("Can't update titlelist! The current (possibly older) titlelist will be used instead.",
                                "Can't update titlelist", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            yls = YLS.Import("titlelist.csv");
                        }
                        else
                        {
                            MessageBox.Show("Can't acquire the title list!\r\n" +
                                "You can still download titles, but firmware downloading is not possible, thus the downloading will be aborted.",
                                "Can't update titlelist", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            b_download.Enabled = true;
                            return;
                        }
                    }
                }

                YLS_Sysver firmw = new YLS_Sysver() { label = (match.Groups[1] + "." + match.Groups[2] + "." + (match.Groups[4].Success ? match.Groups[4].ToString() : "0") + "-" + (match.Groups[6].Success ? match.Groups[6].ToString() : "999")) };
                String region = match.Groups[7].ToString().ToUpper();
                t_titleid.Text = firmw + region;
                t_titleid.Update();
                log("Downloading Firmware: " + firmw + region);
                b_download.Enabled = true;
                working = true;
                b_download.Text = "CANCEL";
                firmwdownload(firmw, region[0]);
                working = false;
                b_download.Text = "BEGIN DOWNLOAD";
                return;
            }

            match = Regex.Match(t_titleid.Text, @"(\d{16})([\s_]+v?(\d+))?");

            if (match.Success)
            {
                ulong title = Convert.ToUInt64(match.Groups[1].ToString(), 16);
                int version = int.Parse(match.Groups[3].ToString());
                b_download.Enabled = true;
                working = true;
                b_download.Text = "CANCEL";
                singledownload(title, version);
                working = false;
                b_download.Text = "BEGIN DOWNLOAD";
                return;
            }
            else
            {
                MessageBox.Show("Your input didn't match as a firmware version, nor as a title ID.\r\n" +
                    "Please see the little question mark above the textbox for more help.",
                    "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            b_download.Enabled = true;
        }

        private Boolean singledownload(UInt64 id, int version)
        {
            String title = id.ToString("X16");
            log("\r\nDownloading " + title + " v" + version + "...");
            sd.op.setProgText("Preparing " + title + " v" + version);
            string ftmp = "tmp";
            string downloadtmd = server + title + "/" + "tmd." + version;
            string downloadcetk = server + title + "/cetk";

            Directory.CreateDirectory(ftmp);

            if (!sd.syncDown(downloadtmd, ftmp + "\\tmd") || !sd.syncDown(downloadcetk, ftmp + "\\cetk"))
            {
                log("\r\nError downloading title " + title + " v" + version + " make sure the entered title ID and versions are correct");
                log("\r\nMake sure that you haven't blocked the NUS Servers!");
                if (!check_noerr.Checked && !cancel)
                {
                    MessageBox.Show("Error downloading title " + title + " v" + version + ".\r\n" +
                        "Make sure the entered title ID and versions are correct",
                        "Invalid title ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            //amount of contents
            try
            {
                FileStream tmd = File.Open(ftmp + "\\tmd", FileMode.Open, FileAccess.Read);
                tmd.Seek(518, SeekOrigin.Begin);
                byte[] cc = new byte[2];
                tmd.Read(cc, 0, 2);
                Array.Reverse(cc);
                int contentcounter = BitConverter.ToInt16(cc, 0);
                log("Title has " + contentcounter + " content" + (contentcounter == 1 ? "" : "s"));

                sd.op.setProgText("Downloading " + title + " v" + version + "\nTitle has " + contentcounter + " contents");

                //download files
                for (int i = 1; i <= contentcounter; i++)
                {
                    int contentoffset = 2820 + (48 * (i - 1));
                    tmd.Seek(contentoffset, SeekOrigin.Begin);
                    byte[] cid = new byte[4];
                    tmd.Read(cid, 0, 4);
                    string contentid = BitConverter.ToString(cid).Replace("-", "");
                    string downname = ftmp + "\\" + contentid;
                    sd.op.setSubText(i + " / " + contentcounter + "\nDownloading " + title + " (" + contentid + ")");
                    if (!sd.syncDown(server + title + "/" + contentid, @downname) && !cancel)
                    {
                        tmd.Close();
                        if (check_noerr.Checked) return false;
                        if (sd.ex.Message.Contains("404"))
                        {
                            sd.ex = new Exception("The given title (" + title + " v" + version + " (" + "[" + contentid + "]) doesn't exist on Nintendo's servers", sd.ex);
                        }
                        else
                        {
                            log("Can't download title:");
                            log(printstack(sd.ex));
                        }

                        return false;
                    }
                    else if (cancel) { tmd.Close(); return false; }
                    log("Downloading complete");
                }

                tmd.Close();
                if (c_cia.Checked)
                {
                    sd.op.setProgText("Packing " + title + " v" + version);

                    //create cia
                    log("Packing as .cia ...");
                    string command;
                    if (t_titleid.Text.Contains("."))
                    {
                        command = " " + "tmp" + " " + t_titleid.Text + "\\" + title + ".cia";
                    }
                    else
                    {
                        command = " " + "tmp" + " " + title + ".cia";
                    }
                    try
                    {
                        Process create = new Process();
                        create.StartInfo.FileName = "make_cdn_cia.exe";
                        create.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        create.StartInfo.Arguments = command;
                        create.Start();
                        create.WaitForExit();
                        try
                        {
                            Directory.Delete(ftmp, true);
                        }
                        catch { log("Can't delete /tmp"); }
                    }
                    catch
                    {
                        if (check_noerr.Checked == false)
                        {
                            MessageBox.Show("There was a Problem when trying to Delete the Diretory 'Temp'.", "Error: Deleting Dir", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (t_titleid.Text.Contains("."))
                    {
                        Directory.Move(ftmp, t_titleid.Text + "\\" + title);
                    }
                    else
                    {
                        Directory.Move(ftmp, title);
                    }
                }

                sd.op.setSubText(" ");

                log("Done.");
                return true;
            }
            catch
            {
                return true;
            }
        }

        private static String printstack(Exception wat, Boolean inner = true)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(wat.GetType().FullName);
            sb.Append(": ");
            sb.AppendLine(wat.Message);
            sb.AppendLine(wat.StackTrace);
            if (inner && wat.InnerException != null)
            {
                sb.Append("Caused by ");
                sb.AppendLine(printstack(wat.InnerException));
            }

            return sb.ToString();
        }

        private void FormMain_ResizeEnd(object sender, EventArgs e)
        {
            panel_ctl.Left = (this.Width / 2) - (panel_ctl.Width / 2);
            b_download.Left = (this.Width / 2) - (b_download.Width / 2);
        }

        private void materialFlatButton1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("This is the new Lightweight version of 3DNUS." + "\r\nThe Current Version you're using is: " + Application.ProductVersion + "" + "\r\n" + "\r\nVisit the forum post or GitHub for more Info." + "\r\n" + "\r\nThanks a Ton to @MarcusD for alot of His work.", "Quick Information:", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("3DNUS Upd - Lite.exe"))
                {
                    process.Kill();
                }
            }
            catch
            {
                MessageBox.Show("Unable to Kill the Process Specified; Please try again.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.dev_dump_info == "1")
            {
                if (!File.Exists("LOG_DUMP.log")) File.Delete(cd + "LOG_DUMP.log");
                File.WriteAllText(cd + "\\LOG_DUMP.log", "--Log Dump Start--" + "\r\n" + "\r\nSystemOS: " + Environment.OSVersion + "\r\n" + "\r\nProgram Version: " + Application.ProductVersion + "\r\n" + "\r\nDebug State: " + "Uknown" + "\r\n" + "\r\nTime Dumped: " + DateTime.Now + "\r\n" + "\r\n---------------------------------------" + "\r\n" + t_log.Text);
                MessageBox.Show("The Log was Dumped Sucessfully! Although, the old Log File may have been Deleted!", "Log Dump:", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (!File.Exists("LOG_DUMP.log")) File.Delete(cd + "LOG_DUMP.log");
                File.WriteAllText(cd + "\\LOG_DUMP.log", "" + t_log.Text);
                MessageBox.Show("The Log was Dumped Sucessfully! Although, the old Log File may have been Deleted!", "Log Dump:", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void faqToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dev_faq f = new dev_faq();
            f.Show();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dev_settings f = new dev_settings();
            f.Show();
            if (dev_bck_music.settings.volume > 1)
            {
                dev_bck_music.settings.volume = (dev_bck_music.settings.volume - (dev_bck_music.settings.volume / 2));
            }
        }

        private void dev_bck_music_Enter(object sender, EventArgs e)
        {
        }

        private void dev_bck_music_EndOfStream(object sender, AxWMPLib._WMPOCXEvents_EndOfStreamEvent e)
        {
            dev_bck_music.URL = cd + "\\snd\\dev_bck_music_main.mp3";
        }

        private void dev_look_forum_Tick(object sender, EventArgs e)
        {
            if (dev_settings.ActiveForm == ActiveForm)
            {
                if (dev_bck_music.settings.volume > 1)
                {
                    dev_bck_music.settings.volume = (dev_bck_music.settings.volume - (dev_bck_music.settings.volume / 2));
                }
            }
            else
            {
                if (dev_bck_music.settings.volume > 1)
                {
                    dev_bck_music.settings.volume = (dev_bck_music.settings.volume - (dev_bck_music.settings.volume / 2));
                }
            }
        }
    }
}