﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Web.Security;
using System.Management;
using System.Security.Cryptography;
using System.Security;
using System.Collections;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;

namespace Revamped_BnS_Buddy
{
    public unsafe partial class Splash1 : MetroFramework.Forms.MetroForm
    {
        public static Splash1 CurrentForm;
        //Form f = Application.OpenForms["Form1"];
        public string AppPath = Path.GetDirectoryName(Application.ExecutablePath);
        public unsafe string username = @"";
        public unsafe string password = @"";
        public unsafe string Protect;
        public unsafe string result;
        public unsafe string SALT = "";
        public bool remembered = false;
        public bool INTRUDER = false;

        public Splash1()
        {
            // Initialize Form
            InitializeComponent();
            // Get Unique SALT
            SALT = Security.FingerPrint.Value();
            SALT = StringToHex(SALT);
            // Check if not already remembered
            if (File.ReadAllText(@AppPath + "\\Settings.ini").Contains("rememberme = true"))
            {
                RegistryKey regKey = Registry.LocalMachine;
                regKey = regKey.OpenSubKey(@"SOFTWARE\BnS Buddy\");

                // foreach account login saved, add to dropbox
                foreach (string InReg in regKey.GetSubKeyNames())
                {
                    metroComboBox1.Items.Add(InReg.ToString());
                }

                if (metroComboBox1.Items.Count >= 1)
                {
                    metroComboBox1.SelectedIndex = 0;

                    string tmp_user = string.Empty;
                    string tmp_pass = string.Empty;
                    if (regKey != null)
                    {
                        tmp_user = regKey.OpenSubKey(metroComboBox1.SelectedItem.ToString()).GetValue("username").ToString();
                        tmp_pass = regKey.OpenSubKey(metroComboBox1.SelectedItem.ToString()).GetValue("password").ToString();
                        metroTextBox1.Text = Dec(tmp_user);
                        metroTextBox2.Text = Dec(tmp_pass);
                        metroCheckBox1.CheckState = CheckState.Checked;
                    }
                }
            }
            // Check caps lock
            CheckLock();
        }

        private string StringToHex(string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char t in s)
            {
                sb.Append(Convert.ToInt32(t).ToString("x"));
            }
            return sb.ToString();
        }

        string Enc(string s)
        {
            s = String.Concat(s, SALT);
            s = Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(s), "Basic Enc"));
            return s;
        }

        string Dec(string s)
        {
            s = Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(s), "Basic Enc"));
            if (s.Contains(SALT))
            {
                s = s.Replace(SALT, "");
            }
            else
            {
                s = "";
                if (INTRUDER == false)
                {
                    Prompt.Popup("Did you changed hardware? Because I don't recognize you.");
                    metroComboBox1.Enabled = false;
                    ClearRegistry();
                    INTRUDER = true;
                }
            }
            return s;
        }

        public void ClearRegistry()
        {
            RegistryKey regKey = Registry.LocalMachine;
            regKey = regKey.OpenSubKey(@"SOFTWARE\BnS Buddy\");
            foreach (string InReg in regKey.GetSubKeyNames())
            {
                Registry.LocalMachine.OpenSubKey("SOFTWARE\\BnS Buddy", true).DeleteSubKeyTree(InReg.ToString());
            }
            
        }

        public void Perform()
        {
            // Login
            if ((metroTextBox1.Text != "") && (metroTextBox2.Text != ""))
            {
                metroButton1.Visible = false;
                try
                {
                    username = @metroTextBox1.Text.ToString();
                    password = @metroTextBox2.Text.ToString();


                    if (metroCheckBox1.CheckState == CheckState.Checked)
                    {
                        try
                        {
                            string input = metroTextBox1.Text;
                            int inputclear = input.IndexOf("@");
                            if (inputclear > 0)
                                input = input.Substring(0, inputclear);

                            Registry.LocalMachine.CreateSubKey("SOFTWARE\\BnS Buddy\\" + input);
                            Registry.LocalMachine.OpenSubKey("SOFTWARE\\BnS Buddy\\" + input, true).SetValue("username", Enc(@metroTextBox1.Text), Microsoft.Win32.RegistryValueKind.String);
                            Registry.LocalMachine.OpenSubKey("SOFTWARE\\BnS Buddy\\" + input, true).SetValue("password", Enc(@metroTextBox2.Text), Microsoft.Win32.RegistryValueKind.String);
                            if (File.ReadAllText(@AppPath + "\\Settings.ini").Contains("rememberme = false"))
                            {
                                string tmp = File.ReadAllText(@AppPath + "\\Settings.ini");
                                tmp = tmp.Replace("rememberme = false", "rememberme = true");
                                File.WriteAllText(@AppPath + "\\Settings.ini", tmp);
                            }
                        }
                        catch
                        {
                            // nothing here
                        }
                    }
                    else
                    {
                        try
                        {
                            string input = metroTextBox1.Text;
                            int inputclear = input.IndexOf("@");
                            if (inputclear > 0)
                                input = input.Substring(0, inputclear);

                            RegistryKey regKey = Registry.LocalMachine;
                            regKey = regKey.OpenSubKey(@"SOFTWARE\BnS Buddy\" + input + @"\");
                            string tmp_user = string.Empty;
                            string tmp_pass = string.Empty;
                            if (regKey != null)
                            {
                                Registry.LocalMachine.OpenSubKey("SOFTWARE\\BnS Buddy", true).DeleteSubKeyTree(input);
                                if (File.ReadAllText(@AppPath + "\\Settings.ini").Contains("rememberme = true"))
                                {
                                    string tmp = File.ReadAllText(@AppPath + "\\Settings.ini");
                                    tmp = tmp.Replace("rememberme = true", "rememberme = false");
                                    File.WriteAllText(@AppPath + "\\Settings.ini", tmp);
                                }
                            }
                        }
                        catch
                        {
                            // nothing here
                        }
                    }


                    this.Hide();
                    ShowDialog();
                    Show(ActiveForm); // Shows the program on taskbar
                    WindowState = FormWindowState.Normal; // Undoes the minimized state of the form
                }
                catch
                {
                    metroButton1.Visible = true;
                }
            }
            else if ((metroTextBox1.Text == "") || (metroTextBox2.Text == ""))
            {
                if ((metroTextBox1.Text == "") && (metroTextBox2.Text == ""))
                {
                    Prompt.Popup("Fields are empty!");
                    metroButton1.Visible = true;
                }
                else
                {
                    Prompt.Popup("One of the fields are empty!");
                    metroButton1.Visible = true;
                }
            }
            else
            {
                Prompt.Popup("Error!");
                metroButton1.Visible = true;
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            Perform();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            // Close login window
            KillApp();
        }

        public void KillApp()
        {
            Process p = Process.GetCurrentProcess();
            p.Kill();
        }

        private void metroLabel3_Click(object sender, EventArgs e)
        {
            Prompt.Popup("Getting 'Wrong Password...' error?" + Environment.NewLine + "Password has to be the following." + Environment.NewLine + "Must be 8 - 16 characters long." + Environment.NewLine + "Must not be similar to your email address or date of birth." + Environment.NewLine + "Must contain at least one number." + Environment.NewLine + "Must contain at least one alphabetic character(A - Z)." + Environment.NewLine + "No more than 4 of the continuous number or letter in a row." + Environment.NewLine + "No more than 4 of the same number or letter in a row." + Environment.NewLine + Environment.NewLine + "If your password does not respect the following," + Environment.NewLine + "please change it." + Environment.NewLine + Environment.NewLine);
        }

        private void metroTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            CheckLock();
            if (e.KeyCode == Keys.Enter)
            {
                Perform();
            }
        }

        private void metroTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            CheckLock();
            if (e.KeyCode == Keys.Enter)
            {
                Perform();
            }
        }

        private void CheckLock()
        {
            if (IsKeyLocked(Keys.CapsLock))
            {
                metroLabel4.Visible = true;
            }
            else
            {
                metroLabel4.Visible = false;
            }
        }

        private void Splash1_KeyDown(object sender, KeyEventArgs e)
        {
            CheckLock();
        }

        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string user = metroComboBox1.SelectedItem.ToString();
                RegistryKey regKey = Registry.LocalMachine;
                regKey = regKey.OpenSubKey(@"SOFTWARE\BnS Buddy\");
                string tmp_user = string.Empty;
                string tmp_pass = string.Empty;
                if (regKey != null)
                {
                    tmp_user = regKey.OpenSubKey(user).GetValue("username").ToString();
                    tmp_pass = regKey.OpenSubKey(user).GetValue("password").ToString();
                    metroTextBox1.Text = Dec(tmp_user);
                    metroTextBox2.Text = Dec(tmp_pass);
                }
            }
            catch
            {
                // nothing here
            }
        }

        public static class Prompt
        {
            public static void Popup(string Message)
            {
                ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
                MetroFramework.Forms.MetroForm prompt = new MetroFramework.Forms.MetroForm()
                {
                    Width = 280,
                    Height = 135,
                    FormBorderStyle = FormBorderStyle.None,
                    Resizable = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowOnly,
                    Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon"))),
                    ControlBox = false,
                    Theme = MetroFramework.MetroThemeStyle.Dark,
                    DisplayHeader = false,
                    TopMost = true,
                    Text = "",
                    StartPosition = FormStartPosition.CenterScreen
                };
                MetroFramework.Controls.MetroLabel textLabel = new MetroFramework.Controls.MetroLabel() { Dock = DockStyle.Fill, AutoSize = true, Left = 5, Top = 0, Text = Message, Width = 270, Height = 40, TextAlign = ContentAlignment.MiddleCenter, Theme = MetroFramework.MetroThemeStyle.Dark };
                MetroFramework.Controls.MetroButton confirmation = new MetroFramework.Controls.MetroButton() {Dock = DockStyle.Bottom, Text = "Ok", Left = 5, Width = 100, Top = (prompt.Height - 20), DialogResult = DialogResult.OK, Theme = MetroFramework.MetroThemeStyle.Dark };
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;
                prompt.ShowDialog();
            }
        }

    }
}

namespace Security
{
    /// <summary>
    /// Generates a 16 byte Unique Identification code of a computer
    /// Example: 4876-8DB5-EE85-69D3-FE52-8CF7-395D-2EA9
    /// </summary>
    public class FingerPrint
    {
        private static string fingerPrint = string.Empty;
        public static string Value()
        {
            fingerPrint = String.Empty;
            if (string.IsNullOrEmpty(fingerPrint))
            {
                string test = (cpuId() + biosId() + baseId());
                test = test.Replace(" ", "");
                fingerPrint = test;
            }
            return fingerPrint;
        }
        #region Original Device ID Getting Code
        //Return a hardware identifier
        private static string identifier
        (string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            ManagementClass mc = new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                if (mo[wmiMustBeTrue].ToString() == "True")
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                            // nothing here
                        }
                    }
                }
            }
            return result;
        }
        //Return a hardware identifier
        private static string identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            ManagementClass mc = new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                        // nothing here
                    }
                }
            }
            return result;
        }
        private static string cpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            string retVal = identifier("Win32_Processor", "UniqueId");
            if (retVal == "") //If no UniqueID, use ProcessorID
            {
                retVal = identifier("Win32_Processor", "ProcessorId");
                if (retVal == "") //If no ProcessorId, use Name
                {
                    retVal = identifier("Win32_Processor", "Name");
                    if (retVal == "") //If no Name, use Manufacturer
                    {
                        retVal = identifier("Win32_Processor", "Manufacturer");
                    }
                    //Add clock speed for extra security
                    //retVal += identifier("Win32_Processor", "MaxClockSpeed");
                }
            }
            return retVal;
        }
        //BIOS Identifier
        private static string biosId()
        {
            return identifier("Win32_BIOS", "Manufacturer")
            + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
            + identifier("Win32_BIOS", "IdentificationCode")
            + identifier("Win32_BIOS", "SerialNumber")
            + identifier("Win32_BIOS", "ReleaseDate")
            + identifier("Win32_BIOS", "Version");
        }
        //Main physical hard drive ID
        private static string diskId()
        {
            return identifier("Win32_DiskDrive", "Model")
            + identifier("Win32_DiskDrive", "Manufacturer")
            + identifier("Win32_DiskDrive", "Signature")
            + identifier("Win32_DiskDrive", "TotalHeads");
        }
        //Motherboard ID
        private static string baseId()
        {
            return identifier("Win32_BaseBoard", "Model")
            + identifier("Win32_BaseBoard", "Manufacturer")
            + identifier("Win32_BaseBoard", "Name")
            + identifier("Win32_BaseBoard", "SerialNumber");
        }
        //Primary video controller ID
        private static string videoId()
        {
            return identifier("Win32_VideoController", "DriverVersion")
            + identifier("Win32_VideoController", "Name");
        }
        //First enabled network card ID
        private static string macId()
        {
            return identifier("Win32_NetworkAdapterConfiguration",
                "MACAddress", "IPEnabled");
        }
        #endregion
    }
}