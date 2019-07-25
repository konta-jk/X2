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
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;

namespace X2
{
    public partial class Form1 : Form, IQATestLaunchPoint
    {
        public QATestStuff testStuff;

        private float testProgress = 0.0f;

        public Form1()
        {
            InitializeComponent();
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");
        }

        private delegate void UpdateResultDelegate();
        private delegate void UpdateProgressDelegate();
        
        public void OnTestFinish()
        {
            //UpdateResult musi być wywołane za pomocą delegata, inaczej form drze mordę, że inny wątek się dobiera do jego kontrolki
            this.BeginInvoke(new UpdateResultDelegate(UpdateResult));

            if (testStuff.killDriver)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(Settings.killDriverDelay)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki; do settingsów
                testStuff.TearDownTest();
            }

            //testStuff = null; //źródło wielu błędów
        }

        public void OnTestProgress()
        {
            //UpdateProgress musi być wywołane za pomocą delegata, inaczej form drze mordę, że inny wątek się dobiera do jego kontrolki
            this.BeginInvoke(new UpdateProgressDelegate(UpdateProgress));
            
        }

        public void OnTestCancel(string reason)
        {
            MessageBox.Show("TestCanceled, reason: " + reason);
        }

        public DataTable GetTestPlanAsDataTable()
        {
            string fileName = textBox1.Text;

            string extension = Regex.Match(fileName, "\\.[0-9a-z]+$").Value;
            DataTable dataTable;

            switch (extension)
            {
                case ".xlsx":
                    dataTable = ReaderXlsx.ReadExcellSheet(testStuff, fileName);
                    break;

                case ".csv":
                    dataTable = ReaderCsv.ReadCsv(testStuff, fileName);
                    break;

                default:
                    dataTable = ReaderCsv.ReadCsv(testStuff, fileName);
                    break;
            }

            return dataTable;
        }

        public IQATestLaunchPoint GetLaunchPoint()
        {
            return this;
        }

        public QATestStuff GetTestStuff()
        {
            return testStuff;
        }

        private void UpdateResult()
        {
            string output = testStuff.testResult.ToCsvString();

            string s = "";
            foreach (Structs.Variable v in testStuff.variables)
            {
                s += v.name + " = " + v.value + "\r\n";
            }
            if (s.Length > 0)
            {
                output += "\r\nVariables:\r\n" + s;
            }

            output += "\r\nLog:\r\n" + testStuff.logger.GetLogString(); //testStuff.log;

            textBox2.Text = output;


            string path = testStuff.logger.GetFolderPath();
            path = path + @"\Output.txt";
            System.IO.File.WriteAllText(path, output);
            

            button2.Enabled = false;
        }

        private void UpdateProgress()
        {
            textBox2.Text = testStuff.testResult.ToCsvString();
            int currentStep = Math.Min(testStuff.testResult.testStepResults.Count, testStuff.testPlan.testSteps.Count - 1);
            textBox2.Text += "\r\nIn progress: " + testStuff.testPlan.testSteps[currentStep].stepDescription;

            testProgress = ((float)testStuff.testResult.testStepResults.Count / (float)testStuff.testPlan.testSteps.Count);
            progressBar1.Value = (int)Math.Round(100 * testProgress, 0);
        }

        private void StartTest()
        {
            DialogResult dialogResult = 
                MessageBox.Show(Settings.message1,
                "Cześć!", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            

            QATestStuff.QATestStuffOptions stuffOptions = new QATestStuff.QATestStuffOptions();
            stuffOptions.killDriver = checkBox1.Checked;
            stuffOptions.minRow = (int)numericUpDown1.Value;
            stuffOptions.maxRow = (int)numericUpDown2.Value;
            testStuff = new QATestStuff(stuffOptions);
            testStuff.CreateDriver(); //inna klasa można chcieć zrobić testStuff.driver = ...
            testStuff.Init();
            new QATestLauncher(this).Run();

            button2.Enabled = true;
        }

        //przycisk "Testuj"
        private void button4_Click(object sender, EventArgs e)
        {
            StartTest();            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();            
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName;            
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
            numericUpDown2.Minimum = Decimal.ToInt32(numericUpDown1.Value);
            /*
            if (numericUpDown2.Value <= numericUpDown1.Value)
            {
                numericUpDown2.Value = numericUpDown1.Value;
            }
            */
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = Decimal.ToInt32(numericUpDown2.Value);
            /*
            if(numericUpDown2.Value <= numericUpDown1.Value)
            {
                numericUpDown1.Value = numericUpDown2.Value;
            }
            */
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
            //...
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //..
            UpdateResult();
            if (testStuff.killDriver)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(Settings.killDriverDelay)); //aby uzytkownik mógł sie przyjrzeć zakończeniu przed zamknięciem przeglądarki; do settingsów
                testStuff.TearDownTest();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //
        }

        private void button3_Click_1(object sender, EventArgs e)
        {         
            Form2SideConverter formSeleniumHog = new Form2SideConverter();
            formSeleniumHog.ShowDialog(this);

            //to był debug button, poniżej testy validatora
            /*
            Validator V = new Validator();
            DataTable t = new DataTable();
            t.Columns.Add("d");
            t.Columns.Add("a");
            t.Columns.Add("t");
            t.Columns.Add("x");
            DataRow r = t.NewRow();            
            r[0] = "klik";
            r[1] = "Click";
            r[2] = "";
            r[3] = "xs";
            t.Rows.Add(t);

            Console.WriteLine("..." + t.Rows.Count + ", " + r[0].ToString() + r[1].ToString() + r[2].ToString() + r[3].ToString());

            Console.WriteLine(V.ValidateRow(r));
            */
        }
                
    }
}


