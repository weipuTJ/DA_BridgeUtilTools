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
using static System.Math;

namespace DA_FrqtUsedCurves
{
    public class FrqtUsedCurves
    {
        #region 抛物线
        /// <summary>
        /// 通过两个端点和中间垂点绘制抛物线
        /// </summary>
        [CommandMethod("DA_Prbl")]
        public static void DA_Prbl()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            Point3d pt1, pt2, pt3 = new Point3d();
            int nPt;
            PromptPointOptions optPt1 = new PromptPointOptions("\n请选择第一个端点：");
            PromptPointResult resPt1 = ed.GetPoint(optPt1);
            if (resPt1.Status != PromptStatus.OK)
            {
                return; //选择失败结束命令
            }
            else
            {
                pt1 = resPt1.Value;
            }
            PromptPointOptions optPt2 = new PromptPointOptions("\n请选择跨中垂点：");
            optPt2.BasePoint = pt1;
            PromptPointResult resPt2 = ed.GetPoint(optPt2);
            if (resPt2.Status != PromptStatus.OK)
            {
                return; //选择失败结束命令
            }
            else
            {
                pt2 = resPt2.Value;
            }
            PromptPointOptions optPt3 = new PromptPointOptions("\n请选择第二个端点：");
            optPt3.BasePoint = pt2;
            PromptPointResult resPt3 = ed.GetPoint(optPt3);
            if (resPt3.Status != PromptStatus.OK)
            {
                return; //选择失败结束命令
            }
            else
            {
                pt3 = resPt3.Value;
            }
            if (Abs((pt1.X + pt3.X) / 2 - pt2.X) > 0.01) //跨中垂点并非位于跨中
            {
                Application.ShowAlertDialog("您选择的垂点并非位于跨中，请重新选择！");
                return;
            }
            PromptIntegerOptions optInt = new PromptIntegerOptions("\n请选择多段线分段数量<20>:");
            optInt.DefaultValue = 20;
            PromptIntegerResult resInt = ed.GetInteger(optInt);
            if (resInt.Status == PromptStatus.OK)
                nPt = resInt.Value;
            else
                nPt = optInt.DefaultValue;
            Polyline pline = new Polyline();
            double xPt, yPt;
            double h = (pt3.X - pt1.X) / nPt; //多段线步长
            double f = (pt3.Y + pt1.Y) / 2 - pt2.Y;
            for (int i = 0; i <= nPt; i++)
            {
                xPt = pt1.X + i * h;
                yPt = PrblY(xPt, pt1.X, pt1.Y, pt3.X, pt3.Y, f);
                pline.AddVertexAt(i, new Point2d(xPt, yPt), 0, 0, 0);
            }
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            pline.TransformBy(mt);
            db.AddToModelSpace(pline);
        }
        #endregion

        #region 近似悬链线
        /// <summary>
        /// 通过两个端点和中间垂点绘制悬链线
        /// </summary>
        [CommandMethod("DA_Ctnr")]
        public static void DA_Ctnr()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            Point3d pt1, pt2, pt3 = new Point3d();
            int nPt;
            PromptPointOptions optPt1 = new PromptPointOptions("\n请选择第一个端点：");
            PromptPointResult resPt1 = ed.GetPoint(optPt1);
            if (resPt1.Status != PromptStatus.OK)
            {
                return; //选择失败结束命令
            }
            else
            {
                pt1 = resPt1.Value;
            }
            PromptPointOptions optPt2 = new PromptPointOptions("\n请选择跨中垂点：");
            optPt2.BasePoint = pt1;
            PromptPointResult resPt2 = ed.GetPoint(optPt2);
            if (resPt2.Status != PromptStatus.OK)
            {
                return; //选择失败结束命令
            }
            else
            {
                pt2 = resPt2.Value;
            }
            PromptPointOptions optPt3 = new PromptPointOptions("\n请选择第二个端点：");
            optPt3.BasePoint = pt2;
            PromptPointResult resPt3 = ed.GetPoint(optPt3);
            if (resPt3.Status != PromptStatus.OK)
            {
                return; //选择失败结束命令
            }
            else
            {
                pt3 = resPt3.Value;
            }
            if (Abs((pt1.X + pt3.X) / 2 - pt2.X) > 0.01) //跨中垂点并非位于跨中
            {
                Application.ShowAlertDialog("您选择的垂点并非位于跨中，请重新选择！");
                return;
            }
            PromptIntegerOptions optInt = new PromptIntegerOptions("\n请选择多段线分段数量<20>:");
            optInt.DefaultValue = 20;
            PromptIntegerResult resInt = ed.GetInteger(optInt);
            if (resInt.Status == PromptStatus.OK)
                nPt = resInt.Value;
            else
                nPt = optInt.DefaultValue;
            Polyline pline = new Polyline();
            double xPt, yPt;
            double h = (pt3.X - pt1.X) / nPt; //多段线步长
            double f = (pt3.Y + pt1.Y) / 2 - pt2.Y;
            double c = FindC(pt1.X, pt1.Y, pt3.X, pt3.Y, f);
            for (int i = 0; i <= nPt; i++)
            {
                xPt = pt1.X + i * h;
                yPt = CtnrY(xPt, pt1.X, pt1.Y, pt3.X, pt3.Y, c);
                pline.AddVertexAt(i, new Point2d(xPt, yPt), 0, 0, 0);
            }
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            pline.TransformBy(mt);
            db.AddToModelSpace(pline);
        }
        #endregion
        /// <summary>
        /// 通过跨中垂度及端点坐标计算抛物线Y坐标
        /// </summary>
        /// <param name="x">待计算点x坐标</param>
        /// <param name="x1">第一端点x坐标</param>
        /// <param name="y1">第一端点y坐标</param>
        /// <param name="x2">第二端点x坐标</param>
        /// <param name="y2">第二端点y坐标</param>
        /// <param name="f"> 跨中垂度</param>
        /// <returns>抛物线上对应x的y坐标</returns>
        public static double PrblY(double x, double x1, double y1, double x2, double y2, double f)
        {
            double y;
            y = 4 * f / Pow((x2 - x1), 2) * (x - x1) * ((x - x1) - (x2 - x1)) + (y2 - y1) / (x2 - x1) * (x - x1) + y1;
            return y;
        }
        /// <summary>
        /// 通过悬链线参数c(p/H)及端点坐标计算悬链线Y坐标
        /// </summary>
        /// <param name="x">待计算点x坐标</param>
        /// <param name="x1">第一端点x坐标</param>
        /// <param name="y1">第一端点y坐标</param>
        /// <param name="x2">第二端点x坐标</param>
        /// <param name="y2">第二端点y坐标</param>
        /// <param name="c"> 悬链线参数，p/H</param>
        /// <returns>悬链线上对应x的y坐标</returns>
        public static double CtnrY(double x, double x1, double y1, double x2, double y2, double c)
        {
            double y, c1, c2, alpha, beta;
            beta = c * (x2 - x1) / 2;
            double s = c * (y2 - y1) / 2 / Sinh(beta);
            alpha = Log(s + Sqrt(s * s + 1)) - beta;
            c1 = alpha - c * x1;
            c2 = y1 - Cosh(alpha) / c;
            y = 1 / c * Cosh(c * x + c1) + c2;
            return y;
        }
        /// <summary>
        /// 通过悬链线端点及跨中垂度计算较为精确的悬链线参数c(p/H)
        /// </summary>
        /// <param name="x1">第一端点x坐标</param>
        /// <param name="y1">第一端点y坐标</param>
        /// <param name="x2">第二端点x坐标</param>
        /// <param name="y2">第二端点y坐标</param>
        /// <param name="f"> 跨中垂度</param>
        /// <returns>悬链线参数c(p/H)</returns>
        public static double FindC(double x1, double y1, double x2, double y2, double f)
        {
            double c, ym, fCal;
            c = 8 * f / (x2 - x1) / (x2 - x1);
            ym = CtnrY((x1 + x2) / 2, x1, y1, x2, y2, c);
            if (Abs((y1 + y2) / 2 - ym - f) > 1e-3)
            {
                fCal = f - ((y1 + y2) / 2 - ym - f);
                c = 8 * fCal / (x2 - x1) / (x2 - x1);
                ym = CtnrY((x1 + x2) / 2, x1, y1, x2, y2, c);
            }
            return c;
        }
    }
}
