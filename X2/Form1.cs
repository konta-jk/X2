/*
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;


namespace X2
{
    public partial class Form1 : Form
    {
        public QATestSetup testSetup = new QATestSetup();

        public Form1()
        {
            InitializeComponent();
            numericUpDown1.Value = testSetup.minRow;
            numericUpDown2.Value = testSetup.maxRow;
            checkBox1.Checked = testSetup.killDriver;            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new QATestLauncher(this).Run();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();            
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Globals.fileName = openFileDialog1.FileName; //
            testSetup.fileName = openFileDialog1.FileName; //nowe
            textBox1.Text = openFileDialog1.FileName;            
        }

        //wystawione dla QATestLauncher, który odpala to przez delegata
        public void UpdateResult()
        {
            string output = testSetup.testResult.ToCsvString();

            string s = "";
            foreach (Structs.Variable v in testSetup.variables)
            {
                s += v.name + " = " + v.value + "\r\n";
            }
            if (s.Length > 0)
            {
                output += "\r\nVariables:\r\n" + s;
            }

            output += "\r\nLog:\r\n" + testSetup.log;

            textBox2.Text = output;

            string path = testSetup.GetPathMakeFolder(@"\Logs\");
            path = path + @"\" + testSetup.testRunId + ".txt";
            System.IO.File.WriteAllText(path, output);
        }

        //wystawione dla QATestLauncher, który odpala to przez delegata
        public void UpdateProgress()
        {
            textBox2.Text = testSetup.testResult.ToCsvString();
        }

        private void SaveToFile(string fileName)
        {

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
            testSetup.minRow = Decimal.ToInt32(numericUpDown1.Value);
            //Console.WriteLine("min: " + testSetup.minRow.ToString() + "max: " + testSetup.maxRow.ToString());

            if (numericUpDown2.Value <= numericUpDown1.Value)
            {
                numericUpDown2.Value = numericUpDown1.Value;
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            testSetup.maxRow = Decimal.ToInt32(numericUpDown2.Value);
            //Console.WriteLine("min: " + testSetup.minRow.ToString() + "max: " + testSetup.maxRow.ToString());
            if(numericUpDown2.Value <= numericUpDown1.Value)
            {
                numericUpDown1.Value = numericUpDown2.Value;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            testSetup.killDriver = checkBox1.Checked;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            BringToFront();
            textBox2.SelectionStart = textBox2.TextLength;
            textBox2.ScrollToCaret();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void debugButton_Click(object sender, EventArgs e)
        {
            
        }
    }
}


