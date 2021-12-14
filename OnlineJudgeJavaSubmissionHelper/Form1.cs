using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Alsing.SourceCode;

namespace OnlineJudgeJavaSubmissionHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(_readJavaCode);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(_readCodeProgessChanged);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_readCodeWorkCompleted);
            // readJavaCode = new ReadJavaCode(_readJavaCode);
        }

        private void _readCodeWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private void _readCodeProgessChanged(object sender, ProgressChangedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private readonly ComponentResourceManager _resources = new ComponentResourceManager(typeof(Form1));

        // public delegate void ReadJavaCode();
        // public ReadJavaCode readJavaCode;
        public void _readJavaCode(object sender, DoWorkEventArgs e)
        {
            using (StreamReader sr = new StreamReader(textBox1.Text))
            {
                // syntaxBoxControl1.Text = sr.ReadToEndAsync().Result;
                // var stream = sr.BaseStream;
                // syntaxBoxControl1.Document = new SyntaxDocument();
                // stream.Length+"";
                var list = new List<string>();
                int i = 0;
                while (!sr.EndOfStream)
                {
                    list.Add(sr.ReadLine());
                    backgroundWorker1.ReportProgress(i++);
                }
                syntaxBoxControl1.Document.Lines = list.ToArray();
            }
        }

        private string lastLocation = null;
        private void button1_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                return;
            }
            var dialog = new OpenFileDialog
            {
                Filter = @"java file|*.java|any file|*.*",
                // Title = this._resources.GetString("Form1_button1_Click_Choose_Hint"),
                Title = @"Please choose the java file that you want to submit.",
                InitialDirectory = (lastLocation==null? Environment.CurrentDirectory : lastLocation ) 
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.FileName;
                lastLocation = Path.GetDirectoryName(dialog.FileName);
                backgroundWorker1.RunWorkerAsync();
                // this.BeginInvoke(readJavaCode);
                // Thread thread = new Thread(new ThreadStart(_readJavaCode));
                // thread.IsBackground = true;
                // thread.Start();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        //一行只能include一次.
        //预处理package和import
        private string[] IncludeJava(string[] input)
        {
            var documentLines = new List<string>(input);
            var imports = new List<string>();
            for (int i = 0; i < documentLines.Count; i++)
            {
                //#include "test.java"
                Match match = Regex.Match(documentLines[i], "//\\s*#\\s*include \"(?<file_name>.*?.java)\"");
                if (match.Success)
                {
                    GroupCollection groups = match.Groups;
                    var file_name = groups["file_name"].Value;
                    // MessageBox.Show(file_name);
                    if (lastLocation==null)
                    {
                        MessageBox.Show("Code not loaded");
                    }
                    // var directoryInfo = new DirectoryInfo(lastLocation);
                    // var directoryInfo = new DirectoryInfo(lastLocation);
                    try
                    {
                        using var streamReader = new StreamReader(Path.Combine(lastLocation, file_name));
                        // var content = streamReader.ReadToEndAsync().Result;
                        var content = new StringBuilder();
                        while (!streamReader.EndOfStream)
                        {
                            var line = streamReader.ReadLine();
                            if (Regex.IsMatch(line,"import .*?;"))
                            {
                                imports.Add(line);
                            }
                            else if(!Regex.IsMatch(line,"package .*?;"))
                            {
                                content.AppendLine(line);
                            }
                        }
                        documentLines.RemoveAt(i);
                        documentLines.Insert(i, content.ToString()); //前插.
                    }
                    catch (FileNotFoundException e)
                    {
                        MessageBox.Show("无法读取文件!\n" + e);
                    }
                }
                else
                {
                    continue;
                }
            }
            imports.AddRange(documentLines);
            return imports.ToArray();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            syntaxBoxControl1.Enabled = false;
            syntaxBoxControl1.Document.Lines = IncludeJava(syntaxBoxControl1.Document.Lines);
            syntaxBoxControl1.Enabled = true;
        }
        //暴力删除，直接删掉一行
        private string[] RemovePackage(string[] input)
        {
            var documentLines = new List<string>(input);
            for (int i = 0; i < documentLines.Count; i++)
            {
                Match match = Regex.Match(documentLines[i], "package .*?;");
                if (match.Success)
                {
                    documentLines.RemoveAt(i);
                }
            }
            return documentLines.ToArray();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            syntaxBoxControl1.Enabled = false;
            syntaxBoxControl1.Document.Lines = RemovePackage(syntaxBoxControl1.Document.Lines);
            syntaxBoxControl1.Enabled = true;
        }
        private string[] ReplaceMain(string[] input)
        {
            // var documentLines = new List<string>(input);
            // Regex reg = new Regex("//\\s*#pragma\\s+[Oo][Jj]\\s+[Mm]ain\n+public\\s+class\\s+\\S+\\s");
            // for (int i = 0; i < documentLines.Count; i++)
            // {
            //     documentLines[i] = reg.Replace(documentLines[i], "public class Main ");
            // }
            // return documentLines.ToArray();
            Regex reg = new Regex("//\\s*#\\s*pragma\\s+[Oo][Jj]\\s+[Mm]ain\\s*[\r\n]+public\\s+class\\s+\\S+\\s");
            var @join = string.Join("\n", input);
            return reg.Replace(@join, "public class Main ").Split("\n");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            syntaxBoxControl1.Enabled = false;
            syntaxBoxControl1.Document.Lines = ReplaceMain(syntaxBoxControl1.Document.Lines);
            syntaxBoxControl1.Enabled = true;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            syntaxBoxControl1.SelectAll();
            syntaxBoxControl1.Copy();
            MessageBox.Show(@"Copied!");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            syntaxBoxControl1.Enabled = false;
            syntaxBoxControl1.Document.Lines = RemovePackage(IncludeJava(ReplaceMain(syntaxBoxControl1.Document.Lines)));
            syntaxBoxControl1.Enabled = true;
        }
    }
}