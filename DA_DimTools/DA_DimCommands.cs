using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using DotNetARX;

namespace DA_DimTools
{
    public class DA_DimCommands
    {
        double scale = 100;//绘图比例
        /// <summary>
        /// 绘制圆弧半径的箭头标注（默认圆弧标注太丑，且容易和其他标注遮盖）
        /// </summary>
        [CommandMethod("DA_DraArrowStyle")]
        public void DimRadiusArrowStyle()
        {            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Arc arc = new Arc();//初始化圆弧
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择圆弧
                PromptEntityOptions arcOpt = new PromptEntityOptions("\n选择圆弧");
                arcOpt.SetRejectMessage("请选择圆弧对象！");
                arcOpt.AddAllowedClass(typeof(Arc),false);
                PromptEntityResult arcRes = ed.GetEntity(arcOpt);
                if (arcRes.Status != PromptStatus.OK) return;
                arc = arcRes.ObjectId.GetObject(OpenMode.ForRead) as Arc;
                CircularArc3d arc3d = new CircularArc3d(arc.StartPoint,arc.Center.PolarPoint((arc.StartAngle+arc.EndAngle)/2,arc.Radius),arc.EndPoint);
                //2.设置标注比例
                PromptDoubleOptions scaleOpt = new PromptDoubleOptions($"\n设置标注比例<{scale}>");
                scaleOpt.AllowNegative = false;
                scaleOpt.AllowZero = false;
                scaleOpt.AllowNone = true;
                PromptDoubleResult scaleRes = ed.GetDouble(scaleOpt);
                if (scaleRes.Status == PromptStatus.OK) scale = scaleRes.Value;
                //3.绘制圆弧半径箭头标注
                db.ArrowRadiusDim(arc3d, scale);
                trans.Commit();//执行事务处理
            }
        }
        /// <summary>
        /// 标注直线或圆弧长度
        /// </summary>
        [CommandMethod("DA_DimLength")]
        public void DimLength()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //1.选择直线或圆弧
                PromptEntityOptions curveOpt = new PromptEntityOptions("\n选择直线或圆弧");
                curveOpt.SetRejectMessage("请选择直线或圆弧对象！");
                curveOpt.AddAllowedClass(typeof(Line), false);
                curveOpt.AddAllowedClass(typeof(Arc), false);
                PromptEntityResult curveRes = ed.GetEntity(curveOpt);
                if (curveRes.Status != PromptStatus.OK) return;
                Entity ent = curveRes.ObjectId.GetObject(OpenMode.ForRead) as Entity;
                //2.设置标注比例
                PromptDoubleOptions scaleOpt = new PromptDoubleOptions($"\n设置标注比例<{scale}>");
                scaleOpt.AllowNegative = false;
                scaleOpt.AllowZero = false;
                scaleOpt.AllowNone = true;
                PromptDoubleResult scaleRes = ed.GetDouble(scaleOpt);
                if (scaleRes.Status == PromptStatus.OK) scale = scaleRes.Value;
                //3.对直线和圆弧分别进行标注
                if (ent is Line)//如果为直线
                {
                    Line line = ent as Line;
                    LineSegment3d line3d = new LineSegment3d(line.StartPoint, line.EndPoint);
                    db.LineLengthDim(line3d, scale);
                }
                else if(ent is Arc)//如果为圆弧
                {
                    Arc arc = ent as Arc;
                    CircularArc3d arc3d = new CircularArc3d(arc.StartPoint, 
                        arc.Center.PolarPoint((arc.StartAngle + arc.EndAngle) / 2, arc.Radius), arc.EndPoint);
                    db.ArcLengthDim(arc3d, scale);
                }               
                trans.Commit();
            }
            
        }
    }
}
