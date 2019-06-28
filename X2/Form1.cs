/*
 * WNIOSKI
 * pomiędzy kolejnymi krokami konieczne są waity, bo inaczej wyścig i błędy ewaluacji; driver ma wait
 * zmiana na formularzu powoduje zmianę xpath, PK: może lepiej jakiś selector css
 * 
 */

using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
//using NUnit.Framework;
using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;


namespace X2
{
    public partial class Form1 : Form
    {      

        public Form1()
        {
            InitializeComponent();
            numericUpDown1.Value = Globals.minRow;
            numericUpDown2.Value = Globals.maxRow;
            checkBox1.Checked = Globals.killDriver;            
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();            
        }

        

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Globals.fileName = openFileDialog1.FileName;
            textBox1.Text = openFileDialog1.FileName;            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new CompleteTest().Run(this);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(Text != null)
            {
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                button4.Enabled = true;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Globals.minRow = Decimal.ToInt32(numericUpDown1.Value);
            Console.WriteLine("min: " + Globals.minRow.ToString() + "max: " + Globals.maxRow.ToString());
            if (numericUpDown2.Value <= numericUpDown1.Value)
            {
                numericUpDown2.Value = numericUpDown1.Value;
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Globals.maxRow = Decimal.ToInt32(numericUpDown2.Value);
            Console.WriteLine("min: " + Globals.minRow.ToString() + "max: " + Globals.maxRow.ToString());
            if(numericUpDown2.Value <= numericUpDown1.Value)
            {
                numericUpDown1.Value = numericUpDown2.Value;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Globals.killDriver = checkBox1.Checked;
        }

        private void UpdateResult(object sender, EventArgs e)
        {
            if(textBox2.Text != Globals.testResult)
            {
                textBox2.Text = Globals.testResult;
            }
        }

        /*
        protected override void OnFormClosing(FormClosingEventArgs e)
        {            
            base.OnFormClosing(e);                        
            //Globals.TearDownTest(); //powoduje błędy, bo druwi wątek próbuje uzywać drivera, a pierwszy go zabija
        }
        */

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            BringToFront();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }
    }
}


