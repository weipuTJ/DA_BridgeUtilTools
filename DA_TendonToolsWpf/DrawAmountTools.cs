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

namespace DA_TendonToolsWpf
{
    public static class DrawAmountTools
    {
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
        /// <param name="Ep">钢束弹性模量(MPa)</param>
        /// <returns>引伸量（mm）</returns>
        internal static double SingleDrawAmount(this Polyline tdLine, double ctrlStress,
            double kii, double miu, int drawEnd, double Ep)
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
                    sigLosts[i + 1] = ctrlStress * (1 - 1 / Math.Exp(kii * acumLens[i + 1] / 1000));//节点预应力损失
                    sigAvgs[i] = (ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * segLengths[i] / 1000))
                        / (kii * segLengths[i] / 1000);//节段平均有效应力
                }
                else if (sortedTdSegs[i] is CircularArc3d)
                {
                    CircularArc3d arcSeg = sortedTdSegs[i] as CircularArc3d;
                    segLengths[i] = Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle) * arcSeg.Radius;
                    acumLens[i + 1] = acumLens[i] + segLengths[i];//i节点处钢束累计长度
                    acumAngs[i + 1] = acumAngs[i] + Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle);//i节点处钢束累计转角
                    sigLosts[i + 1] = ctrlStress * (1 - 1 / Math.Exp(kii * acumLens[i + 1] / 1000 + miu * acumAngs[i + 1]));//节点预应力损失
                    sigAvgs[i] = (ctrlStress - sigLosts[i])
                        * (1 - 1 / Math.Exp(kii * segLengths[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)))
                        / (kii * segLengths[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle));//节段平均有效应力
                }
            }
            double drawAmount = 0;
            for (int i = 0; i < sortedTdSegs.Count; i++)
            {
                drawAmount += sigAvgs[i] / Ep * segLengths[i];
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
        /// <param name="Ep">钢束模量(MPa)</param>
        /// <returns>两侧引伸量(mm)</returns>
        internal static double[] BothDrawAmount(this Polyline tdLine, double ctrlStress,
           double kii, double miu, double Ep)
        {
            double[] drawAmounts = new double[2];//存放两侧引伸量数据
            //1.左侧数据记录
            //获取各节段曲线并排序
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
            
            List<double> sigLostsL = new List<double>();//初始化左侧节点预应力损失向量
            List<double> sigAvgsL = new List<double>();//初始化左侧节段平均预应力损失向量
            List<double> segLengthsL = new List<double>();//初始化左侧节段长度
            List<double> acumLensL = new List<double>();//初始化左侧起各节点处钢束累计长度
            List<double> acumAngsL = new List<double>();//初始化左侧起各节点处钢束累计转角
            sigLostsL.Add(0);//端点损失为0
            acumLensL.Add(0);//端点累计长度为0
            acumAngsL.Add(0);//端点累计转角为0
            for (int i = 0; i < sortedTdSegs.Count; i++)
            {
                if (sortedTdSegs[i] is LineSegment3d)
                {
                    LineSegment3d lineSeg = sortedTdSegs[i] as LineSegment3d;
                    segLengthsL.Add(lineSeg.Length);//节段长度
                    acumLensL.Add(acumLensL[i] + segLengthsL[i]);//i节点处钢束累计长度
                    acumAngsL.Add(acumAngsL[i]);//i节点处钢束累计转角，直线段不增加
                    sigLostsL.Add(ctrlStress * (1 - 1 / Math.Exp(kii * acumLensL[i + 1] / 1000 + miu * acumAngsL[i + 1])));//节点预应力损失
                    sigAvgsL.Add((ctrlStress - sigLostsL[i])
                        * (1 - 1 / Math.Exp(kii * segLengthsL[i] / 1000))
                        / (kii * segLengthsL[i] / 1000));//节段平均有效应力
                }
                else if (sortedTdSegs[i] is CircularArc3d)
                {
                    CircularArc3d arcSeg = sortedTdSegs[i] as CircularArc3d;
                    segLengthsL.Add(Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle) * arcSeg.Radius);
                    acumLensL.Add(acumLensL[i] + segLengthsL[i]);//i节点处钢束累计长度
                    acumAngsL.Add(acumAngsL[i] + Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle));//i节点处钢束累计转角
                    sigLostsL.Add(ctrlStress * (1 - 1 / Math.Exp(kii * acumLensL[i + 1] / 1000 + miu * acumAngsL[i + 1])));//节点预应力损失
                    sigAvgsL.Add((ctrlStress - sigLostsL[i])
                        * (1 - 1 / Math.Exp(kii * segLengthsL[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)))
                        / (kii * segLengthsL[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)));//节段平均有效应力
                }                
            }
            //2.右侧数据记录
            //线段降序排列
            sortedTdSegs = (from seg in tdSegs
                            orderby seg.StartPoint.X descending
                            select seg).ToList();
            List<double> sigLostsR = new List<double>();//初始化右侧节点预应力损失向量
            List<double> sigAvgsR = new List<double>();//初始化右侧节段平均预应力损失向量
            List<double> segLengthsR = new List<double>();//初始化右侧节段长度
            List<double> acumLensR = new List<double>();//初始化右侧起各节点处钢束累计长度
            List<double> acumAngsR = new List<double>();//初始化右侧起各节点处钢束累计转角
            sigLostsR.Add(0);//端点损失为0
            acumLensR.Add(0);//端点累计长度为0
            acumAngsR.Add(0);//端点累计转角为0
            for (int i = 0; i < sortedTdSegs.Count; i++)
            {
                if (sortedTdSegs[i] is LineSegment3d)
                {
                    LineSegment3d lineSeg = sortedTdSegs[i] as LineSegment3d;
                    segLengthsR.Add(lineSeg.Length);//节段长度
                    acumLensR.Add(acumLensR[i] + segLengthsR[i]);//i节点处钢束累计长度
                    acumAngsR.Add(acumAngsR[i]);//i节点处钢束累计转角，直线段不增加
                    sigLostsR.Add(ctrlStress * (1 - 1 / Math.Exp(kii * acumLensR[i + 1] / 1000 + miu * acumAngsR[i + 1])));//节点预应力损失
                    sigAvgsR.Add((ctrlStress - sigLostsR[i])
                        * (1 - 1 / Math.Exp(kii * segLengthsR[i] / 1000))
                        / (kii * segLengthsR[i] / 1000));//节段平均有效应力
                }
                else if (sortedTdSegs[i] is CircularArc3d)
                {
                    CircularArc3d arcSeg = sortedTdSegs[i] as CircularArc3d;
                    segLengthsR.Add(Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle) * arcSeg.Radius);
                    acumLensR.Add(acumLensR[i] + segLengthsR[i]);//i节点处钢束累计长度
                    acumAngsR.Add(acumAngsR[i] + Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle));//i节点处钢束累计转角
                    sigLostsR.Add(ctrlStress * (1 - 1 / Math.Exp(kii * acumLensR[i + 1] / 1000 + miu * acumAngsR[i + 1])));//节点预应力损失
                    sigAvgsR.Add((ctrlStress - sigLostsR[i])
                        * (1 - 1 / Math.Exp(kii * segLengthsR[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)))
                        / (kii * segLengthsR[i] / 1000 + miu * Math.Abs(arcSeg.EndAngle - arcSeg.StartAngle)));//节段平均有效应力
                }
            }
            //3.计算平衡点所在段及在该段的比例     
            int blcIndex = 0;//初始化平衡点所在段索引（从左起）
            double iita = 0;//平衡点在该段的比例（从左起）       
            for (int i = 1; i < sigLostsL.Count-1; i++)//平衡点不可能在端点，故循环起终点为1和sigLostsL.Count-2
            {
                double sigELJ = ctrlStress - sigLostsL[i];//该节点从左侧起算的有效应力
                double sigERJ = ctrlStress - sigLostsR[sigLostsL.Count - i - 1];//该节点从右侧起算的有效应力
                double sigELK = ctrlStress - sigLostsL[i + 1];//下一节点从左侧起算的有效应力
                double sigERK = ctrlStress - sigLostsR[sigLostsL.Count - i - 2];//下一节点从右侧起算的有效应力
                if (sigELJ == sigERJ)
                {
                    blcIndex = i - 1;//平衡段序号为i - 1
                    iita = 1;//平衡点在该段比例为1，即全长
                    break;
                }
                else if ((sigELJ - sigERJ) * (sigELK - sigERK) < 0)//该节段左右侧起算的有效应力大小关系转变
                {
                    blcIndex = i;
                    double xM = segLengthsL[i];
                    double sitaM = acumAngsL[i + 1] - acumAngsL[i];
                    iita = (Math.Log(sigELJ / sigERK) / (kii * xM + miu * sitaM) + 1) / 2;
                    break;
                }
            }
            //4.分别求两侧伸长量
            //4.1 左侧
            for (int i = 0; i <= blcIndex; i++)
            {
                drawAmounts[0] += sigAvgsL[i] / Ep * 
                    ((i == blcIndex)? iita : 1) * segLengthsL[i];//计算左侧引伸量
            }
            //4.2 右侧
            for(int i = 0; i <= sigAvgsR.Count - blcIndex - 1; i++)
            {
                drawAmounts[1] += sigAvgsR[i] / Ep *
                    ((i == sigAvgsR.Count - blcIndex - 1) ? (1-iita) : 1) * segLengthsR[i];//计算右侧引伸量
            }
            //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"平衡点在左侧{blcIndex + 1}段{iita}位置");
            return drawAmounts;
        }       
    }
}
