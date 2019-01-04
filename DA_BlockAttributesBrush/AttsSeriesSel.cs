using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using DotNetARX;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace DA_BlockAttributesBrush
{
    public enum NumberType
    {
        数字 = 0,
        括号数字 = 1,
        汉字 = 2,
        括号汉字 = 3
    }
    public partial class AttsSeriesSel : Form
    {
        public AttsSeriesSel()
        {
            InitializeComponent();
        }

        List<string> attsForSeries = new List<string>();
        List<string> prefixForSeries = new List<string>();
        List<string> suffixForSeries = new List<string>();
        List<int> startNumForSeries = new List<int>();
        List<NumberType> numTypeForSeries = new List<NumberType>();

        private void comboBoxAtts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBoxAtts.SelectedItem != null)
            {
                //选中某项属性时，前缀文本框里显示其现有属性值
                textBoxPrefix.Text = BlockAttributesBrush.atts[(string)comboBoxAtts.SelectedItem];
                //序列化CheckBox被激活
                checkBoxIsSeries.Enabled = true;
                checkBoxIsSeries.Checked = false;
                comboBoxNumType.Text = "1";
                textBoxStartNum.Text = "1";
                textBoxSuffix.Text = "";
            }
            else
            {
                //没有选择时，序列化CheckBox被钝化
                checkBoxIsSeries.Enabled = false;
            }
        }

        private void checkBoxIsSeries_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxIsSeries.Checked == true)
            {
                //选中序列化时，激活前缀、编号类别、起始编号、后缀及添加按钮
                textBoxPrefix.Enabled = true;
                comboBoxNumType.Enabled = true;
                textBoxStartNum.Enabled = true;
                textBoxSuffix.Enabled = true;
                buttonAdd.Enabled = true;
            }
            else
            {
                //不选中序列化时，钝化前缀、编号类别、起始编号、后缀及添加按钮
                textBoxPrefix.Enabled = false;
                comboBoxNumType.Enabled = false;
                textBoxStartNum.Enabled = false;
                textBoxSuffix.Enabled = false;
                buttonAdd.Enabled = false;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if(attsForSeries.Contains((string)comboBoxAtts.SelectedItem))
            {
                MessageBox.Show("该属性已经序列化，如要修改，请删除相关记录！");
                return;
            }
            else
            {
                attsForSeries.Add((string)comboBoxAtts.SelectedItem);
                prefixForSeries.Add(textBoxPrefix.Text);
                suffixForSeries.Add(textBoxSuffix.Text);
                switch (comboBoxNumType.Text)
                {
                    case "1":
                        numTypeForSeries.Add(NumberType.数字);
                        break;
                    case "(1)":
                        numTypeForSeries.Add(NumberType.括号数字);
                        break;
                    case "一":
                        numTypeForSeries.Add(NumberType.汉字);
                        break;
                    case "（一）":
                        numTypeForSeries.Add(NumberType.括号汉字);
                        break;
                }
                int startNum;
                if (int.TryParse(textBoxStartNum.Text, out startNum))
                {
                    startNumForSeries.Add(startNum);
                }
                else
                {
                    MessageBox.Show("起始编号非整数！");
                    return;
                }
                listBoxAtts.Items.Add((string)comboBoxAtts.SelectedItem + ":" + textBoxPrefix.Text
                    + $"*格式：{comboBoxNumType.Text}；起始：" + startNum.ToString() + "*" + textBoxSuffix.Text);
            }            
        }

        private void listBoxAtts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBoxAtts.SelectedItems != null)
            {
                buttonRemove.Enabled = true;
            }
            else
            {
                buttonRemove.Enabled = false;
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            foreach(int i in listBoxAtts.SelectedIndices)
            {
                listBoxAtts.Items.RemoveAt(i);
                attsForSeries.RemoveAt(i);
                prefixForSeries.RemoveAt(i);
                suffixForSeries.RemoveAt(i);
                startNumForSeries.RemoveAt(i);
                numTypeForSeries.RemoveAt(i);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Dispose();
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //选择要刷新的块，可以任选，挑出其中的同名快
                PromptSelectionOptions tgtOpt = new PromptSelectionOptions();
                tgtOpt.MessageForAdding = "选择要序列化的同名块参照";
                PromptSelectionResult tgtRes = ed.GetSelection(tgtOpt);
                ObjectId[] tgtIds = tgtRes.Value.GetObjectIds();
                List<BlockReference> tgtBlkRefs = new List<BlockReference>();
                foreach (ObjectId tgtId in tgtIds)
                {
                    DBObject tgtObj = tgtId.GetObject(OpenMode.ForRead);
                    BlockReference tgtBlkRef;
                    if (tgtObj is BlockReference)
                    {
                        tgtBlkRef = tgtObj as BlockReference;
                        if (tgtBlkRef.BlockName == BlockAttributesBrush.orgBlkRef.BlockName)//同名块才做刷新
                        {
                            tgtBlkRefs.Add(tgtBlkRef);//获得有效的同名块
                        }
                    }
                }
                //将图块排序
                var sortedTgtBlkRefs = from br in tgtBlkRefs
                                       orderby br.Position.Y descending, br.Position.X //先行后列
                                       select br;
                uint numSeries = 0;
                foreach (BlockReference tgtBlkRef in sortedTgtBlkRefs)
                {                    
                    foreach (ObjectId attId in tgtBlkRef.AttributeCollection)
                    {                        
                        AttributeReference attRef = attId.GetObject(OpenMode.ForWrite) as AttributeReference;
                        if (BlockAttributesBrush.atts.ContainsKey(attRef.Tag.ToUpper()))//如果前面属性字典中含有该属性项
                        {
                            if (attsForSeries.Contains(attRef.Tag))//如果在序列化属性中
                            {
                                int i = attsForSeries.IndexOf(attRef.Tag);
                                switch(numTypeForSeries[i])
                                {
                                    case NumberType.数字:
                                        attRef.TextString = prefixForSeries[i] + (startNumForSeries[i] + numSeries).ToString()
                                            + suffixForSeries[i];
                                        break;
                                    case NumberType.括号数字:
                                        attRef.TextString = prefixForSeries[i] + "("+(startNumForSeries[i] + numSeries).ToString()+")"
                                            + suffixForSeries[i];
                                        break;
                                    case NumberType.汉字:
                                        attRef.TextString = prefixForSeries[i] + TextTools.IntToChChar((startNumForSeries[i] + numSeries).ToString())
                                            + suffixForSeries[i];
                                        break;
                                    case NumberType.括号汉字:
                                        attRef.TextString = prefixForSeries[i] + "（"+TextTools.IntToChChar((startNumForSeries[i] + numSeries).ToString())+"）"
                                            + suffixForSeries[i];
                                        break;
                                }
                            }
                        }
                        attRef.DowngradeOpen();//安全起见，将打开模式降为写模式
                        
                    }
                    numSeries++;
                }
                trans.Commit();
            }
        }
    }
}
