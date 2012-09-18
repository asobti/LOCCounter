using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace LOCCounter_v1
{
    /// <summary>
    /// Interaction logic for whitelist.xaml
    /// </summary>
    public partial class whitelist : Window
    {
        private static FileExtensions extensions;
        public whitelist(FileExtensions incomingextensions)
        {            
            InitializeComponent();
            extensions = incomingextensions;
            PopulateExtensionBoxes();
        }

        private void PopulateExtensionBoxes()
        {
            tbxPrograms.Clear();
            tbxScripts.Clear();
            tbxOthers.Clear();
            tbxCustom.Clear();
            

            foreach (string extension in extensions.Program.extensions)
            {
                tbxPrograms.Text += Environment.NewLine + extension.ToLower();
            }            
            foreach (string extension in extensions.Script.extensions)
            {
                tbxScripts.Text += Environment.NewLine + extension.ToLower();
            }
            foreach (string extension in extensions.Other.extensions)
            {
                tbxOthers.Text += Environment.NewLine + extension.ToLower();
            }            
            foreach (string extension in extensions.Custom.extensions)
            {
                tbxCustom.Text += Environment.NewLine + extension.ToLower();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string newExt = tbxAdd.Text.ToUpper().Replace(".", "");
            tbxResult.Text = String.Empty;

            if (!String.IsNullOrEmpty(newExt))
            {
                if (isWhitelisted(newExt))
                {
                    tbxResult.Text = "Extension already exists";
                }
                else
                {
                    extensions.Custom.extensions.Add(newExt);
                    tbxCustom.Text += Environment.NewLine + newExt.ToLower();
                    tbxResult.Text = "Custom extension added";
                    tbxAdd.Clear();
                }
            }
        }

        private bool isWhitelisted(string extension)
        {
            if (extensions.Script.extensions.Contains(extension))
            {
                return true;
            }
            else if (extensions.Program.extensions.Contains(extension))
            {
                return true;
            }
            else if (extensions.Other.extensions.Contains(extension))
            {
                return true;
            }
            else if (extensions.Custom.extensions.Contains(extension))
            {
                return true;
            }

            return false;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = "fileextension.xml";
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(FileExtensions));
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                    serializer.Serialize(fs, extensions);

                tbxSaveResult.Text = "Whitelist saved";
                
            }
            catch (Exception ex)
            {
                tbxSaveResult.Text = "Unable to save whitelist. Exception: " + ex.Message;
            }
            
        }
    }
}
