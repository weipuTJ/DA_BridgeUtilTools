using System;
using Autodesk.AutoCAD.DatabaseServices;
using System.ComponentModel;

namespace DA_TendonToolsWpf
{
    public class Tendon:INotifyPropertyChanged
    {
        /// <summary>
        /// 钢束名称
        /// </summary>
        private string tdName = "F1";
        public string TdName
        {
            get { return tdName; }
            set { tdName = value; OnPropertyChanged(nameof(TdName)); }
        }
        /// <summary>
        /// 钢束规格
        /// </summary>
        private string tdStyle = "Φ15-12";
        public string TdStyle
        {
            get { return tdStyle; }
            set { tdStyle = value; OnPropertyChanged(nameof(TdStyle)); }
        }
        /// <summary>
        /// 钢束数量
        /// </summary>
        private int tdNum = 1;
        public int TdNum
        {
            get { return tdNum; }
            set { tdNum = value; OnPropertyChanged(nameof(TdNum)); }
        }
        /// <summary>
        /// 管道直径（mm）
        /// </summary>
        private double tdPipeDia = 90;
        public double TdPipeDia
        {
            get { return tdPipeDia; }
            set { tdPipeDia = value; OnPropertyChanged(nameof(TdPipeDia)); }
        }
        /// <summary>
        /// 钢束张拉方式
        /// </summary>
        private TendonDrawStyle tdDrawStyle = TendonDrawStyle.Both;
        public TendonDrawStyle TdDrawStyle
        {
            get { return tdDrawStyle; }
            set { tdDrawStyle = value; OnPropertyChanged(nameof(TdDrawStyle)); }
        }
        /// <summary>
        /// 左端是否张拉
        /// </summary>
        public bool IsLeftDraw
        {
            get
            {
                if (tdDrawStyle != TendonDrawStyle.Right)
                    return true;
                else
                    return false;
            }
            set { }
        }
        /// <summary>
        /// 右端是否张拉
        /// </summary>
        public bool IsRightDraw
        {
            get
            {
                if (tdDrawStyle != TendonDrawStyle.Left)
                    return true;
                else
                    return false;
            }
            set { }
        }
        /// <summary>
        /// 左侧张拉量
        /// </summary>
        public double LeftDrawAmount
        {
            get
            {
                switch (tdDrawStyle)
                {
                    case TendonDrawStyle.Left://左侧张拉
                        return TdLine.SingleDrawAmount(
                            ctrlStress: TendonGeneralParameters.CtrlStress,
                            kii: TendonGeneralParameters.Kii,
                            miu: TendonGeneralParameters.Miu,
                            drawEnd: -1,
                            Ep: TendonGeneralParameters.Ep
                            );
                    case TendonDrawStyle.Right://右侧张拉
                        return 0;
                    default://两端张拉
                        return TdLine.BothDrawAmount(
                            ctrlStress: TendonGeneralParameters.CtrlStress,
                            kii: TendonGeneralParameters.Kii,
                            miu: TendonGeneralParameters.Miu,
                            Ep: TendonGeneralParameters.Ep
                            )[0];
                }
            }
            set { }
        }
        /// <summary>
        /// 右侧张拉量
        /// </summary>
        public double RightDrawAmount
        {
            get
            {
                switch (tdDrawStyle)
                {
                    case TendonDrawStyle.Right://右侧张拉
                        return TdLine.SingleDrawAmount(
                            ctrlStress: TendonGeneralParameters.CtrlStress,
                            kii: TendonGeneralParameters.Kii,
                            miu: TendonGeneralParameters.Miu,
                            drawEnd: 1,
                            Ep: TendonGeneralParameters.Ep
                            );
                    case TendonDrawStyle.Left://左侧张拉
                        return 0;
                    default://两端张拉
                        return TdLine.BothDrawAmount(
                            ctrlStress: TendonGeneralParameters.CtrlStress,
                            kii: TendonGeneralParameters.Kii,
                            miu: TendonGeneralParameters.Miu,
                            Ep: TendonGeneralParameters.Ep
                            )[1];
                }
            }
            set { }
        }
        /// <summary>
        /// 钢束隐藏标识
        /// </summary>
        private string tdKey = "TendonHdl_";
        public string TdKey
        {
            get { return tdKey; }
            set { }
        }
        /// <summary>
        /// 钢束线的ObjectId
        /// </summary>
        private ObjectId tdId = ObjectId.Null;
        public ObjectId TdId
        {
            get { return tdId; }
            set
            {
                tdId = value;
                tdKey = "TendonHdl_" + tdId.Handle.ToString();
                OnPropertyChanged(nameof(TdId));
            }
        }
        /// <summary>
        /// 钢束对应的多段线
        /// </summary>
        public Polyline TdLine
        {
            get
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Polyline tdLine = new Polyline();
                using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
                {
                    tdLine = tdId.GetObject(OpenMode.ForRead) as Polyline;
                    trans.Commit();//执行事务处理
                }
                return tdLine;
            }
            set { }
        }
        /// <summary>
        /// 钢束净长
        /// </summary>
        public double TdNetLen
        {
            get { return TdLine.Length; }
            set { }
        }
        public double TdTotalLen
        {
            get
            {
                switch (tdDrawStyle)
                {
                    case TendonDrawStyle.Right://右侧张拉
                    case TendonDrawStyle.Left://左侧张拉
                        return TdNetLen + TendonGeneralParameters.WorkLen;
                    default://两端张拉
                        return TdNetLen + 2 * TendonGeneralParameters.WorkLen;
                }
            }
            set { }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public enum TendonDrawStyle : int
    {
        Left = -1,
        Both = 0,
        Right = 1
    }
}
