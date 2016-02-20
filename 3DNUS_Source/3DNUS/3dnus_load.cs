﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;

namespace _3DNUS
{
    public partial class main_load : Form
    {
        public main_load()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            Main form = new Main();
            form.Show();
            Hide();
        }

        private void main_load_Load(object sender, EventArgs e)
        {
            Process.Start("3DNUS Upd - Lite.exe");
            {
                string cd = Path.GetDirectoryName(Application.ExecutablePath);
                {
                    File.ReadAllText(cd + "\\Config\\dev_mode_cfg.cfg");
                    File.ReadAllText(cd + "\\Config\\adv_dns.cfg");
                    File.ReadAllText(cd + "\\Config\\adv_dns_add_p.cfg");
                    File.ReadAllText(cd + "\\Config\\adv_dns_add_s.cfg");
                    File.ReadAllText(cd + "\\Config\\adv_dns_ip4.cfg");
                    File.ReadAllText(cd + "\\Config\\adv_px.cfg");
                    File.ReadAllText(cd + "\\Config\\adv_px_add.cfg");
                    File.ReadAllText(cd + "\\Config\\adv_px_pass.cfg");
                    File.ReadAllText(cd + "\\Config\\adv_px_usr.cfg");
                    File.ReadAllText(cd + "\\Config\\adv_sub_mask.cfg");
                    File.ReadAllText(cd + "\\Config\\dev_mode_cfg.cfg");
                    File.ReadAllText(cd + "\\Config\\ext_ext.cfg");
                    File.ReadAllText(cd + "\\Config\\ext_net_4.cfg");
                    File.ReadAllText(cd + "\\Config\\ext_sandbox.cfg");
                    File.ReadAllText(cd + "\\Config\\ext_sandbox_md.cfg");
                    File.ReadAllText(cd + "\\Config\\ext_sandbox_st.cfg");
                    File.ReadAllText(cd + "\\Config\\upd_auto.cfg");
                    File.ReadAllText(cd + "\\Config\\upd_custom.cfg");
                    File.ReadAllText(cd + "\\Config\\upd_custom_svr.cfg");
                    File.ReadAllText(cd + "\\Config\\ver_dis.cfg");
                    File.ReadAllText(cd + "\\Config\\vnm.cfg");
                    File.ReadAllText(cd + "\\Config\\vnm_fw.cfg");
                    File.ReadAllText(cd + "\\Config\\vnm_install_signed.cfg");
                    File.ReadAllText(cd + "\\Config\\vnm_skip_failed.cfg");
                    File.ReadAllText(cd + "\\Config\\upd_checker.cfg");



                    {

                        if (File.ReadAllText(cd + "\\Config\\dev_mode_cfg.cfg") == ("1"))
                        {
                            panel1.Visible = true;
                        }





                        // Process create = new Process();
                        // create.StartInfo.FileName = "3DNUS_Upd.exe";
                        //Application.Exit();
                    }
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            label2.Visible = true;
            timer2.Stop();
        }
    }
}

