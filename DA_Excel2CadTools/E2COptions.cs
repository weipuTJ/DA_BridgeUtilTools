using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace DA_Excel2CadTools
{
    /// <summary>
    /// Excel to CAD options
    /// </summary>
    [Serializable]
    public class E2COptions:INotifyPropertyChanged
    {
        /// <summary>
        /// 表格标题
        /// </summary>
        private string title="";
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        /// <summary>
        /// 制表比例
        /// </summary>
        private double scale = 1;
        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                OnPropertyChanged(nameof(Scale));
            }
        }
        /// <summary>
        /// 插入点
        /// </summary>
        private string insertPt = "左上";
        public string InsertPt
        {
            get { return insertPt; }
            set
            {
                insertPt = value;
                OnPropertyChanged(nameof(InsertPt));
            }
        }
        /// <summary>
        /// 外框颜色
        /// </summary>
        private Color outerLineColor = Color.FromColorIndex(ColorMethod.ByLayer, 3);
        public Color OuterLineColor
        {
            get { return outerLineColor; }
            set
            {
                outerLineColor = value;
                OnPropertyChanged(nameof(OuterLineColor));
            }
        }
        /// <summary>
        /// 内框颜色
        /// </summary>
        private Color innerLineColor = Color.FromColorIndex(ColorMethod.ByLayer, 3);
        public Color InnerLineColor
        {
            get { return innerLineColor; }
            set
            {
                innerLineColor = value;
                OnPropertyChanged(nameof(InnerLineColor));
            }
        }
        /// <summary>
        /// 字高
        /// </summary>
        private double textHeight = 3;
        public double TextHeight
        {
            get { return textHeight; }
            set
            {
                textHeight = value;
                OnPropertyChanged(nameof(TextHeight));
            }
        }
        /// <summary>
        /// 文字宽度系数
        /// </summary>
        private double textWidthFactor = 0.7;
        public double TextWidthFactor
        {
            get { return textWidthFactor; }
            set
            {
                textWidthFactor = value;
                OnPropertyChanged(nameof(TextWidthFactor));
            }
        }
        /// <summary>
        /// 行自动
        /// </summary>
        private bool rowAuto = true;
        public bool RowAuto
        {
            get { return rowAuto; }
            set
            {
                rowAuto = value;
                OnPropertyChanged(nameof(RowAuto));
            }
        }
        /// <summary>
        /// 标题行高
        /// </summary>
        private double headerRowHeight = 5;
        public double HeaderRowHeight
        {
            get { return headerRowHeight; }
            set
            {
                headerRowHeight = value;
                OnPropertyChanged(nameof(HeaderRowHeight));
            }
        }
        /// <summary>
        /// 内容行高
        /// </summary>
        private double contentRowHeight = 3.5;
        public double ContentRowHeight
        {
            get { return contentRowHeight; }
            set
            {
                contentRowHeight = value;
                OnPropertyChanged(nameof(contentRowHeight));
            }
        }
        /// <summary>
        /// 列自动
        /// </summary>
        private bool columnAuto = true;
        public bool ColumnAuto
        {
            get { return columnAuto; }
            set
            {
                columnAuto = value;
                OnPropertyChanged(nameof(ColumnAuto));
            }
        }
        /// <summary>
        /// 列设置
        /// </summary>
        private ObservableCollection<ColumnOptions> colOptList = null;
        public ObservableCollection<ColumnOptions> ColOptList
        {
            get { return colOptList; }
            set
            {
                colOptList = value;
                OnPropertyChanged(nameof(ColOptList));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
