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

namespace DA_TendonTools
{
    public enum DrawType
    {
        LefeEnd = -1,
        RightEnd = 1,
        BothEnd = 0
    };
    public class TendonTools
    {
        internal double scale = 100;//绘图比例缺省值
        internal double kii = 0.0015;//局部偏差影响系数缺省值，1/m
        internal double miu = 0.16;//摩擦系数，无量纲
        internal double Ep = 1.95e5;//钢束弹性模量，MPa
        internal double ctrlStress = 1395;//张拉控制应力，MPa
        internal double workLen = 800;//默认工作长度,mm
        /// <summary>
        /// 单根钢束标注命令
        /// </summary>
        [CommandMethod("DA_TendonAnnotation")]
        public void TendonAnnotation()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //1.选择梁顶缘线
                PromptEntityOptions tpLineOpt = new PromptEntityOptions("\n选择梁顶缘线");
                tpLineOpt.SetRejectMessage("\n顶缘线应为直线、圆弧或多段线");
                tpLineOpt.AddAllowedClass(typeof(Line), true);//可以选择直线
                tpLineOpt.AddAllowedClass(typeof(Polyline), true);//可以选择多段线
                tpLineOpt.AddAllowedClass(typeof(Arc), true);//可以选择圆弧线
                PromptEntityResult tpLineRes = ed.GetEntity(tpLineOpt);
                if (tpLineRes.Status != PromptStatus.OK) return;
                ObjectId tpLineId = tpLineRes.ObjectId;
                Curve tpLine = trans.GetObject(tpLineId, OpenMode.ForRead) as Curve;

                //2.选择钢束线
                PromptEntityOptions tdLineOpt = new PromptEntityOptions("\n选择钢束");
                tdLineOpt.SetRejectMessage("\n钢束应为多段线");
                tdLineOpt.AddAllowedClass(typeof(Polyline), true);//仅能选择多段线      
                PromptEntityResult tdLineRes = ed.GetEntity(tdLineOpt);
                if (tdLineRes.Status != PromptStatus.OK) return;
                ObjectId tdLineId = tdLineRes.ObjectId;
                Polyline tdLine = trans.GetObject(tdLineId, OpenMode.ForRead) as Polyline;

                //判断钢束线是否在顶缘线以内，否则报错返回
                if (tdLine.StartPoint.X < tpLine.StartPoint.X || tdLine.EndPoint.X > tpLine.EndPoint.X)
                {
                    Application.ShowAlertDialog("钢束线超出顶缘线，请检查！");
                    return;
                }

                //3.设置绘图参数
                //3.1 绘图比例
                PromptDoubleOptions scaleOpt = new PromptDoubleOptions($"\n设置绘图比例<{scale}>");
                scaleOpt.AllowNone = true;//允许回车，则采用前次比例
                scaleOpt.AllowNegative = false;//不允许负值
                scaleOpt.AllowZero = false;//不允许零值
                PromptDoubleResult scaleRes = ed.GetDouble(scaleOpt);//获取比例
                if (scaleRes.Status != PromptStatus.OK && scaleRes.Status != PromptStatus.None) return;
                else if (scaleRes.Status == PromptStatus.OK) scale = scaleRes.Value;

                //3.2 尺寸标注绘图位置
                PromptPointOptions posOpt = new PromptPointOptions("\n设置标注线位置");
                PromptPointResult posRes = ed.GetPoint(posOpt);
                if (posRes.Status != PromptStatus.OK) return;
                Point3d pos = posRes.Value;

                //4.开始建立标注
                List<Point3d> ptsH = new List<Point3d>();//创建水平标注点集
                List<Dimension> dims = new List<Dimension>();//创建标注集，存放各类标注
                for (int i = 0; i < tdLine.NumberOfVertices - 1; i++)
                {
                    //4.1 水平点集
                    ptsH.Add(tdLine.GetPoint3dAt(i));

                    //4.2 每段钢束线的长度
                    //4.3 直线标注角度
                    //4.4 圆弧线标注半径
                    if (tdLine.GetSegmentType(i) == SegmentType.Line)
                    {
                        LineSegment3d lineSeg = tdLine.GetLineSegmentAt(i);
                        //4.2 每段钢束线的长度
                        db.LineLengthDim(lineSeg, scale);
                        //4.3 直线标注角度
                        if (tdLine.StartPoint.X < tdLine.EndPoint.X)
                            db.LineAngelDim(lineSeg, !(i == tdLine.NumberOfVertices - 2),scale);
                        else
                            db.LineAngelDim(lineSeg, (i == tdLine.NumberOfVertices - 2), scale);
                    }
                    else if (tdLine.GetSegmentType(i) == SegmentType.Arc)
                    {
                        CircularArc3d arcSeg = tdLine.GetArcSegmentAt(i);
                        //4.2 每段钢束线的长度
                        db.ArcLengthDim(arcSeg, scale);
                        //4.3 圆弧标注半径
                        db.ArrowRadiusDim(arcSeg, scale);
                    }
                    //4.5 竖直距离标注
                    Ray vRay = new Ray();//建立竖直射线
                    vRay.BasePoint = tdLine.GetPoint3dAt(i);
                    vRay.UnitDir = new Vector3d(0, 1, 0);
                    Point3dCollection ptIntersects = new Point3dCollection();
                    tpLine.IntersectWith(vRay, Intersect.OnBothOperands, ptIntersects, IntPtr.Zero, IntPtr.Zero);
                    Point3d ptIntersect = ptIntersects[0];
                    RotatedDimension dimV = new RotatedDimension();
                    dimV.XLine1Point = tdLine.GetPoint3dAt(i);    //第一条尺寸边线
                    dimV.XLine2Point = ptIntersect;               //第二条尺寸边线
                    dimV.DimLinePoint = tdLine.GetPoint3dAt(i);   //尺寸线位置
                    dimV.Rotation = Math.PI / 2;                  //标注旋转90度
                    dimV.DimensionStyle = db.Dimstyle;            //尺寸样式为当前样式
                    dimV.Dimscale = scale;                         //设置尺寸全局比例
                    dims.Add(dimV);
                }
                //4.1 节点间距点集缺钢束最后一个点、梁顶缘线端点
                ptsH.Add(tdLine.EndPoint);
                ptsH.Add(tpLine.StartPoint);
                ptsH.Add(tpLine.EndPoint);
                db.ContinuedHorizontalDims(ptsH, pos, scale);//建立水平连续标注

                //4.5 竖直距离标注缺最后一个点
                Ray vRayLast = new Ray();//建立竖直射线
                vRayLast.BasePoint = tdLine.GetPoint3dAt(tdLine.NumberOfVertices - 1);
                vRayLast.UnitDir = new Vector3d(0, 1, 0);
                Point3dCollection ptIntersectsLast = new Point3dCollection();
                tpLine.IntersectWith(vRayLast, Intersect.OnBothOperands, ptIntersectsLast, IntPtr.Zero, IntPtr.Zero);
                Point3d ptIntersectLast = ptIntersectsLast[0];
                RotatedDimension dimVLast = new RotatedDimension();
                dimVLast.XLine1Point = tdLine.GetPoint3dAt(tdLine.NumberOfVertices - 1);    //第一条尺寸边线
                dimVLast.XLine2Point = ptIntersectLast;               //第二条尺寸边线
                dimVLast.DimLinePoint = tdLine.GetPoint3dAt(tdLine.NumberOfVertices - 1);   //尺寸线位置
                dimVLast.Rotation = Math.PI / 2;                  //标注旋转90度
                dimVLast.DimensionStyle = db.Dimstyle;            //尺寸样式为当前样式
                dimVLast.Dimscale = scale;                         //设置尺寸全局比例
                dims.Add(dimVLast);

                //4.6 绘制张拉端
                //4.6.1 绘制两侧张拉段
                //获取钢束线真实的起点和终点
                Point3d tdStart = (tdLine.StartPoint.X < tdLine.EndPoint.X) ? tdLine.StartPoint : tdLine.EndPoint;
                Point3d tdEnd = (tdLine.StartPoint.X < tdLine.EndPoint.X) ? tdLine.EndPoint : tdLine.StartPoint;
                //获取钢束线真实的起终点角度
                double iclStart = (tdLine.StartPoint.X < tdLine.EndPoint.X) ?
                    GetAngleOfLineSeg(tdLine.GetLineSegmentAt(0)) : GetAngleOfLineSeg(tdLine.GetLineSegmentAt(tdLine.NumberOfVertices - 2));
                double iclEnd = (tdLine.StartPoint.X < tdLine.EndPoint.X) ?
                    GetAngleOfLineSeg(tdLine.GetLineSegmentAt(tdLine.NumberOfVertices - 2)) : GetAngleOfLineSeg(tdLine.GetLineSegmentAt(0));
                //创建张拉端几何点
                Point3d tdDrawL = GeTools.PolarPoint(tdStart, iclStart, -800);
                Point3d tdDrawR = GeTools.PolarPoint(tdEnd, iclEnd, 800);
                //创建张拉段
                Line leftDraw = new Line(tdDrawL, tdStart);
                leftDraw.Layer = tdLine.Layer;//张拉段与钢束线应该在同一层
                Line rightDraw = new Line(tdDrawR, tdEnd);
                rightDraw.Layer = tdLine.Layer;//张拉段与钢束线应该在同一层

                //4.6.2 标注张拉段的长度
                //左侧张拉段
                MText lengthL = new MText();
                //长度
                lengthL.Contents = "工作长度800";
                //文字高度
                lengthL.TextHeight = 3 * scale;
                //样式为当前样式
                lengthL.TextStyleId = db.Textstyle;
                //旋转角度同直线段倾角
                lengthL.Rotation = iclStart;
                //对齐位置为右上
                lengthL.Attachment = AttachmentPoint.TopRight;
                //位置为中点垂线以下0.5个单位
                lengthL.Location = GeTools.PolarPoint(GeTools.MidPoint(leftDraw.StartPoint,
                    leftDraw.EndPoint), iclStart - Math.PI / 2, 0.5 * scale);

                //右侧张拉段
                MText lengthR = new MText();
                //长度
                lengthR.Contents = "工作长度800";
                //文字高度
                lengthR.TextHeight = 3 * scale;
                //样式为当前样式
                lengthR.TextStyleId = db.Textstyle;
                //旋转角度同直线段倾角
                lengthR.Rotation = iclEnd;
                //对齐位置为左上
                lengthR.Attachment = AttachmentPoint.TopLeft;
                //位置为中点垂线以下0.5个单位
                lengthR.Location = GeTools.PolarPoint(GeTools.MidPoint(rightDraw.StartPoint,
                    rightDraw.EndPoint), iclEnd - Math.PI / 2, 0.5 * scale);

                //4.7 在截面顶缘标识“梁顶缘线”
                Point3d midPt = GeTools.MidPoint(tpLine.StartPoint, tpLine.EndPoint);//顶缘线起终点中点
                Point3d midPtInTp = tpLine.GetClosestPointTo(midPt, Vector3d.YAxis, true);//顶缘线上靠近中点的点
                MText tpAnno = new MText();
                tpAnno.Contents = "梁顶缘线";
                //文字高度
                tpAnno.TextHeight = 3 * scale;
                //样式为当前样式
                tpAnno.TextStyleId = db.Textstyle;
                //对齐位置为右上
                tpAnno.Attachment = AttachmentPoint.BottomLeft;
                //位置为中点以上0.5个单位
                tpAnno.Location = GeTools.PolarPoint(midPtInTp, Math.PI / 2, 0.5 * scale);

                db.AddToModelSpace(dims.ToArray());//添加各类标注
                db.AddToModelSpace(leftDraw, rightDraw, lengthL, lengthR);//添加张拉段线
                db.AddToModelSpace(tpAnno);//添加梁顶缘线标识

                trans.Commit();
            }
        }
        /// <summary>
        /// 计算线段的倾角，表示为弧度，-pi~pi
        /// </summary>
        /// <param name="line">x线段</param>
        /// <returns>倾角</returns>
        private double GetAngleOfLineSeg(LineSegment3d line)
        {
            LineSegment3d lineDim = line;
            if (line.StartPoint.X > line.EndPoint.X) //如果StartPoint在EndPoint的后端
            {
                //则新建一个头尾调换的线段
                lineDim = new LineSegment3d(line.EndPoint, line.StartPoint);
            }
            Vector2d vec = new Vector2d(lineDim.EndPoint.X - lineDim.StartPoint.X,
                           lineDim.EndPoint.Y - lineDim.StartPoint.Y);
            double incline = vec.Angle;//获取直线的倾角
            if (incline > Math.PI)//大于180度的表示为负角度更为方便
                incline -= 2 * Math.PI;
            return incline;
        }
        /// <summary>
        /// 输出单根钢束引伸量
        /// </summary>
        [CommandMethod("DA_DrawingAmounts")]
        public void DrawingAmounts()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            int drwStyle = 0;//张拉方式，0为两端张拉，-1为左端张拉，1为右端张拉
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择钢束线
                PromptEntityOptions tdLineOpt = new PromptEntityOptions("\n选择钢束或[张拉方式(D)/管道偏差系数(K)/摩阻系数(U)/张拉控制应力(C)]");
                tdLineOpt.SetRejectMessage("\n钢束应为多段线");
                tdLineOpt.AddAllowedClass(typeof(Polyline), true);//仅能选择多段线
                tdLineOpt.Keywords.Add("D");
                tdLineOpt.Keywords.Add("K");
                tdLineOpt.Keywords.Add("U");
                tdLineOpt.Keywords.Add("C");
                tdLineOpt.AppendKeywordsToMessage = false;//提示信息中不显示关键字
                for (;;)//无限循环，直到选中钢束线为止
                {
                    PromptEntityResult tdLineRes = ed.GetEntity(tdLineOpt);
                    //2.各关键字下分别设置钢束张拉参数
                    if (tdLineRes.Status == PromptStatus.Keyword)
                    {
                        switch (tdLineRes.StringResult)
                        {
                            case "D":
                                PromptIntegerOptions drwOpt = new PromptIntegerOptions($"\n输入张拉方式[两端张拉(0)/左端张拉[-1]/右端张拉[1]<{drwStyle}>");
                                drwOpt.AllowNone = true;
                                PromptIntegerResult drwRes = ed.GetInteger(drwOpt);
                                if (drwRes.Value == 0)
                                {
                                    drwStyle = 0;
                                }
                                else if (drwRes.Value == -1)
                                {
                                    drwStyle = -1;
                                }
                                else if (drwRes.Value == 1)
                                {
                                    drwStyle = 1;
                                }
                                break;
                            case "K":
                                PromptDoubleOptions kiiOpt = new PromptDoubleOptions($"\n设置管道偏差系数(1/m)<{kii}>");
                                kiiOpt.AllowNone = true;
                                kiiOpt.AllowNegative = false;
                                kiiOpt.AllowZero = false;
                                PromptDoubleResult kiiRes = ed.GetDouble(kiiOpt);
                                if (kiiRes.Status == PromptStatus.OK)
                                {
                                    kii = kiiRes.Value;
                                }
                                break;
                            case "U":
                                PromptDoubleOptions miuOpt = new PromptDoubleOptions($"\n设置摩阻系数<{miu}>");
                                miuOpt.AllowNone = true;
                                miuOpt.AllowNegative = false;
                                miuOpt.AllowZero = false;
                                PromptDoubleResult miuRes = ed.GetDouble(miuOpt);
                                if (miuRes.Status == PromptStatus.OK)
                                {
                                    miu = miuRes.Value;
                                }
                                break;
                            case "C":
                                PromptDoubleOptions ctrOpt = new PromptDoubleOptions($"\n设置张拉控制应力(MPa)<{ctrlStress}>");
                                ctrOpt.AllowNone = true;
                                PromptDoubleResult ctrRes = ed.GetDouble(ctrOpt);
                                if (ctrRes.Status == PromptStatus.OK)
                                {
                                    ctrlStress = ctrRes.Value;
                                }
                                break;
                        }

                    }
                    //3.输出引伸量
                    else if (tdLineRes.Status == PromptStatus.OK)
                    {
                        ObjectId tdLineId = tdLineRes.ObjectId;
                        Polyline tdLine = trans.GetObject(tdLineId, OpenMode.ForRead) as Polyline;
                        if (drwStyle == 0)//两端张拉
                        {
                            double[] drawAmounts = BothDrawAmount(tdLine, ctrlStress, kii, miu, Ep);
                            ed.WriteMessage("\n左侧引伸量：" + drawAmounts[0].ToString("F0") + "; " +
                                "右侧引伸量：" + drawAmounts[1].ToString("F0") + "。");
                        }
                        else if (drwStyle == -1)//左侧张拉
                        {
                            double drawAmount = SingleDrawAmount(tdLine, ctrlStress, kii, miu, -1, Ep);
                            ed.WriteMessage("\n左侧引伸量：" + drawAmount.ToString("F0") + "。");
                        }
                        else if (drwStyle == 1)//右侧张拉
                        {
                            double drawAmount = SingleDrawAmount(tdLine, ctrlStress, kii, miu, 1, Ep);
                            ed.WriteMessage("\n右侧引伸量：" + drawAmount.ToString("F0") + "。");
                        }
                        break;
                    }
                    else
                    {
                        ed.WriteMessage("输入有误！");
                        return;
                    }
                }
                trans.Commit();//执行事务处理
            }
        }
        /// <summary>
        /// 计算单端张拉引伸量
        /// 摩阻损失按简化公式计算
        /// 钢束默认按照mm绘制
        /// </summary>
        /// <param name="tdLine">钢束多段线</param>
        /// <param name="ctrlStress">张拉控制应力(MPa)</param>
        /// <param name="kii">管道偏差系数(1/m)</param>
        /// <param name="miu">摩阻系数</param>
        /// <param name="drawEnd">张拉端，-1为左端（默认值），1为右端</param>
        /// <param name="E">钢束弹性模量(MPa)</param>
        /// <returns>引伸量（mm）</returns>
        internal static double SingleDrawAmount(Polyline tdLine, double ctrlStress,
            double kii, double miu, int drawEnd, double E)
        {
            double[] sigLosts = new double[tdLine.NumberOfVertices];//初始化各节点应力损失
            double[] sigAvgs = new double[tdLine.NumberOfVertices - 1];//初始化各节段平均应力
            double[] segLengths = new double[tdLine.NumberOfVertices - 1];//初始化各节段长度
            double[] acumLens = new double[tdLine.NumberOfVertices];//初始化各节点处钢束累计长度
            double[] acumAngs = new double[tdLine.NumberOfVertices];//初始化各节点处钢束累计转角
            List<Curve3d> tdSegs = new List<Curve3d>();//存放各节段曲线
            for (int i = 0; i < tdLine.NumberOfVertices - 1; i++)//获取各节段
            {
                if (tdLine.GetSegmentType(i) == SegmentType.Line)
                    tdSegs.Add(tdLine.GetLineSegmentAt(i));
                else if (tdLine.GetSegmentType(i) == SegmentType.Arc)
                    tdSegs.Add(tdLine.GetArcSegmentAt(i));
            }
            //将各节段按x坐标递增排序
            var sortedTdSegs = (from seg in tdSegs
                                orderby seg.StartPoint.X
                                select seg).ToList();
            if (drawEnd == 1)//如果右侧张拉，则按x坐标递减排序
            {
                sortedTdSegs = (from seg in tdSegs
                                orderby seg.StartPoint.X descending
                                select seg).ToList();
            }
            for (int i = 0; i < sortedTdSegs.Count; i++)
            {
                if (sortedTdSegs[i] is LineSegment3d)
                {
                    LineSegment3d lineSeg = sortedTdSegs[i] as LineSegment3d;
                    segLengths[i] = lineSeg.Length;//节段长度
                    acumLens[i + 1] = acumLens[i] + segLengths[i];//i节点处钢束累计长度
                    acumAngs[i + 1] = acumAngs[i];//i节点处钢束累计转角，直线段不增加
                    sigLosts[i + 1] = ctrlStress*(1-1/Math.Exp(kii* acumLens[i + 1]/1000));//节点预应力损失
                    sigAvgs[i] = (ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * segLengths[i]/1000))
                        / (kii * segLengths[i]/1000);//节段平均有效应力
                }
                else if (sortedTdSegs[i] is CircularArc3d)
                {
                    CircularArc3d arcSeg = sortedTdSegs[i] as CircularArc3d;
                    segLengths[i] = Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle) * arcSeg.Radius;
                    acumLens[i + 1] = acumLens[i] + segLengths[i];//i节点处钢束累计长度
                    acumAngs[i + 1] = acumAngs[i] + Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle);//i节点处钢束累计转角
                    sigLosts[i + 1] = ctrlStress * (1 - 1 / Math.Exp(kii * acumLens[i + 1]/1000 + miu * acumAngs[i + 1]));//节点预应力损失
                    sigAvgs[i] = (ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * segLengths[i]/1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)))
                        / (kii * segLengths[i]/1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle));//节段平均有效应力
                }
            }
            double drawAmount = 0;
            for (int i = 0; i < sortedTdSegs.Count; i++)
            {
                drawAmount += sigAvgs[i] / E * segLengths[i];
            }
            return drawAmount;
        }
        /// <summary>
        /// 计算两端张拉引伸量
        /// 摩阻损失按简化公式计算
        /// 钢束线默认按照mm绘制
        /// </summary>
        /// <param name="tdLine">钢束线</param>
        /// <param name="ctrlStress">张拉控制应力(MPa)</param>
        /// <param name="kii">管道偏差系数(1/m)</param>
        /// <param name="miu">摩阻系数</param>
        /// <param name="E">钢束模量(MPa)</param>
        /// <returns>两侧引伸量(mm)</returns>
        internal static double[] BothDrawAmount(Polyline tdLine, double ctrlStress,
           double kii, double miu, double E)
        {
            double[] drawAmounts = new double[2];//存放两侧引伸量数据
            //1.获取各节段曲线并排序
            List<Curve3d> tdSegs = new List<Curve3d>();//存放各节段曲线
            for (int i = 0; i < tdLine.NumberOfVertices - 1; i++)//获取各节段
            {
                if (tdLine.GetSegmentType(i) == SegmentType.Line)
                    tdSegs.Add(tdLine.GetLineSegmentAt(i));
                else if (tdLine.GetSegmentType(i) == SegmentType.Arc)
                    tdSegs.Add(tdLine.GetArcSegmentAt(i));
            }
            var sortedTdSegs = (from seg in tdSegs
                                orderby seg.StartPoint.X
                                select seg).ToList();
            //2.计算总损失
            double totalLen = 0;//初始化钢束累计长度
            double totalAng = 0;//初始化钢束累计转角
            for (int i = 0; i < sortedTdSegs.Count; i++)
            {
                if (sortedTdSegs[i] is LineSegment3d)
                {
                    LineSegment3d lineSeg = sortedTdSegs[i] as LineSegment3d;
                    double segLength = lineSeg.Length;//节段长度
                    totalLen += segLength;//累计钢束长度
                }
                else if (sortedTdSegs[i] is CircularArc3d)
                {
                    CircularArc3d arcSeg = sortedTdSegs[i] as CircularArc3d;
                    double segLength = Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle) * arcSeg.Radius;
                    totalAng += Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle);//累计钢束转角
                }
            }
            double totalLost = ctrlStress * (1 - 1 / Math.Exp(kii * totalLen / 1000 + miu * totalAng));
            //3.左侧引伸量计算
            List<double> sigLosts = new List<double>();//初始化左侧节点预应力损失向量
            List<double> sigAvgs = new List<double>();//初始化左侧节段平均预应力损失向量
            List<double> segLengths = new List<double>();//初始化左侧节段长度
            List<double> acumLens = new List<double>();//初始化左侧起各节点处钢束累计长度
            List<double> acumAngs = new List<double>();//初始化左侧起各节点处钢束累计转角
            sigLosts.Add(0);//端点损失为0
            acumLens.Add(0);//端点累计长度为0
            acumAngs.Add(0);//端点累计转角为0
            for (int i = 0; i < sortedTdSegs.Count; i++)
            {
                if (sortedTdSegs[i] is LineSegment3d)
                {
                    LineSegment3d lineSeg = sortedTdSegs[i] as LineSegment3d;
                    segLengths.Add(lineSeg.Length);//节段长度
                    acumLens.Add(acumLens[i] + segLengths[i]);//i节点处钢束累计长度
                    acumAngs.Add(acumAngs[i]);//i节点处钢束累计转角，直线段不增加
                    sigLosts.Add(ctrlStress * (1 - 1 / Math.Exp(kii * acumLens[i + 1] / 1000)));//节点预应力损失
                    sigAvgs.Add((ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * segLengths[i] / 1000))
                        / (kii * segLengths[i] / 1000));//节段平均有效应力
                }
                else if (sortedTdSegs[i] is CircularArc3d)
                {
                    CircularArc3d arcSeg = sortedTdSegs[i] as CircularArc3d;
                    segLengths.Add(Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle) * arcSeg.Radius);
                    acumLens.Add(acumLens[i] + segLengths[i]);//i节点处钢束累计长度
                    acumAngs.Add(acumAngs[i] + Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle));//i节点处钢束累计转角
                    sigLosts.Add(ctrlStress * (1 - 1 / Math.Exp(kii * acumLens[i + 1] / 1000 + miu * acumAngs[i + 1])));//节点预应力损失
                    sigAvgs.Add((ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * segLengths[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)))
                        / (kii * segLengths[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)));//节段平均有效应力
                }
                if(sigLosts[i + 1] > totalLost/2)//到达平衡点，分别修改平均应力和节段长度至平衡点位置，并退出循环
                {
                    double iita = (totalLost / 2 - sigLosts[i]) / (sigLosts[i + 1] - sigLosts[i]);//该节段内平衡点与i节点间的长度占节段长度的比例，按照线性近似
                    //修改最后一个节段的平均应力
                    if (sortedTdSegs[i] is LineSegment3d)
                    {
                        sigAvgs[i] = (ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * iita * segLengths[i] / 1000))
                        / (kii * iita * segLengths[i] / 1000);//节段平均有效应力
                    }    
                    else if(sortedTdSegs[i] is CircularArc3d)
                    {
                        CircularArc3d arcSeg = sortedTdSegs[i] as CircularArc3d;
                        sigAvgs[i] = (ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * iita* segLengths[i] / 1000 + miu * iita* Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)))
                        / (kii * iita* segLengths[i] / 1000 + miu * iita* Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle));//节段平均有效应力
                    }
                    segLengths[i] = iita * segLengths[i];//修改最后一个节段的长度为平衡点与i节点间的长度
                    break;
                }
            }
            for(int i = 0; i < sigAvgs.Count; i++)
            {
                drawAmounts[0] += sigAvgs[i] / E * segLengths[i];//计算左侧引伸量
            }
            //4.右侧引伸量计算
            //线段降序排列
            sortedTdSegs = (from seg in tdSegs
                            orderby seg.StartPoint.X descending
                            select seg).ToList();
            sigLosts = new List<double>();//初始化右侧节点预应力损失向量
            sigAvgs = new List<double>();//初始化右侧节段平均预应力损失向量
            segLengths = new List<double>();//初始化右侧节段长度
            acumLens = new List<double>();//初始化右侧起各节点处钢束累计长度
            acumAngs = new List<double>();//初始化右侧起各节点处钢束累计转角
            sigLosts.Add(0);//端点损失为0
            acumLens.Add(0);//端点累计长度为0
            acumAngs.Add(0);//端点累计转角为0
            for (int i = 0; i < sortedTdSegs.Count; i++)
            {
                if (sortedTdSegs[i] is LineSegment3d)
                {
                    LineSegment3d lineSeg = sortedTdSegs[i] as LineSegment3d;
                    segLengths.Add(lineSeg.Length);//节段长度
                    acumLens.Add(acumLens[i] + segLengths[i]);//i节点处钢束累计长度
                    acumAngs.Add(acumAngs[i]);//i节点处钢束累计转角，直线段不增加
                    sigLosts.Add(ctrlStress * (1 - 1 / Math.Exp(kii * acumLens[i + 1] / 1000)));//节点预应力损失
                    sigAvgs.Add((ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * segLengths[i] / 1000))
                        / (kii * segLengths[i] / 1000));//节段平均有效应力
                }
                else if (sortedTdSegs[i] is CircularArc3d)
                {
                    CircularArc3d arcSeg = sortedTdSegs[i] as CircularArc3d;
                    segLengths.Add(Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle) * arcSeg.Radius);
                    acumLens.Add(acumLens[i] + segLengths[i]);//i节点处钢束累计长度
                    acumAngs.Add(acumAngs[i] + Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle));//i节点处钢束累计转角
                    sigLosts.Add(ctrlStress * (1 - 1 / Math.Exp(kii * acumLens[i + 1] / 1000 + miu * acumAngs[i + 1])));//节点预应力损失
                    sigAvgs.Add((ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * segLengths[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)))
                        / (kii * segLengths[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)));//节段平均有效应力
                }
                if (sigLosts[i + 1] > totalLost / 2)//到达平衡点，分别修改平均应力和节段长度至平衡点位置，并退出循环
                {
                    double iita = (totalLost / 2 - sigLosts[i]) / (sigLosts[i + 1] - sigLosts[i]);//该节段内平衡点与i节点间的长度占节段长度的比例，按照线性近似
                    //修改最后一个节段的平均应力
                    if (sortedTdSegs[i] is LineSegment3d)
                    {
                        sigAvgs[i] = (ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * iita * segLengths[i] / 1000))
                        / (kii * iita * segLengths[i] / 1000);//节段平均有效应力
                    }
                    else if (sortedTdSegs[i] is CircularArc3d)
                    {
                        CircularArc3d arcSeg = sortedTdSegs[i] as CircularArc3d;
                        sigAvgs[i] = (ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * iita * segLengths[i] / 1000 + miu * iita * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)))
                        / (kii * iita * segLengths[i] / 1000 + miu * iita * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle));//节段平均有效应力
                    }
                    segLengths[i] = iita * segLengths[i];//修改最后一个节段的长度为平衡点与i节点间的长度
                    break;
                }
            }
            for (int i = 0; i < sigAvgs.Count; i++)
            {
                drawAmounts[1] += sigAvgs[i] / E * segLengths[i];//计算右侧引伸量
            }
            return drawAmounts;
        }
        /// <summary>
        /// 操作和输出钢束表
        /// </summary>
        [CommandMethod("DA_TendonTable")]
        public void TendonTable()
        {
            //创建钢束信息界面
            TendonInfo tdInfo = new TendonInfo();            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.操作有名对象字典，获取钢束总体信息 
                // 获取当前数据库的有名对象字典
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
                if (dicts.Contains("DA_Tendons"))//调试用
                    dicts.Remove("DA_Tendons");//调试用
                if (!dicts.Contains("DA_Tendons"))//如果字典中不含DA_Tendons的字典项
                {
                    ObjectId tdsDictNewId = db.AddNamedDictionary("DA_Tendons");//则添加该字典项
                    //管道偏差系数
                    TypedValueList values = new TypedValueList();
                    values.Add(DxfCode.Real, kii);
                    tdsDictNewId.AddXrecord2DBDict("kii", values);
                    //摩阻系数
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, miu);
                    tdsDictNewId.AddXrecord2DBDict("miu", values);
                    //钢束弹性模量
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, Ep);
                    tdsDictNewId.AddXrecord2DBDict("Ep", values);
                    //张拉控制应力
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, ctrlStress);
                    tdsDictNewId.AddXrecord2DBDict("ctrlStress", values);
                    //张拉端工作长度
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, workLen);
                    tdsDictNewId.AddXrecord2DBDict("workLen", values);                    
                }
                //如果已有名为DA_Tendons的字典项，则将其中数据读入界面中
                dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;//获取当前数据库有名对象字典
                ObjectId tdsDictId = dicts.GetAt("DA_Tendons");
                DBDictionary tdsDict = tdsDictId.GetObject(OpenMode.ForWrite) as DBDictionary; //获取DA_Tendons字典
                ed.WriteMessage(tdsDict.Count.ToString());//调试用
                //管道偏差系数
                ObjectId xrecId = tdsDict.GetAt("kii");
                Xrecord xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                TypedValueList vls = xrec.Data;
                tdInfo.textBoxKii.Text = vls[0].Value.ToString();
                //摩阻系数
                xrecId = tdsDict.GetAt("miu");
                xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                vls = xrec.Data;
                tdInfo.textBoxMiu.Text = vls[0].Value.ToString();
                //钢束弹性模量
                xrecId = tdsDict.GetAt("Ep");
                xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                vls = xrec.Data;
                tdInfo.textBoxEp.Text = vls[0].Value.ToString();
                //张拉控制应力
                xrecId = tdsDict.GetAt("ctrlStress");
                xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                vls = xrec.Data;
                tdInfo.textBoxCtrlStress.Text = vls[0].Value.ToString();
                //张拉端工作长度
                xrecId = tdsDict.GetAt("workLen");
                xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                vls = xrec.Data;
                tdInfo.textBoxWorkLen.Text = vls[0].Value.ToString();
                trans.Commit();//执行事务处理
            }
            //显示钢束信息界面
            Application.ShowModalDialog(tdInfo);       
        }
    }
}
