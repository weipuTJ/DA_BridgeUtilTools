using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using DotNetARX;


namespace DA_Excel2CadTools
{
    /// <summary>
    /// 单元格在合并区域中的位置
    /// </summary>
    public enum PosInMergeRange
    {
        NotMergeRange,
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom,
        Middle,
        EdgeMiddle
    }
    public static class ExcelTools
    {
        /// <summary>
        /// 将Excel区域内容置于二维字符串数组中
        /// </summary>
        /// <param name="rng">Excel区域</param>
        /// <returns>存储由Excel内容的二维字符串数组</returns>
        public static string[,] GetExcelContent(this Excel.Range rng)
        {
            string[,] excelContent = new string[rng.Rows.Count, rng.Columns.Count];//初始化内容数组
            for(int i = 1; i <= rng.Rows.Count; i++)
            {
                for(int j = 1; j <= rng.Columns.Count; j++)
                {
                    excelContent[i - 1, j - 1] = rng[i, j].Text as string;
                }
            }
            return excelContent;
        }
        /// <summary>
        /// 获取Excel区域在CAD中的行高，选择行自动时按字高乘以1.5确定，否则按行高设置值
        /// </summary>
        /// <param name="rng">Excel区域</param>
        /// <param name="isDefault">s是否返回默认值，为true是不管是否选择行自动均返回默认值</param>
        /// <returns>Excel区域在CAD中的行高数组</returns>
        public static double[] GetTableHeights(this Excel.Range rng, bool isDefault = false)
        {
            double[] tableHeights = new double[rng.Rows.Count];//初始化行高数组
            if(isDefault || Excel2CADSettings.e2cOptions.RowAuto)//如果选择行自动
            {
                for (int i = 0; i < rng.Rows.Count; i++)
                {
                    tableHeights[i] = Excel2CADSettings.e2cOptions.TextHeight 
                        * Excel2CADSettings.e2cOptions.Scale * 1.5;//行高度为3单位字高乘以1.5放大系数
                }
            }
            else//如果不采用行自动
            {
                tableHeights[0] = Excel2CADSettings.e2cOptions.HeaderRowHeight * Excel2CADSettings.e2cOptions.Scale;//表头高度按设置值
                for (int i = 1; i < rng.Rows.Count; i++)
                {
                    tableHeights[i] = Excel2CADSettings.e2cOptions.ContentRowHeight * Excel2CADSettings.e2cOptions.Scale;//内容行高度按设置值
                }
            }
            return tableHeights;
        }
        /// <summary>
        /// 获取Excel区域在CAD中的列宽，选择列自动时按本列文字最大宽度乘以1.2确定，否则按列宽设置值
        /// </summary>
        /// <param name="rng">Excel区域</param>
        /// <param name="isDefault">是否返回默认值，为true是不管是否选择列自动均返回默认值</param>
        /// <returns>>Excel区域在CAD中的列宽数组</returns>
        public static double[] GetTableWidths(this Excel.Range rng, bool isDefault = false)
        {
            double[] tableWidths = new double[rng.Columns.Count];
            string[,] excelContent = rng.GetExcelContent();
            if (isDefault || Excel2CADSettings.e2cOptions.ColumnAuto)//如果选择列自动
            {
                for(int j = 0; j < rng.Columns.Count; j++)
                {
                    //获得该列内容中最多的字节数
                    int maxChars = (from txt in rng.GetExcelContent().GetColumnAt(j)
                                    let nChars = txt.Count()
                                    select nChars).Max();
                    tableWidths[j] = maxChars * Excel2CADSettings.e2cOptions.TextHeight * Excel2CADSettings.e2cOptions.Scale
                        * Excel2CADSettings.e2cOptions.TextWidthFactor * 1.5;//默认宽度为该列最大文字宽度乘以1.5
                    
                }
            }
            else//不选择列自动
            {
                for (int j = 0; j < rng.Columns.Count; j++)
                {
                    tableWidths[j] = Excel2CADSettings.e2cOptions.ColOptList[j].ColWidth * Excel2CADSettings.e2cOptions.Scale;
                }
            }
            return tableWidths;
        }
        /// <summary>
        /// 绘制文字和边框线
        /// 该函数为使用事务处理，外部调用时应放置在Transaction内
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="rng">用户选择的Excel表格区域</param>
        /// <returns>所有文字和直线的ObjectIdCollection</returns>
        public static ObjectIdCollection DrawTextsAndLines(this Database db, Excel.Range rng)
        {
            //初始化返回的ObjectIdCollection
            ObjectIdCollection entIds = new ObjectIdCollection();
            //获取表格行高
            double[] tableHeights = rng.GetTableHeights();
            double tableH = tableHeights.Sum();//总高
            //获取表格列宽
            double[] tableWidths = rng.GetTableWidths();
            double tableW = tableWidths.Sum();//总宽
            //表格行列数
            int nRow = rng.Rows.Count;
            int nCol = rng.Columns.Count;
            #region 1.绘制文字
            for (int j = 0; j < nCol; j++)//列循环
            {
                for(int i = 0; i < nRow; i++)//行循环
                {
                    DBText txt = new DBText();
                    txt.TextString = rng[i + 1, j + 1].Text as string;//内容                
                    txt.Height = Excel2CADSettings.e2cOptions.TextHeight
                            * Excel2CADSettings.e2cOptions.Scale;//文字高度               
                    txt.TextStyleId = db.Textstyle;//样式为当前样式                
                    txt.WidthFactor = Excel2CADSettings.e2cOptions.TextWidthFactor;//宽度系数
                    double txtX = 0;//初始化插入点横坐标
                    double txtY = 0;//初始化插入点纵坐标
                    //水平对齐位置
                    if (Excel2CADSettings.e2cOptions.ColumnAuto)//如果选择列自动，按中对齐
                    {
                        txt.HorizontalMode = TextHorizontalMode.TextMid;
                        //插入点X为单元格中心
                        if (rng[i + 1, j + 1].PositionInMergeRange() == PosInMergeRange.LeftTop)
                            txtX = tableWidths.GetSumAt(j - 1) + tableWidths.GetSumAt(j, rng[i + 1, j + 1].MergeArea.Columns.Count) / 2;
                        else
                            txtX = tableWidths.GetSumAt(j - 1) + tableWidths[j] / 2;
                    }
                    else//如果不选择列自动
                    {
                        switch (Excel2CADSettings.e2cOptions.ColOptList[j].HrAlignment)
                        {
                            case 0:
                                txt.HorizontalMode = TextHorizontalMode.TextLeft;
                                //插入点X为单元格左边线向右0.2单位
                                txtX = tableWidths.GetSumAt(j - 1) + 0.2 * Excel2CADSettings.e2cOptions.Scale;
                                break;
                            case 1:
                                txt.HorizontalMode = TextHorizontalMode.TextMid;
                                //插入点X为单元格中心
                                if (rng[i + 1, j + 1].PositionInMergeRange() == PosInMergeRange.LeftTop)
                                    txtX = tableWidths.GetSumAt(j - 1) + tableWidths.GetSumAt(j, rng[i + 1, j + 1].MergeArea.Columns.Count) / 2;
                                else
                                    txtX = tableWidths.GetSumAt(j - 1) + tableWidths[j] / 2;
                                break;
                            case 2:
                                txt.HorizontalMode = TextHorizontalMode.TextRight;
                                //插入点X为单元格右边线向左0.2单位
                                txtX = tableWidths.GetSumAt(j) - 0.2 * Excel2CADSettings.e2cOptions.Scale;
                                break;
                        }
                    }
                    //竖直对齐位置
                    if (Excel2CADSettings.e2cOptions.ColumnAuto)//如果选择列自动，按中对齐
                    {
                        txt.VerticalMode = TextVerticalMode.TextVerticalMid;
                        //插入点Y为单元格中心
                        if (rng[i + 1, j + 1].PositionInMergeRange() == PosInMergeRange.LeftTop)
                            txtY = -(tableHeights.GetSumAt(i - 1) + tableHeights.GetSumAt(i, rng[i + 1, j + 1].MergeArea.Rows.Count) / 2);
                        else
                            txtY = -(tableHeights.GetSumAt(i - 1) + tableHeights[i] / 2);
                    }
                    else
                    {
                        switch (Excel2CADSettings.e2cOptions.ColOptList[j].VtAlignment)
                        {
                            case 0:
                                txt.VerticalMode = TextVerticalMode.TextTop;
                                //插入点Y为单元格上边线向下0.2单位
                                txtY = -tableHeights.GetSumAt(i - 1) - 0.2 * Excel2CADSettings.e2cOptions.Scale;
                                break;
                            case 1:
                                txt.VerticalMode = TextVerticalMode.TextVerticalMid;
                                //插入点Y为单元格中心
                                if (rng[i + 1, j + 1].PositionInMergeRange() == PosInMergeRange.LeftTop)
                                    txtY = -(tableHeights.GetSumAt(i - 1) + tableHeights.GetSumAt(i, rng[i + 1, j + 1].MergeArea.Rows.Count) / 2);
                                else
                                    txtY = -(tableHeights.GetSumAt(i - 1) + tableHeights[i] / 2);
                                break;
                            case 2:
                                txt.VerticalMode = TextVerticalMode.TextBottom;//仅测试后，发现CAD该枚举内部应该有问题
                                //插入点Y为单元格下边线向上0.2单位
                                txtY = -tableHeights.GetSumAt(i) + 0.2 * Excel2CADSettings.e2cOptions.Scale;
                                break;
                        }
                    }
                    txt.AlignmentPoint = new Point3d(txtX, txtY, 0);
                    db.AddToModelSpace(txt);
                    entIds.Add(txt.ObjectId);
                }
            }
            #endregion
            #region 2.绘制边框线
            //2.1 绘制外边框
            //2.1.1 上边线
            double stP = 0;//起始位置
            double edP = tableW;//结束位置
            bool flag = false;//是否有无边线格
            for(int j = 0; j < nCol; j++)//遍历首行各单元格，注意COM编程时下标从1开始
            {
                //如果存在无边框单元格
                if (rng[1,j+1].Borders[XlBordersIndex.xlEdgeTop].LineStyle.Equals(XlLineStyle.xlLineStyleNone))
                {
                    edP = tableWidths.GetSumAt(j-1);//末端坐标移到无边框格起点
                    if(edP != stP) //如果起终点不重合则绘制框线
                    {
                        Line line = new Line(new Point3d(stP, 0, 0), new Point3d(edP, 0, 0));
                        line.Color = Excel2CADSettings.e2cOptions.OuterLineColor;
                        db.AddToModelSpace(line);
                        entIds.Add(line.ObjectId);
                    }
                    stP = tableWidths.GetSumAt(j);
                    edP = tableW;
                    flag = true;
                    continue;
                }
            }
            if(flag == false)//如果没有无边框格
            {
                Line line = new Line(new Point3d(0, 0, 0), new Point3d(tableW, 0, 0));
                line.Color = Excel2CADSettings.e2cOptions.OuterLineColor;
                db.AddToModelSpace(line);
                entIds.Add(line.ObjectId);
            }
            //2.1.2 下边线
            stP = 0;//起始位置
            edP = tableW;//结束位置
            flag = false;//是否有无边线格
            for (int j = 0; j < nCol; j++)//遍历末行各单元格，注意COM编程时下标从1开始
            {
                //如果存在无边框单元格
                if (rng[nRow, j + 1].Borders[XlBordersIndex.xlEdgeBottom].LineStyle.Equals(XlLineStyle.xlLineStyleNone))
                {
                    edP = tableWidths.GetSumAt(j - 1);//末端坐标移到无边框格起点
                    if (edP != stP) //如果起终点不重合则绘制框线
                    {
                        Line line = new Line(new Point3d(stP, -tableH, 0), new Point3d(edP, -tableH, 0));
                        line.Color = Excel2CADSettings.e2cOptions.OuterLineColor;
                        db.AddToModelSpace(line);
                        entIds.Add(line.ObjectId);
                    }
                    stP = tableWidths.GetSumAt(j);
                    edP = tableW;
                    flag = true;
                    continue;
                }
            }
            if (flag == false)//如果没有无边框格
            {
                Line line = new Line(new Point3d(0, -tableH, 0), new Point3d(tableW, -tableH, 0));
                line.Color = Excel2CADSettings.e2cOptions.OuterLineColor;
                db.AddToModelSpace(line);
                entIds.Add(line.ObjectId);
            }
            //2.1.3 左边线
            stP = 0;//起始位置
            edP = tableH;//结束位置
            flag = false;//是否有无边线格
            for (int i = 0; i < nRow; i++)//遍历首列各单元格，注意COM编程时下标从1开始
            {
                //如果存在无边框单元格
                if (rng[i + 1, 1].Borders[XlBordersIndex.xlEdgeLeft].LineStyle.Equals(XlLineStyle.xlLineStyleNone))
                {
                    edP = tableHeights.GetSumAt(i - 1);//末端坐标移到无边框格起点
                    if (edP != stP) //如果起终点不重合则绘制框线
                    {
                        Line line = new Line(new Point3d(0, -stP, 0), new Point3d(0, -edP, 0));
                        line.Color = Excel2CADSettings.e2cOptions.OuterLineColor;
                        db.AddToModelSpace(line);
                        entIds.Add(line.ObjectId);
                    }
                    stP = tableHeights.GetSumAt(i);
                    edP = tableH;
                    flag = true;
                    continue;
                }
            }
            if (flag == false)//如果没有无边框格
            {
                Line line = new Line(new Point3d(0, 0, 0), new Point3d(0, -tableH, 0));
                line.Color = Excel2CADSettings.e2cOptions.OuterLineColor;
                db.AddToModelSpace(line);
                entIds.Add(line.ObjectId);
            }
            //2.1.4 右边线
            stP = 0;//起始位置
            edP = tableH;//结束位置
            flag = false;//是否有无边线格
            for (int i = 0; i < nRow; i++)//遍历末列各单元格，注意COM编程时下标从1开始
            {
                //如果存在无边框单元格
                if (rng[i + 1, nCol].Borders[XlBordersIndex.xlEdgeRight].LineStyle.Equals(XlLineStyle.xlLineStyleNone))
                {
                    edP = tableHeights.GetSumAt(i - 1);//末端坐标移到无边框格起点
                    if (edP != stP) //如果起终点不重合则绘制框线
                    {
                        Line line = new Line(new Point3d(tableW, -stP, 0), new Point3d(tableW, -edP, 0));
                        line.Color = Excel2CADSettings.e2cOptions.OuterLineColor;
                        db.AddToModelSpace(line);
                        entIds.Add(line.ObjectId);
                    }
                    stP = tableHeights.GetSumAt(i);
                    edP = tableH;
                    flag = true;
                    continue;
                }
            }
            if (flag == false)//如果没有无边框格
            {
                Line line = new Line(new Point3d(tableW, 0, 0), new Point3d(tableW, -tableH, 0));
                line.Color = Excel2CADSettings.e2cOptions.OuterLineColor;
                db.AddToModelSpace(line);
                entIds.Add(line.ObjectId);
            }
            //2.2 绘制内框行线
            //2.2.1 绘制行线
            for (int i = 1; i < nRow; i++)//遍历中间各行
            {
                stP = 0;//起始位置
                edP = tableW;//结束位置
                flag = false;//是否有无边线格
                for (int j = 0; j < nCol; j++)//遍历每行各单元格，注意COM编程时下标从1开始
                {
                    //如果存在无边框单元格,或者本单元格为合并单元格的中间格
                    if (rng[i, j + 1].Borders[XlBordersIndex.xlEdgeBottom].LineStyle.Equals(XlLineStyle.xlLineStyleNone)
                        && rng[i + 1, j + 1].Borders[XlBordersIndex.xlEdgeTop].LineStyle.Equals(XlLineStyle.xlLineStyleNone)
                        || rng[i + 1, j + 1].MergeCells.Equals(true) && rng[i + 1, j + 1].Row != rng[i + 1, j + 1].MergeArea.Row)
                    {
                        edP = tableWidths.GetSumAt(j - 1);//末端坐标移到无边框格起点
                        if (edP != stP) //如果起终点不重合则绘制框线
                        {
                            Line line = new Line(new Point3d(stP, -tableHeights.GetSumAt(i - 1), 0), new Point3d(edP, -tableHeights.GetSumAt(i - 1), 0));
                            line.Color = Excel2CADSettings.e2cOptions.InnerLineColor;
                            db.AddToModelSpace(line);
                            entIds.Add(line.ObjectId);
                        }
                        stP = tableWidths.GetSumAt(j + rng[i + 1, j + 1].MergeArea.Columns.Count - 1);
                        edP = tableW;
                        flag = true;
                        continue;
                    }
                }
                if (edP != stP) //如果起终点不重合则绘制框线
                {
                    Line line = new Line(new Point3d(stP, -tableHeights.GetSumAt(i - 1), 0), new Point3d(edP, -tableHeights.GetSumAt(i - 1), 0));
                    line.Color = Excel2CADSettings.e2cOptions.InnerLineColor;
                    db.AddToModelSpace(line);
                    entIds.Add(line.ObjectId);
                }
            }
            //2.2.2 绘制列线
            for (int j = 1; j < nCol; j++)//遍历中间各行
            {
                stP = 0;//起始位置
                edP = tableH;//结束位置
                flag = false;//是否有无边线格
                for (int i = 0; i < nRow; i++)//遍历每列各单元格，注意COM编程时下标从1开始
                {
                    //如果存在无边框单元格
                    if (rng[i + 1, j].Borders[XlBordersIndex.xlEdgeRight].LineStyle.Equals(XlLineStyle.xlLineStyleNone)
                        && rng[i + 1, j + 1].Borders[XlBordersIndex.xlEdgeLeft].LineStyle.Equals(XlLineStyle.xlLineStyleNone)
                        || rng[i + 1, j + 1].MergeCells.Equals(true) && rng[i + 1, j + 1].Column != rng[i + 1, j + 1].MergeArea.Column)
                    {
                        edP = tableHeights.GetSumAt(i - 1);//末端坐标移到无边框格起点
                        if (edP != stP) //如果起终点不重合则绘制框线
                        {
                            Line line = new Line(new Point3d(tableWidths.GetSumAt(j - 1), -stP, 0), new Point3d(tableWidths.GetSumAt(j - 1), -edP, 0));
                            line.Color = Excel2CADSettings.e2cOptions.InnerLineColor;
                            db.AddToModelSpace(line);
                            entIds.Add(line.ObjectId);
                        }
                        stP = tableHeights.GetSumAt(i + rng[i + 1, j + 1].MergeArea.Rows.Count - 1);
                        edP = tableH;
                        flag = true;
                        continue;
                    }
                }
                if (edP != stP) //如果起终点不重合则绘制框线
                {
                    Line line = new Line(new Point3d(tableWidths.GetSumAt(j - 1), -stP, 0), new Point3d(tableWidths.GetSumAt(j - 1), -edP, 0));
                    line.Color = Excel2CADSettings.e2cOptions.InnerLineColor;
                    db.AddToModelSpace(line);
                    entIds.Add(line.ObjectId);
                }
            }
            #endregion
            #region 3.绘制标题
            DBText title = new DBText();
            title.TextString = Excel2CADSettings.e2cOptions.Title;//内容                
            title.Height = 1.5 * Excel2CADSettings.e2cOptions.TextHeight
                    * Excel2CADSettings.e2cOptions.Scale;//标题高度为文字高度的1.5倍               
            title.TextStyleId = db.Textstyle;//样式为当前样式                
            title.WidthFactor = Excel2CADSettings.e2cOptions.TextWidthFactor;//宽度系数
            title.HorizontalMode = TextHorizontalMode.TextMid;
            title.VerticalMode = TextVerticalMode.TextBottom;
            title.AlignmentPoint = new Point3d(tableW / 2, 5 * Excel2CADSettings.e2cOptions.Scale, 0);//标题位置为中部向上5个单位
            db.AddToModelSpace(title);
            entIds.Add(title.ObjectId);
            #endregion
            return entIds;
        }
        /// <summary>
        /// 返回单元格在合并单元格中的位置
        /// </summary>
        /// <param name="rng">待检验的区域</param>
        /// <returns>位置枚举</returns>
        public static PosInMergeRange PositionInMergeRange(this Excel.Range rng)
        {
            PosInMergeRange pos = PosInMergeRange.LeftTop;
            if (rng.MergeCells.Equals(false))//不包含合并单元格
                pos = PosInMergeRange.NotMergeRange;
            else if (rng.Row == rng.MergeArea.Row && rng.Column == rng.MergeArea.Column)//左上角
                pos = PosInMergeRange.LeftTop;
            else if (rng.Row == rng.MergeArea.Row &&
                rng.Column == rng.MergeArea.Column + rng.MergeArea.Columns.Count)//右上角
                pos = PosInMergeRange.RightTop;
            else if (rng.Row == rng.MergeArea.Row + rng.MergeArea.Rows.Count &&
                rng.Column == rng.MergeArea.Column)//左下角
                pos = PosInMergeRange.LeftBottom;
            else if (rng.Row == rng.MergeArea.Row + rng.MergeArea.Rows.Count &&
                rng.Column == rng.MergeArea.Column + rng.MergeArea.Columns.Count)//右下角
                pos = PosInMergeRange.RightBottom;
            else if (rng.Row == rng.MergeArea.Row + rng.MergeArea.Rows.Count ||
                rng.Row == rng.MergeArea.Row ||
                rng.Column == rng.MergeArea.Column + rng.MergeArea.Columns.Count ||
                rng.Column == rng.MergeArea.Column)//非角点边部单元
                pos = PosInMergeRange.EdgeMiddle;
            else//中间单元
                pos = PosInMergeRange.Middle;
            return pos;
        }
    }
}
