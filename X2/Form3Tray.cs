﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace X2
{
    public partial class Form3Tray : Form
    {
        public Form3Tray()
        {
            InitializeComponent();
            checkBox1.Checked = ApplicationIsInStartup();
            ToggleToTray(true);
        }

        private void Form3Tray_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Wyłączenie aplikacji może spowodować pominięcie testów. Czy na pewno chcesz wyłączyć?", "Uwaga!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                e.Cancel = true;
                ToggleToTray(true);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                AddApplicationToStartup();
            }
            else
            {
                if (MessageBox.Show("Wyłączenie automatycznego uruchomienia aplikacji może spowodować pominięcie testów. Czy na pewno chcesz wyłączyć?", "Uwaga!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    RemoveApplicationFromStartup();
                }
                else
                {
                    checkBox1.Checked = true;
                }
            }
        }

        private static void AddApplicationToStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                string name = Application.ExecutablePath;
                name = name.Substring(name.LastIndexOf('\\') + 1);
                if (!key.GetValueNames().Contains(name))
                {
                    key.SetValue(name, "\"" + Application.ExecutablePath + "\"");
                    Console.WriteLine("Adding to startup");
                }
            }
        }

        private static void RemoveApplicationFromStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                string name = Application.ExecutablePath;
                name = name.Substring(name.LastIndexOf('\\') + 1);
                if (key.GetValueNames().Contains(name))
                {
                    key.DeleteValue(name, false);
                    Console.WriteLine("Removing from startup");
                }
            }
        }

        private static bool ApplicationIsInStartup()
        {
            string name = Application.ExecutablePath;
            name = name.Substring(name.LastIndexOf('\\') + 1);

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                return key.GetValueNames().Contains(name);
            }
        }

        private void ToggleToTray(bool hide)
        {
            if (hide)
            {
                WindowState = FormWindowState.Minimized;
                Hide();
                notifyIcon1.Visible = true;
            }
            else
            {
                Show();
                this.WindowState = FormWindowState.Normal;
                //notifyIcon1.Visible = false;
            }

        }

        

        private void Form3Tray_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                ToggleToTray(true);
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            ToggleToTray(false);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            OnTimerTick();
        }

        private void OnTimerTick()
        {
            string s = "TODO: database (pooling!); is day and hour within brackets " +
                DateTime.Now.DayOfWeek.ToString() +
                " " + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() +
                "\r\nis test running" +
                "\r\nis test batch planned and not done yet" +
                "\r\nthen batch.run";
            Console.WriteLine(s);
        }
        
        //drugi timer do wysłania notyfikacji o rezultatach do uzytkownika




        // DEBUG -----------------------------

        private void DEBUG_BTN_Click(object sender, EventArgs e)
        {
            





        }





    }
}