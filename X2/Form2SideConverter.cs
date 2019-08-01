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
    public partial class Form2SideConverter : Form
    {
        public Form2SideConverter()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //dodać ifa filtr save dialoga; dorobić funkcję ReadSideSaveSql
            switch (saveFileDialog1.FilterIndex)
            {
                case 1:
                    new SideConverter.SideReader().ReadSideSaveCsv(openFileDialog1.FileName, saveFileDialog1.FileName);
                    break;
                case 2:
                    new SideConverter.SideReader().ReadSideSaveSql(openFileDialog1.FileName, saveFileDialog1.FileName);
                    break;
            }
            
        }
    }
}
