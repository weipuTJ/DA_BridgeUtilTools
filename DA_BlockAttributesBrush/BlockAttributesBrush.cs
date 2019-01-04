using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System.Windows.Forms;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace DA_BlockAttributesBrush
{
    
    public class BlockAttributesBrush
    {
        public static BlockReference orgBlkRef;//源块参照
        public static Dictionary<string, string> atts;//源块参照中的属性名及属性值

        /// <summary>
        /// 将目标块中的全部属性设置为与源块相同
        /// </summary>
        [CommandMethod("DA_BlkAttBrushAll")]
        public void BlkAttBrushAll()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //提示用户选择数据源块参照
                PromptEntityOptions orgOpt = new PromptEntityOptions("选择源格式块");
                orgOpt.SetRejectMessage("选择的不是块！");
                orgOpt.AddAllowedClass(typeof(BlockReference), true);//只能选择块参照
                PromptEntityResult orgRes = ed.GetEntity(orgOpt);
                if(orgRes.Status == PromptStatus.OK) //选择正确
                {
                    orgBlkRef = orgRes.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;
                    if (orgBlkRef.AttributeCollection.Count == 0)
                    {
                        ed.WriteMessage("所选对象不包含属性！");
                        return;
                    }
                    else
                    {
                        atts = new Dictionary<string, string>();//初始化属性集字典
                        foreach(ObjectId attId in orgBlkRef.AttributeCollection)//获得源块属性值
                        {
                            AttributeReference attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                            atts.Add(attRef.Tag.ToUpper(), attRef.TextString);
                        }
                        //选择要刷新的块，可以任选，挑出其中的同名快
                        PromptSelectionOptions tgtOpt = new PromptSelectionOptions();
                        tgtOpt.MessageForAdding="选择要刷新的块参照";
                        PromptSelectionResult tgtRes = ed.GetSelection(tgtOpt);
                        ObjectId[] tgtIds = tgtRes.Value.GetObjectIds();
                        foreach(ObjectId tgtId in tgtIds)
                        {
                            DBObject tgtObj = tgtId.GetObject(OpenMode.ForRead);
                            BlockReference tgtBlkRef;
                            if (tgtObj is BlockReference)
                            {
                                tgtBlkRef = tgtObj as BlockReference;
                                if (tgtBlkRef.BlockName == orgBlkRef.BlockName)//同名块才做刷新
                                {
                                    foreach (ObjectId attId in tgtBlkRef.AttributeCollection)
                                    {
                                        AttributeReference attRef = attId.GetObject(OpenMode.ForWrite) as AttributeReference;
                                        if (atts.ContainsKey(attRef.Tag.ToUpper()))//如果前面属性字典中含有该属性项
                                        {
                                            attRef.TextString = atts[attRef.Tag.ToUpper()];
                                        }
                                        attRef.DowngradeOpen();//安全起见，将打开模式降为写模式
                                    }
                                }
                            }
                        }
                    }    
                }
                ed.WriteMessage("本命令仅刷新同名块，请确保目标块与源块同名！");
                trans.Commit();
            }
        }
        /// <summary>
        /// 使目标块中选择的属性与源块相同
        /// </summary>
        [CommandMethod("DA_BlkAttsBrush")]
        public void BlkAttsBrush()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            AttsSelection attsSel = new AttsSelection();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //提示用户选择数据源块参照
                PromptEntityOptions orgOpt = new PromptEntityOptions("选择源格式块");
                orgOpt.SetRejectMessage("选择的不是块！");
                orgOpt.AddAllowedClass(typeof(BlockReference), true);//只能选择块参照
                PromptEntityResult orgRes = ed.GetEntity(orgOpt);
                if (orgRes.Status == PromptStatus.OK) //选择正确
                {
                    orgBlkRef = orgRes.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;
                    if (orgBlkRef.AttributeCollection.Count == 0)
                    {
                        ed.WriteMessage("所选对象不包含属性！");
                        return;
                    }
                    else
                    {
                        atts = new Dictionary<string, string>();//初始化属性集字典
                        foreach (ObjectId attId in orgBlkRef.AttributeCollection)//获得源块属性值
                        {
                            AttributeReference attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                            atts.Add(attRef.Tag.ToUpper(), attRef.TextString);
                            attsSel.checkedListBoxAtts.Items.Add(attRef.Tag);
                        }
                    }
                }
                trans.Commit();
            }
            AcadApp.ShowModalDialog(attsSel);
        }
        [CommandMethod("DA_BlkAttsSeries")]
        public void BlkAttsSeries()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            AttsSeriesSel attsSeries = new AttsSeriesSel();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //提示用户选择数据源块参照
                PromptEntityOptions orgOpt = new PromptEntityOptions("选择源格式块");
                orgOpt.SetRejectMessage("选择的不是块！");
                orgOpt.AddAllowedClass(typeof(BlockReference), true);//只能选择块参照
                PromptEntityResult orgRes = ed.GetEntity(orgOpt);
                if (orgRes.Status == PromptStatus.OK) //选择正确
                {
                    orgBlkRef = orgRes.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;
                    if (orgBlkRef.AttributeCollection.Count == 0)
                    {
                        ed.WriteMessage("所选对象不包含属性！");
                        return;
                    }
                    else
                    {
                        atts = new Dictionary<string, string>();//初始化属性集字典
                        foreach (ObjectId attId in orgBlkRef.AttributeCollection)//获得源块属性值
                        {
                            AttributeReference attRef = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                            atts.Add(attRef.Tag.ToUpper(), attRef.TextString);
                            attsSeries.comboBoxAtts.Items.Add(attRef.Tag);
                        }
                    }
                }
                trans.Commit();
            }
            AcadApp.ShowModalDialog(attsSeries);
        }
    }
}
