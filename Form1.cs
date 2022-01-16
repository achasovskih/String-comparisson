using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form1 : Form
    {
        enum diffType : int
        {
            OldLine = 1,
            ModifiedLine,
            DeletedLine
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            var fileInfo = OpenFileDlg();

            if (fileInfo == null)
                return;

            richTextBox1.AppendText(File.ReadAllText(fileInfo.FullName));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            var fileInfo = OpenFileDlg();

            if (fileInfo == null)
                return;

            richTextBox2.AppendText(File.ReadAllText(fileInfo.FullName));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox3.Clear();

            string file1 = this.richTextBox1.Text;
            string file2 = this.richTextBox2.Text;

            // simple optimization
            if (file1.GetHashCode() == file2.GetHashCode())
            {
                this.richTextBox3.SelectionColor = Color.Green;
                this.richTextBox3.AppendText(file1);
                return;
            }

            if (file2.Length == 0 && file1.Length > 0)
            {
                this.richTextBox3.SelectionColor = Color.Red;
                this.richTextBox3.AppendText(file1);
                return;
            }

            if (file1.Length == 0 && file2.Length > 0)
            {
                this.richTextBox3.SelectionColor = Color.Orange;
                this.richTextBox3.AppendText(file2);
                return;
            }

            var list1 = file1.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
            var list2 = file2.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);

            var diffs = (list1.Length >= list2.Length) ? Diff(list1, list2, true)
                                                       : Diff(list2, list1, false);

            foreach (var d in diffs)
            {

                switch (d.Type)
                {
                    case diffType.OldLine:
                        this.richTextBox3.SelectionColor = Color.Green;
                        break;
                    case diffType.ModifiedLine:
                        this.richTextBox3.SelectionColor = Color.Orange;
                        break;
                    case diffType.DeletedLine:
                        this.richTextBox3.SelectionColor = Color.Red;
                        break;
                    default:
                        break;
                };

                this.richTextBox3.AppendText(d.Value + "\n");
            }
        }

        private FileInfo OpenFileDlg()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "txt | *.txt";

            DialogResult result = dlg.ShowDialog();

            string filePath;

            if (result == DialogResult.OK)
            {
                filePath = dlg.FileName;
                return new FileInfo(filePath);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file1">Larger file</param>
        /// <param name="file2"></param>
        /// <param name="isSrc">flag if file1 para is source of target file</param>
        /// <returns></returns>
        private List<(string Value, diffType Type)> Diff(string[] list1, string[] list2, bool isSrc)
        {
            List<(string, diffType)> diffList = new List<(string, diffType)>(list1.Length);

            for (int i = 0; i < list1.Length; i++)
            {
                if (list2.Contains(list1[i]))
                {
                    diffList.Add((list1[i], diffType.OldLine));
                }
                else
                {
                    diffList.Add((list1[i], (isSrc ? diffType.DeletedLine : diffType.ModifiedLine)));

                    float highestMutchCoff = 0;
                    float matchCoff = 0;
                    int index = 0;

                    for (int j = 0; j < list2.Length; j++)
                    {
                        matchCoff = TanimotoCoff(list1[i], list2[j]);

                        if (highestMutchCoff < matchCoff)
                        {
                            highestMutchCoff = matchCoff;
                            index = j;
                        }

                        if (highestMutchCoff < 0.9f && highestMutchCoff > 0.5f && !list1.Contains(list2[index]))
                        {
                            diffList.Add((list2[index], diffType.ModifiedLine));
                            break;
                        }
                    }
                }
            }

            foreach (var item in list2) 
            {
                if (diffList.Find(x => x.Item1 == item) == default)
                {
                    diffList.Add((item, diffType.ModifiedLine));
                }
            }

            return diffList;
        } 

        private float TanimotoCoff(string s1, string s2)
        {
            int a = s1.Length, b = s2.Length;
            float c = 0f;

            foreach (var sym in s1)
            {
                if (s2.Contains(sym))
                {
                    c += 1;
                }
            }
            return c / (a + b - c);
        }

    }
}
