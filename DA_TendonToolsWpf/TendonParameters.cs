using System;

namespace DA_TendonToolsWpf
{
    public class TendonParameters
    {
        /// <summary>
        /// 钢束名称
        /// </summary>
        public string TdName { get; set; }
        /// <summary>
        /// 钢束规格
        /// </summary>
        public string TdStyle { get; set; }
        /// <summary>
        /// 钢束数量
        /// </summary>
        public int TdNum { get; set; }
        /// <summary>
        /// 管道直径（mm）
        /// </summary>
        public double TdPipeDia { get; set; }
        /// <summary>
        /// 钢束张拉方式
        /// </summary>
        public TendonDrawStyle TdDrawStyle { get; set; }
        /// <summary>
        /// 默认构造函数，各属性获得默认值
        /// </summary>
        public TendonParameters()
        {
            TdName = "Unnamed";
            TdStyle = "Φ15-12";
            TdNum = 1;
            TdPipeDia = 90;
            TdDrawStyle = TendonDrawStyle.Both;
        }
    }
    public enum TendonDrawStyle:int
    {
        Left = -1,
        Both = 0,
        Right = 1
    }
}
