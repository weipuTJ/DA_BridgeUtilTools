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

namespace DA_ElevationTool
{
    public class DA_Elevation
    {
        private double baseElevation = 0;//表示图形中的Y坐标与实际标高的差值
        private double scaleFactor = 100;//标高符号的放大比例
        /// <summary>
        /// 自动填加标高
        /// </summary>
        [CommandMethod("DA_ElvtDenote")]
        public void DA_ElvtDenote()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            //提示用户选择基准点
            PromptPointOptions ptOpt = new PromptPointOptions("\n请选择基准点或[上一个(P)/UCS(U)]<P>");
            ptOpt.Keywords.Add("P");//添加“上一个”关键字
            ptOpt.Keywords.Add("U");//添加“UCS”关键字
            ptOpt.Keywords.Default = "P";//默认关键字为“P”，即“上一个”
            ptOpt.AppendKeywordsToMessage = false;//提示列表中不显示关键字
            PromptPointResult ptRes = ed.GetPoint(ptOpt);//获取基准点
            //如果即未选择点也未输入关键字，则发出错误信息，并返回
            if(ptRes.Status != PromptStatus.OK && ptRes.Status != PromptStatus.Keyword)
            {
                ed.WriteMessage("\n请选取基准点或输入关键字！");
                return;
            }
            //如果选取了基准点
            else if(ptRes.Status == PromptStatus.OK)
            {
                PromptDoubleOptions elvOpt = new PromptDoubleOptions("\n请输入该基准点标高");
                PromptDoubleResult elvRes = ed.GetDouble(elvOpt);
                while (elvRes.Status != PromptStatus.OK)
                {
                    elvRes = ed.GetDouble(elvOpt);
                }
                baseElevation = ptRes.Value.Y-elvRes.Value;
            }
            else if(ptRes.Status == PromptStatus.Keyword)
            {
                switch(ptRes.StringResult)
                {
                    case "P":   
                        break;
                    case "U":
                        baseElevation = 0;
                        break;
                }
            }
            PromptDoubleOptions douOpt = new PromptDoubleOptions($"\n请输入标注比例<{scaleFactor}>");//提示输入标注比例
            douOpt.AllowNegative = false;//不允许负值
            douOpt.AllowZero = false;//不允许0
            douOpt.AllowNone = true;//允许回车
            PromptDoubleResult douRes = ed.GetDouble(douOpt);
            //输入正确时，设置标注比例
            if(douRes.Status == PromptStatus.OK)
            {
                scaleFactor = douRes.Value;
            }
            //输入回车时保留现比例
            else if (douRes.Status == PromptStatus.None)
            {

            }
            else
            {
                ed.WriteMessage("\n请输入正确比例！");
                return;
            }
            //提示用户选择标注点
            PromptPointOptions pteOpt = new PromptPointOptions("\n选择标注点");
            PromptPointResult pteRes = ed.GetPoint(pteOpt);
            while(pteRes.Status == PromptStatus.OK)
            {
                //CreateElevationBlock();
                AddElevationAtt();
                InsertElevation(baseElevation,scaleFactor,pteRes.Value);
                pteRes = ed.GetPoint(pteOpt);
            }
            return;
        }
        /// <summary>
        /// 插入带有属性的标高标注块
        /// </summary>
        /// <param name="baseElevation">基准标高差</param>
        /// <param name="scaleFactor">标注缩放比例</param>
        /// <param name="ptElevation">待标注点</param>
        private void InsertElevation(double baseElevation,double scaleFactor,Point3d ptElevation)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Dictionary<string, string> atts = new Dictionary<string, string>();
                atts.Add("ELEVATION", ((ptElevation.Y - baseElevation)/1000).ToString("f3"));//添加标高属性值
                ObjectId spaceId = db.CurrentSpaceId;//获取当前空间（模型空间或布局空间）
                ObjectId layerId = db.Clayer;//获取当前图层的ObjectId
                LayerTableRecord ltr = layerId.GetObject(OpenMode.ForRead) as LayerTableRecord;//获取当前图层的表记录
                string layerName = ltr.Name;//获取当前图层面名
                spaceId.InsertBlockReference(layerName, "DA_ELEVATION", 
                    ptElevation, new Scale3d(scaleFactor), 0, atts);
                trans.Commit();
            }
        }
        /// <summary>
        /// 创建标高标注的块，名称为DA_ELEVATION
        /// </summary>
        /// <returns>DA_ELEVATION块定义的ObjectId</returns>
        private ObjectId CreateElevationBlock()
        {
            ObjectId blockId;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Line lineBt = new Line(new Point3d(-1, 0, 0), new Point3d(1, 0, 0));//标高底面水平线
                Line lineLeft = new Line(Point3d.Origin, Point3d.Origin.PolarPoint(135*Math.PI/180, 2));//左斜线
                Line lineRight = new Line(Point3d.Origin, Point3d.Origin.PolarPoint(45 * Math.PI / 180, 2));//左斜线
                Line lineTp = new Line(Point3d.Origin.PolarPoint(135 * Math.PI / 180, 2), 
                    Point3d.Origin.PolarPoint(135 * Math.PI / 180, 2).PolarPoint(0, 10));//顶面线
                blockId = db.AddBlockTableRecord("DA_ELEVATION", lineBt, lineLeft, lineRight, lineTp);
                trans.Commit();
            }
            return blockId;
        }
        /// <summary>
        /// 在DA_ELEVATION添加表示标高的属性
        /// </summary>
        private void AddElevationAtt()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId blockId = CreateElevationBlock();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                AttributeDefinition attElevation = new AttributeDefinition(
                    Point3d.Origin, "", "ELEVATION", "输入标高值", ObjectId.Null);
                attElevation.Height = 3;//字高为3
                attElevation.HorizontalMode = TextHorizontalMode.TextLeft;//水平方向左对齐
                attElevation.VerticalMode = TextVerticalMode.TextBottom;//竖直方向下对齐
                attElevation.AlignmentPoint = new Point3d(2, 1.8, 0);//对齐位置
                attElevation.Visible = true;//文字可见 
                attElevation.WidthFactor = 0.75;//文字水平比例
                blockId.AddAttsToBlock(attElevation);//将属性定义添加至块定义中
                trans.Commit();
            }
        }
    }
}
