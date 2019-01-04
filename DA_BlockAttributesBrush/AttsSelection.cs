using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace DA_BlockAttributesBrush
{
    public partial class AttsSelection : Form
    {
        public AttsSelection()
        {
            InitializeComponent();
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            //设置全选状态
            for (int i = 0; i < checkedListBoxAtts.Items.Count; i++)
                checkedListBoxAtts.SetItemChecked(i, true);
        }

        private void buttonCancelAll_Click(object sender, EventArgs e)
        {
            //设置全部撤销状态
            for (int i = 0; i < checkedListBoxAtts.Items.Count; i++)
                checkedListBoxAtts.SetItemChecked(i, false);
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
                tgtOpt.MessageForAdding = "选择要刷新的块参照";
                PromptSelectionResult tgtRes = ed.GetSelection(tgtOpt);
                ObjectId[] tgtIds = tgtRes.Value.GetObjectIds();
                foreach (ObjectId tgtId in tgtIds)
                {
                    DBObject tgtObj = tgtId.GetObject(OpenMode.ForRead);
                    BlockReference tgtBlkRef;
                    if (tgtObj is BlockReference)
                    {
                        tgtBlkRef = tgtObj as BlockReference;
                        if (tgtBlkRef.BlockName == BlockAttributesBrush.orgBlkRef.BlockName)//同名块才做刷新
                        {
                            foreach (ObjectId attId in tgtBlkRef.AttributeCollection)
                            {
                                AttributeReference attRef = attId.GetObject(OpenMode.ForWrite) as AttributeReference;
                                if (BlockAttributesBrush.atts.ContainsKey(attRef.Tag.ToUpper()))//如果前面属性字典中含有该属性项
                                {
                                    if(checkedListBoxAtts.CheckedItems.Contains(attRef.Tag))
                                        attRef.TextString = BlockAttributesBrush.atts[attRef.Tag.ToUpper()];//如果在checkBoxList中选择了
                                }
                                attRef.DowngradeOpen();//安全起见，将打开模式降为写模式
                            }
                        }
                    }
                }
                ed.WriteMessage("本命令仅刷新同名块，请确保目标块与源块同名！");
                trans.Commit();
            }
        }
    }
}
